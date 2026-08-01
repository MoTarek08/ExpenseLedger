using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.SpendingGoalsUseCases.DeleteSpendingGoal;
using Domain.Entities.SpendingGoalNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.SpendingGoalsUseCases.DeleteSpendingGoal
{
    public class DeleteSpendingGoalUseCaseTests
    {
        private readonly ISpendingGoalsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteSpendingGoalUseCase> _logger;
        private readonly DeleteSpendingGoalUseCase _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _goalId = Guid.NewGuid();
        private readonly DateOnly _futureEnd = new(2026, 8, 31);
        private readonly DateOnly _pastEnd = new(2026, 6, 30);

        public DeleteSpendingGoalUseCaseTests()
        {
            _repository = A.Fake<ISpendingGoalsRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<DeleteSpendingGoalUseCase>>();
            _sut = new DeleteSpendingGoalUseCase(_repository, _unitOfWork, _logger);
        }

        [Fact]
        public async Task Execute_WhenGoalNotFound_ShouldReturnSuccess()
        {
            A.CallTo(() => _repository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoal?>(null));

            var result = await _sut.Execute(_goalId, _userId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenGoalNotOwned_ShouldReturnFailure()
        {
            var goal = SpendingGoal.Create(
                Guid.NewGuid(), "Test", null, 500m, 100m,
                new DateOnly(2026, 1, 1), _futureEnd,
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoal?>(goal));

            var result = await _sut.Execute(_goalId, _userId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenGoalCompleted_ShouldDeleteAndReturnSuccess()
        {
            var goal = SpendingGoal.Create(
                _userId, "Test", null, 500m, 100m,
                new DateOnly(2026, 1, 1), _pastEnd,
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoal?>(goal));

            var result = await _sut.Execute(_goalId, _userId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Remove(goal)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenValidRequest_ShouldRemoveGoalAndReturnSuccess()
        {
            var goal = SpendingGoal.Create(
                _userId, "Test", null, 500m, 100m,
                new DateOnly(2026, 8, 1), _futureEnd,
                new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _repository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(Task.FromResult<SpendingGoal?>(goal));

            var result = await _sut.Execute(_goalId, _userId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Remove(goal)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
