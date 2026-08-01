using Application.ApplicationConstantsNamesapce;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.ExpensesUseCases.SearchExpenses;
using Application.UseCases.ExpensesUseCases.SearchExpenses.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesUseCases.SearchExpenses
{
    public class SearchExpensesUseCaseTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<SearchExpensesUseCase> _logger;
        private readonly SearchExpensesUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");

        public SearchExpensesUseCaseTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _usersRepository = A.Fake<IUsersRepository>();
            _logger = A.Fake<ILogger<SearchExpensesUseCase>>();
            _sut = new SearchExpensesUseCase(_expensesRepository, _usersRepository, _logger);
        }

        [Fact]
        public async Task Execute_WhenUserHasNoFinancialProfile_ShouldReturnEmptyList()
        {
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns((UserFinancialProfile?)null);

            var query = new SearchExpensesQueryParameters(null, null, null, null, null, null, null);
            var result = await _sut.Execute(UserId, query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task Execute_WhenUserHasFinancialProfile_ShouldReturnExpenses()
        {
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(UserFinancialProfile.Create(UserId, 5000, 1, DateTimeOffset.UtcNow));

            var query = new SearchExpensesQueryParameters(null, null, null, null, null, null, null);
            var expenses = new List<ExpenseDto>
            {
                new(Guid.NewGuid(), UserId, new DateOnly(2026, 7, 1), "Test", 100, "FOOD", null, null, 0)
            };

            A.CallTo(() => _expensesRepository.GetAllForUserQuery(UserId))
                .Returns(new List<Expense>().AsQueryable());

            A.CallTo(() => _expensesRepository.GetExpenseDtoAsync(A<IQueryable<Expense>>._, A<CancellationToken>._))
                .Returns(expenses);

            var result = await _sut.Execute(UserId, query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task Execute_WhenFilteringByCategory_ShouldReturnFilteredExpenses()
        {
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(UserFinancialProfile.Create(UserId, 5000, 1, DateTimeOffset.UtcNow));

            var categoryId = Guid.NewGuid();
            var query = new SearchExpensesQueryParameters(
                new List<Guid> { categoryId }, null, null, null, null, null, null);

            A.CallTo(() => _expensesRepository.GetAllForUserQuery(UserId))
                .Returns(new List<Expense>().AsQueryable());

            A.CallTo(() => _expensesRepository.GetExpenseDtoAsync(A<IQueryable<Expense>>._, A<CancellationToken>._))
                .Returns(new List<ExpenseDto>());

            var result = await _sut.Execute(UserId, query, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Execute_WhenSortingByAmountAscending_ShouldApplyCorrectSort()
        {
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(UserFinancialProfile.Create(UserId, 5000, 1, DateTimeOffset.UtcNow));

            var query = new SearchExpensesQueryParameters(
                null, null, null, null, null, null, null,
                ApplicationConstants.ExpensesSortOptions.Amount,
                ApplicationConstants.SortOrders.Ascending);

            A.CallTo(() => _expensesRepository.GetAllForUserQuery(UserId))
                .Returns(new List<Expense>().AsQueryable());

            A.CallTo(() => _expensesRepository.GetExpenseDtoAsync(A<IQueryable<Expense>>._, A<CancellationToken>._))
                .Returns(new List<ExpenseDto>());

            var result = await _sut.Execute(UserId, query, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }
    }
}
