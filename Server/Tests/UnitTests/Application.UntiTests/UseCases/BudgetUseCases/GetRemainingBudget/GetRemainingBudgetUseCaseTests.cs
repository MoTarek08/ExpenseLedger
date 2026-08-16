using Application.Interfaces.BusinessQueries;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.BudgetUseCases.GetRemainingBudget;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.BudgetUseCases.GetRemainingBudget
{
    public class GetRemainingBudgetUseCaseTests
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IDateProvider _dateProvider;
        private readonly IBudgetQueries _budgetQueries;
        private readonly ILogger<GetRemainingBudgetUseCase> _logger;

        private readonly GetRemainingBudgetUseCase _sut;

        private readonly Guid _userId;
        private readonly DateOnly _today;
        private readonly UserFinancialProfile _profile;

        public GetRemainingBudgetUseCaseTests()
        {
            _usersRepository = A.Fake<IUsersRepository>();
            _dateProvider = A.Fake<IDateProvider>();
            _budgetQueries = A.Fake<IBudgetQueries>();
            _logger = A.Fake<ILogger<GetRemainingBudgetUseCase>>();

            _sut = new GetRemainingBudgetUseCase(
                _usersRepository,
                _dateProvider,
                _budgetQueries,
                _logger);

            _userId = Guid.NewGuid();
            _today = new DateOnly(2026, 7, 15);
            _profile = UserFinancialProfile.Create(_userId, 5000m, 1, DateTimeOffset.UtcNow);

            A.CallTo(() => _dateProvider.Now)
                .Returns(new DateTimeOffset(_today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
        }

        [Fact]
        public async Task Execute_WhenBudgetIsPartiallySpent_ShouldReturnRemainingAmount()
        {
            decimal spent = 1200m;
            decimal expected = _profile.MonthlyNetIncome - spent;

            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(_profile);
            A.CallTo(() => _budgetQueries.GetTotalSpentAsync(_userId, A<DateOnly>._, A<DateOnly>._, A<CancellationToken>._))
                .Returns(spent);

            var result = await _sut.Execute(_userId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.Data);
        }

        [Fact]
        public async Task Execute_WhenNoExpensesInPeriod_ShouldReturnFullIncome()
        {
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(_profile);
            A.CallTo(() => _budgetQueries.GetTotalSpentAsync(_userId, A<DateOnly>._, A<DateOnly>._, A<CancellationToken>._))
                .Returns(0m);

            var result = await _sut.Execute(_userId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(_profile.MonthlyNetIncome, result.Data);
        }

        [Fact]
        public async Task Execute_WhenSpendingExceedsIncome_ShouldReturnNegativeRemaining()
        {
            decimal spent = 6000m;

            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(_profile);
            A.CallTo(() => _budgetQueries.GetTotalSpentAsync(_userId, A<DateOnly>._, A<DateOnly>._, A<CancellationToken>._))
                .Returns(spent);

            var result = await _sut.Execute(_userId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.True(result.Data < 0);
        }
    }
}
