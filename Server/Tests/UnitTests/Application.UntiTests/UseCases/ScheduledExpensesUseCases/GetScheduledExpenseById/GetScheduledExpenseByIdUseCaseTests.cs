using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.UseCases.ScheduledExpensesUseCases.GetScheduledExpenseById;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using FakeItEasy;

namespace UnitTests.Application.UntiTests.UseCases.ScheduledExpensesUseCases.GetScheduledExpenseById
{
    public class GetScheduledExpenseByIdUseCaseTests
    {
        private readonly IScheduledExpensesRepository _repository;
        private readonly GetScheduledExpenseByIdUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ScheduledExpenseId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
        private static readonly Guid CategoryId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");

        public GetScheduledExpenseByIdUseCaseTests()
        {
            _repository = A.Fake<IScheduledExpensesRepository>();
            _sut = new GetScheduledExpenseByIdUseCase(_repository, A.Fake<Microsoft.Extensions.Logging.ILogger<GetScheduledExpenseByIdUseCase>>());
        }

        [Fact]
        public async Task Execute_WhenScheduledExpenseNotFound_ShouldReturnFailure()
        {
            A.CallTo(() => _repository.FindIncludingCategoriesAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns((ScheduledExpense?)null);

            var result = await _sut.Execute(UserId, ScheduledExpenseId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenNotOwnedByUser_ShouldReturnFailure()
        {
            var expense = CreateExpense(OtherUserId);
            A.CallTo(() => _repository.FindIncludingCategoriesAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(UserId, ScheduledExpenseId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenOwnedByUser_ShouldReturnDto()
        {
            var expense = CreateExpense(UserId);
            A.CallTo(() => _repository.FindIncludingCategoriesAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(UserId, ScheduledExpenseId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
        }



        private static ScheduledExpense CreateExpense(Guid userId)
        {
            var expense = ScheduledExpense.Create(userId, "Rent", 1500m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            var category = ExpenseCategory.Create("HOUSING", "Housing", "Housing expenses");
            typeof(ScheduledExpense).GetProperty("Category")!.SetValue(expense, category);
            return expense;
        }
    }
}
