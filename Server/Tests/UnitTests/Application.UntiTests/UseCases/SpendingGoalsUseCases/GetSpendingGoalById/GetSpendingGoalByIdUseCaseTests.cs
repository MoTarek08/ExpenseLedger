using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.SpendingGoalNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.SpendingGoalsUseCases.GetSpendingGoalById
{
    public class GetSpendingGoalByIdUseCaseTests
    {
        private readonly ISpendingGoalsRepository _repository;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<GetSpendingGoalByIdUseCase> _logger;
        private readonly GetSpendingGoalByIdUseCase _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _goalId = Guid.NewGuid();
        private readonly DateOnly _today = new(2026, 7, 24);

        public GetSpendingGoalByIdUseCaseTests()
        {
            _repository = A.Fake<ISpendingGoalsRepository>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<GetSpendingGoalByIdUseCase>>();
            _sut = new GetSpendingGoalByIdUseCase(_repository, _dateProvider, _logger);

            A.CallTo(() => _dateProvider.Today).Returns(_today);
        }

        [Fact]
        public async Task Execute_WhenWrongOwnership_ShouldReturnNotFound()
        {
            var goal = SpendingGoal.Create(
                Guid.NewGuid(), "Test", null, 500m, 100m,
                new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31),
                new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoalWithSpent?>(null));

            var result = await _sut.Execute(_userId, _goalId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenPending_ShouldReturnPendingStatus()
        {
            var goal = SpendingGoal.Create(
                _userId, "Pending goal", null, 500m, 100m,
                new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31),
                new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoalWithSpent?>(new SpendingGoalWithSpent(goal, 0m)));

            var result = await _sut.Execute(_userId, _goalId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(SpendingGoalStatus.Pending, result.Data!.Status);
        }

        [Fact]
        public async Task Execute_WhenInProgress_ShouldReturnInProgressStatus()
        {
            var goal = SpendingGoal.Create(
                _userId, "Active goal", null, 500m, 100m,
                new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31),
                new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoalWithSpent?>(new SpendingGoalWithSpent(goal, 200m)));

            var result = await _sut.Execute(_userId, _goalId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(SpendingGoalStatus.InProgress, result.Data!.Status);
        }

        [Fact]
        public async Task Execute_WhenSucceeded_ShouldReturnSucceededStatus()
        {
            var goal = SpendingGoal.Create(
                _userId, "Completed goal", null, 500m, 100m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoalWithSpent?>(new SpendingGoalWithSpent(goal, 300m)));

            var result = await _sut.Execute(_userId, _goalId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(SpendingGoalStatus.Succeeded, result.Data!.Status);
        }

        [Fact]
        public async Task Execute_WhenFailed_ShouldReturnFailedStatus()
        {
            var goal = SpendingGoal.Create(
                _userId, "Failed goal", null, 500m, 100m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoalWithSpent?>(new SpendingGoalWithSpent(goal, 600m)));

            var result = await _sut.Execute(_userId, _goalId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(SpendingGoalStatus.Failed, result.Data!.Status);
        }
    }
}
