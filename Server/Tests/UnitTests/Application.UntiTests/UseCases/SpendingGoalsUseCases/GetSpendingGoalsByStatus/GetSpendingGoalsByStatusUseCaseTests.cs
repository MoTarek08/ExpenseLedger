using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Domain.Entities.SpendingGoalNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;

namespace UnitTests.Application.UntiTests.UseCases.SpendingGoalsUseCases.GetSpendingGoalsByStatus
{
    public class GetSpendingGoalsByStatusUseCaseTests
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ISpendingGoalsRepository _spendingGoalsRepository;
        private readonly IDateProvider _dateProvider;
        private readonly GetSpendingGoalsByStatusUseCase _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly DateOnly _today = new(2026, 7, 15);

        public GetSpendingGoalsByStatusUseCaseTests()
        {
            _usersRepository = A.Fake<IUsersRepository>();
            _spendingGoalsRepository = A.Fake<ISpendingGoalsRepository>();
            _dateProvider = A.Fake<IDateProvider>();

            _sut = new GetSpendingGoalsByStatusUseCase(
                _usersRepository,
                _spendingGoalsRepository,
                _dateProvider);

            A.CallTo(() => _dateProvider.Today).Returns(_today);
        }
        private void SetupEmptyQuery()
        {
            A.CallTo(() => _spendingGoalsRepository.GetAllForUserQuery(_userId))
                .Returns(new List<SpendingGoal>().AsQueryable());
        }

        private List<GetSpendingGoalsByStatusDto> MakeDtos(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new GetSpendingGoalsByStatusDto(
                    Guid.NewGuid(),
                    $"Goal {i}",
                    null,
                    100m,
                    500m,
                    200m,
                    _today.AddDays(-10),
                    _today.AddDays(10),
                    DateTimeOffset.UtcNow))
                .ToList();
        }

        [Fact]
        public async Task Execute_WhenStatusSucceeded_ShouldCallGetSucceededGoalsAsync()
        {
            SetupEmptyQuery();
            var expected = MakeDtos(2);

            A.CallTo(() => _spendingGoalsRepository.GetSucceededGoalsAsync(
                    A<IQueryable<SpendingGoal>>._,
                    _today,
                    A<int>._,
                    A<int>._,
                    A<CancellationToken>._))
                .Returns(expected);

            var result = await _sut.Execute(_userId, SpendingGoalStatus.Succeeded,
                new GetSpendingGoalsByStatusQueryParameters(null, null, null), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.Data);
        }

        [Fact]
        public async Task Execute_WhenStatusFailed_ShouldCallGetFailedGoalsAsync()
        {
            SetupEmptyQuery();
            var expected = MakeDtos(1);

            A.CallTo(() => _spendingGoalsRepository.GetFailedGoalsAsync(
                    A<IQueryable<SpendingGoal>>._,
                    _today,
                    A<int>._,
                    A<int>._,
                    A<CancellationToken>._))
                .Returns(expected);

            var result = await _sut.Execute(_userId, SpendingGoalStatus.Failed,
                new GetSpendingGoalsByStatusQueryParameters(null, null, null), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.Data);
        }

        [Fact]
        public async Task Execute_WhenStatusInProgress_ShouldCallGetInProgressGoalsAsync()
        {
            SetupEmptyQuery();
            var expected = MakeDtos(2);

            A.CallTo(() => _spendingGoalsRepository.GetInProgressGoalsAsync(
                    A<IQueryable<SpendingGoal>>._,
                    _today,
                    A<int>._,
                    A<int>._,
                    A<CancellationToken>._))
                .Returns(expected);

            var result = await _sut.Execute(_userId, SpendingGoalStatus.InProgress,
                new GetSpendingGoalsByStatusQueryParameters(null, null, null), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.Data);
        }

        [Fact]
        public async Task Execute_WhenStatusPending_ShouldCallGetPendingGoalsAsync()
        {
            SetupEmptyQuery();
            var expected = MakeDtos(1);

            A.CallTo(() => _spendingGoalsRepository.GetPendingGoalsAsync(
                    A<IQueryable<SpendingGoal>>._,
                    _today,
                    A<int>._,
                    A<int>._,
                    A<CancellationToken>._))
                .Returns(expected);

            var result = await _sut.Execute(_userId, SpendingGoalStatus.Pending,
                new GetSpendingGoalsByStatusQueryParameters(null, null, null), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.Data);
        }

        [Fact]
        public async Task Execute_WhenCategoryFilterProvided_ShouldPassFilteredQuery()
        {
            var categoryId = Guid.NewGuid();
            SetupEmptyQuery();

            List<IQueryable<SpendingGoal>> capturedQueries = new();
            A.CallTo(() => _spendingGoalsRepository.GetInProgressGoalsAsync(
                    A<IQueryable<SpendingGoal>>._,
                    _today,
                    A<int>._,
                    A<int>._,
                    A<CancellationToken>._))
                .Invokes((IQueryable<SpendingGoal> q, DateOnly _, int _, int _, CancellationToken _) =>
                    capturedQueries.Add(q))
                .Returns(MakeDtos(1));

            var queryParams = new GetSpendingGoalsByStatusQueryParameters(categoryId, null, null);
            var result = await _sut.Execute(_userId, SpendingGoalStatus.InProgress, queryParams, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Single(capturedQueries);
        }
    }
}
