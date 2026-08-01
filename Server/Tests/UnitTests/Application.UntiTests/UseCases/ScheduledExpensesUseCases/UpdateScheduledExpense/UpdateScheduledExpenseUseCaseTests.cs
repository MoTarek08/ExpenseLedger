using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.ScheduledExpenseNamespace;
using Domain.ExceptionsNamespace;
using FakeItEasy;

namespace UnitTests.Application.UntiTests.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense
{
    public class UpdateScheduledExpenseUseCaseTests
    {
        private readonly IScheduledExpensesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly UpdateScheduledExpenseUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ScheduledExpenseId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
        private static readonly Guid CategoryId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");

        public UpdateScheduledExpenseUseCaseTests()
        {
            _repository = A.Fake<IScheduledExpensesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _backgroundJobsService = A.Fake<IBackgroundJobsService>();
            _sut = new UpdateScheduledExpenseUseCase(
                _repository,
                _unitOfWork,
                _backgroundJobsService,
                A.Fake<Microsoft.Extensions.Logging.ILogger<UpdateScheduledExpenseUseCase>>());
        }

        private ScheduledExpense CreateActiveExpense(Guid userId, Guid? categoryId = null)
        {
            return ScheduledExpense.Create(userId, "Test", 100m, categoryId ?? CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Execute_WhenNotFound_ShouldReturnFailure()
        {
            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns((ScheduledExpense?)null);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, new UpdateScheduledExpenseRequestModel(Title: "Updated", null, null, null), default);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenNotOwnedByUser_ShouldReturnFailure()
        {
            var expense = CreateActiveExpense(OtherUserId);
            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, new UpdateScheduledExpenseRequestModel(Title: "Updated", null, null, null), default);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenInactive_ShouldReturnFailure()
        {
            var expense = CreateActiveExpense(UserId);
            expense.Cancel();
            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, new UpdateScheduledExpenseRequestModel(Title: "Updated", null, null, null), default);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_ACTIVE, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenProcessedBeforeAndChangingFirstDue_ShouldReturnFailure()
        {
            var expense = CreateActiveExpense(UserId);
            typeof(ScheduledExpense).GetProperty("LastProcessedAt")!.SetValue(expense, new DateOnly(2026, 7, 1));

            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, new UpdateScheduledExpenseRequestModel(null, null, new DateOnly(2026, 9, 1), null), default);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_PROCESSED_BEFORE_AND_CANNOT_CHANGE_FIRST_DUE, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenOnlyTitleChanged_ShouldNotRescheduleJob()
        {
            var expense = CreateActiveExpense(UserId);
            var originalNextDueOn = expense.NextDueOn;

            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, new UpdateScheduledExpenseRequestModel(Title: "Updated Title", null, null, null), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(originalNextDueOn, expense.NextDueOn);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                A<Guid>._, A<DateOnly>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenCadenceChanged_ShouldRescheduleJob()
        {
            var expense = CreateActiveExpense(UserId);

            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, new UpdateScheduledExpenseRequestModel(null, null, null, CadenceInterval.Weekly), default);

            Assert.True(result.IsSuccess);
            Assert.NotNull(expense.NextDueOn);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                expense.Id, expense.NextDueOn!.Value)).MustHaveHappenedOnceExactly();
        }
    }
}
