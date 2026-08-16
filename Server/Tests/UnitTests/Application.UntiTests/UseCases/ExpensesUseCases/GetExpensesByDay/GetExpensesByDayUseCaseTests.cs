using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.ExpensesUseCases.GetExpensesByDay;
using Application.UseCases.ExpensesUseCases.GetExpensesByDay.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Domain.Entities.ExpenseNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesUseCases.GetExpensesByDay
{
    public class GetExpensesByDayUseCaseTests
    {
        private readonly IExpensesRepository _repository;
        private readonly ILogger<GetExpensesByDayUseCase> _logger;
        private readonly GetExpensesByDayUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly DateOnly Day = new(2026, 7, 22);
        private static readonly Guid CategoryId = Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93");

        public GetExpensesByDayUseCaseTests()
        {
            _repository = A.Fake<IExpensesRepository>();
            _logger = A.Fake<ILogger<GetExpensesByDayUseCase>>();
            _sut = new GetExpensesByDayUseCase(_repository, _logger);
        }

        private Expense CreateExpense(decimal amount) =>
            Expense.CreateManualExpense(UserId, CategoryId, "Test", amount, Day, DateTimeOffset.UtcNow);

        private List<ExpenseDto> ToDtos(IQueryable<Expense> query) =>
            query.Select(e => new ExpenseDto(
                e.Id, e.UserId, e.SpentOn, e.Title, e.Amount,
                "FOOD", null, e.ScheduledExpenseId, 0)).ToList();

        [Fact]
        public async Task Execute_WhenExpensesExist_ShouldReturnList()
        {
            var expenses = new List<Expense> { CreateExpense(100) };

            A.CallTo(() => _repository.GetAllForUserInDayQuery(UserId, Day))
                .Returns(expenses.AsQueryable());
            A.CallTo(() => _repository.ToExpenseDtoAsync(A<IQueryable<Expense>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<Expense> q, CancellationToken _) =>
                    Task.FromResult(ToDtos(q)));

            var request = new GetExpensesByDayRequestModel(Day);
            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task Execute_WhenNoExpenses_ShouldReturnEmptyList()
        {
            A.CallTo(() => _repository.GetAllForUserInDayQuery(UserId, Day))
                .Returns(new List<Expense>().AsQueryable());
            A.CallTo(() => _repository.ToExpenseDtoAsync(A<IQueryable<Expense>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<Expense> q, CancellationToken _) =>
                    Task.FromResult(ToDtos(q)));

            var request = new GetExpensesByDayRequestModel(Day);
            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task Execute_WhenPageSizeExceedsRemainingRows_ShouldReturnAllRows()
        {
            var expenses = new List<Expense>
            {
                CreateExpense(10),
                CreateExpense(20),
                CreateExpense(30),
                CreateExpense(40),
                CreateExpense(50),
            };

            A.CallTo(() => _repository.GetAllForUserInDayQuery(UserId, Day))
                .Returns(expenses.AsQueryable());
            A.CallTo(() => _repository.ToExpenseDtoAsync(A<IQueryable<Expense>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<Expense> q, CancellationToken _) =>
                    Task.FromResult(ToDtos(q)));

            var request = new GetExpensesByDayRequestModel(Day)
            {
                PageSize = 10,
                PageNumber = 1
            };
            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(5, result.Data.Count);
        }
    }
}
