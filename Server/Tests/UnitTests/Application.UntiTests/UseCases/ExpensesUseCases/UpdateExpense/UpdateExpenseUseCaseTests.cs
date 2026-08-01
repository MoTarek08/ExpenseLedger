using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Interfaces.SharedServices;
using Application.UseCases.ExpensesUseCases.UpdateExpense;
using Application.UseCases.ExpensesUseCases.UpdateExpense.Models;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesUseCases.UpdateExpense
{
    public class UpdateExpenseUseCaseTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckBudgetStateService _checkBudget;
        private readonly ILogger<UpdateExpenseUseCase> _logger;
        private readonly UpdateExpenseUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ExpenseId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
        private static readonly Guid CategoryId = Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93");
        private static readonly Guid SubCategoryId = Guid.Parse("4bf4e511-9194-429b-9968-0bc1295b0fd5");

        public UpdateExpenseUseCaseTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _categoriesRepository = A.Fake<ICategoriesRepository>();
            _usersRepository = A.Fake<IUsersRepository>();
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _backgroundJobsService = A.Fake<IBackgroundJobsService>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            _checkBudget = A.Fake<ICheckBudgetStateService>();
            A.CallTo(() => _checkBudget.EvaluateAsync(A<Guid>._, A<CancellationToken>._))
                .Returns((Domain.Entities.Notification.Notification?)null);

            _logger = A.Fake<ILogger<UpdateExpenseUseCase>>();
            _sut = new UpdateExpenseUseCase(
                _expensesRepository,
                _categoriesRepository,
                _usersRepository,
                _notificationsRepository,
                _dateTimeProvider,
                _backgroundJobsService,
                _unitOfWork,
                _checkBudget,
                _logger);
        }

        private Expense CreateTestExpense(DateOnly? spentOn = null) =>
            Expense.CreateManualExpense(
                UserId,
                CategoryId,
                "Test",
                100,
                spentOn ?? new DateOnly(2026, 7, 1),
                DateTimeOffset.UtcNow);

        private UserFinancialProfile CreateProfile() =>
            UserFinancialProfile.Create(UserId, 5000, 1, DateTimeOffset.UtcNow);

        [Fact]
        public async Task Execute_WhenExpenseNotFound_ShouldReturnNotFound()
        {
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns((Expense?)null);

            var request = new UpdateExpenseRequestModel("New title", null, null, null, null);
            var result = await _sut.Execute(UserId, ExpenseId, request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenExpenseNotOwned_ShouldReturnNotFound()
        {
            var expense = Expense.CreateManualExpense(
                OtherUserId,
                CategoryId,
                "Test",
                100,
                new DateOnly(2026, 7, 1),
                DateTimeOffset.UtcNow);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var request = new UpdateExpenseRequestModel("New title", null, null, null, null);
            var result = await _sut.Execute(UserId, ExpenseId, request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenSpentOnChanged_ShouldDetectChangeAndUpdate()
        {
            var originalSpentOn = new DateOnly(2026, 7, 1);
            var newSpentOn = new DateOnly(2026, 7, 15);
            var expense = CreateTestExpense(originalSpentOn);
            var profile = CreateProfile();

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);
            A.CallTo(() => _dateTimeProvider.Now)
                .Returns(new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.Zero));

            var request = new UpdateExpenseRequestModel(null, null, null, null, newSpentOn);
            var result = await _sut.Execute(UserId, ExpenseId, request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(newSpentOn, expense.SpentOn);
        }

        [Fact]
        public async Task Execute_WhenCategoryChangesAndSubCategoryNotBelong_ShouldReturnFailure()
        {
            var expense = CreateTestExpense();
            var newCategoryId = Guid.NewGuid();

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _categoriesRepository.SubBelongsToMainAsync(newCategoryId, SubCategoryId, A<CancellationToken>._))
                .Returns(false);

            var request = new UpdateExpenseRequestModel(null, null, newCategoryId, SubCategoryId, null);
            var result = await _sut.Execute(UserId, ExpenseId, request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER, result.Error!.Code);
        }
    }
}
