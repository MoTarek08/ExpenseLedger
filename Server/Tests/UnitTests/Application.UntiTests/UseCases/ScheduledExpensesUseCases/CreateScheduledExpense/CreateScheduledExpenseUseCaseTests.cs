using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.ScheduledExpenseNamespace;
using FakeItEasy;

namespace UnitTests.Application.UntiTests.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense
{
    public class CreateScheduledExpenseUseCaseTests
    {
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IDateProvider _dateProvider;
        private readonly CreateScheduledExpenseUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid CategoryId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");
        private static readonly Guid SubCategoryId = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301");

        public CreateScheduledExpenseUseCaseTests()
        {
            _scheduledExpensesRepository = A.Fake<IScheduledExpensesRepository>();
            _categoriesRepository = A.Fake<ICategoriesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _backgroundJobsService = A.Fake<IBackgroundJobsService>();
            _dateProvider = A.Fake<IDateProvider>();
            _sut = new CreateScheduledExpenseUseCase(
                _scheduledExpensesRepository,
                _categoriesRepository,
                _unitOfWork,
                _backgroundJobsService,
                _dateProvider,
                A.Fake<Microsoft.Extensions.Logging.ILogger<CreateScheduledExpenseUseCase>>());
        }

        [Fact]
        public async Task Execute_WhenSubCategoryDoesNotBelongToMain_ShouldReturnFailure()
        {
            var request = new CreateScheduledExpenseRequestModel("Test", 100m, CategoryId, SubCategoryId, CadenceInterval.Monthly, new DateOnly(2026, 8, 1));
            A.CallTo(() => _categoriesRepository.SubBelongsToMainAsync(CategoryId, SubCategoryId, A<CancellationToken>._))
                .Returns(false);

            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenValidRequest_ShouldCreateAndReturnIdWithFirstDueEqualToNextDue()
        {
            var now = DateTimeOffset.UtcNow;
            var firstDueOn = new DateOnly(2026, 8, 1);
            var request = new CreateScheduledExpenseRequestModel("Rent", 1500m, CategoryId, null, CadenceInterval.Monthly, firstDueOn);

            A.CallTo(() => _dateProvider.Now).Returns(now);

            ScheduledExpense? capturedEntry = null;
            A.CallTo(() => _scheduledExpensesRepository.Add(A<ScheduledExpense>._))
                .Invokes((ScheduledExpense entry) => capturedEntry = entry);

            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Data);
            Assert.NotNull(capturedEntry);
            Assert.Equal(capturedEntry!.FirstDueOn, capturedEntry.NextDueOn);
            A.CallTo(() => _scheduledExpensesRepository.Add(capturedEntry)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                capturedEntry.Id, capturedEntry.NextDueOn!.Value)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenDomainValidationFails_ShouldThrowDomainException()
        {
            var request = new CreateScheduledExpenseRequestModel("Test", -100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 8, 1));

            await Assert.ThrowsAsync<Domain.ExceptionsNamespace.DomainException>(() =>
                _sut.Execute(UserId, request, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Execute_WhenSubCategoryNotProvided_ShouldNotCheckCategory()
        {
            var now = DateTimeOffset.UtcNow;
            var request = new CreateScheduledExpenseRequestModel("Test", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 8, 1));

            A.CallTo(() => _dateProvider.Now).Returns(now);

            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _categoriesRepository.SubBelongsToMainAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                A<Guid>._, A<DateOnly>._)).MustHaveHappenedOnceExactly();
        }
    }
}
