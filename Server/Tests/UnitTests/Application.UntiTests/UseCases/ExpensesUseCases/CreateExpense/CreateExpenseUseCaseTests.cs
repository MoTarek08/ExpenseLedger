using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.ErrorNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.SharedServices;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.CreateExpense.Models;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Domain.Entities.ExpenseNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesUseCases.CreateExpense
{
    public class CreateExpenseUseCaseTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly ICheckBudgetStateService _budgetEvaluator;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IBuildExpenseService _buildExpenseService;
        private readonly ILogger<CreateExpenseUseCase> _logger;
        private readonly CreateExpenseUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid CategoryId = Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93");
        private static readonly Guid SubCategoryId = Guid.Parse("4bf4e511-9194-429b-9968-0bc1295b0fd5");

        public CreateExpenseUseCaseTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _backgroundJobsService = A.Fake<IBackgroundJobsService>();
            _budgetEvaluator = A.Fake<ICheckBudgetStateService>();
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _buildExpenseService = A.Fake<IBuildExpenseService>();
            _logger = A.Fake<ILogger<CreateExpenseUseCase>>();

            A.CallTo(() => _budgetEvaluator.EvaluateAsync(A<Guid>._, A<CancellationToken>._))
                .Returns((Domain.Entities.Notification.Notification?)null);

            _sut = new CreateExpenseUseCase(
                _expensesRepository,
                _unitOfWork,
                _backgroundJobsService,
                _budgetEvaluator,
                _notificationsRepository,
                _buildExpenseService,
                _logger);
        }

        [Fact]
        public async Task Execute_WhenSubCategoryDoesNotBelongToCategory_ShouldReturnFailure()
        {
            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .Returns(Result<Expense>.Failure(new Error(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER)));

            var request = new CreateExpenseRequestModel(CategoryId, "Test", 100, new DateOnly(2026, 7, 22), SubCategoryId);
            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenValidWithoutSubCategory_ShouldCreateAndTriggerJobs()
        {
            var request = new CreateExpenseRequestModel(CategoryId, "Test", 100, new DateOnly(2026, 7, 22), null);

            A.CallTo(() => _buildExpenseService.BuildExpense(UserId, request, A<CancellationToken>._))
                .Returns(Result<Expense>.Success(Expense.CreateManualExpense(
                    UserId, CategoryId, "Test", 100, new DateOnly(2026, 7, 22), DateTimeOffset.UtcNow)));

            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Data!.ExpenseId);
            A.CallTo(() => _expensesRepository.Add(A<Expense>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappened();
            A.CallTo(() => _backgroundJobsService.TriggerAfterManualExpenseCreationJobs(A<Guid>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenValidWithSubCategory_ShouldCreateExpense()
        {
            var request = new CreateExpenseRequestModel(CategoryId, "Test", 100, new DateOnly(2026, 7, 22), SubCategoryId);

            A.CallTo(() => _buildExpenseService.BuildExpense(UserId, request, A<CancellationToken>._))
                .Returns(Result<Expense>.Success(Expense.CreateManualExpense(
                    UserId, CategoryId, "Test", 100, new DateOnly(2026, 7, 22), DateTimeOffset.UtcNow, SubCategoryId)));

            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _expensesRepository.Add(A<Expense>._)).MustHaveHappenedOnceExactly();
        }
    }
}
