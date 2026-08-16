using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.Notification;
using Domain.Entities.SpendingGoalNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal
{
    public class UpdateSpendingGoalUseCaseTests
    {
        private readonly ISpendingGoalsRepository _goalsRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<UpdateSpendingGoalUseCase> _logger;
        private readonly UpdateSpendingGoalUseCase _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _otherUserId = Guid.NewGuid();
        private readonly Guid _goalId = Guid.NewGuid();
        private readonly DateOnly _startDate = new(2026, 8, 1);
        private readonly DateOnly _endDate = new(2026, 8, 31);
        private readonly DateOnly _today = new(2026, 7, 22);

        public UpdateSpendingGoalUseCaseTests()
        {
            _goalsRepository = A.Fake<ISpendingGoalsRepository>();
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<UpdateSpendingGoalUseCase>>();
            _sut = new UpdateSpendingGoalUseCase(
                _goalsRepository, _notificationsRepository,
                _unitOfWork, _dateProvider, _logger);

            A.CallTo(() => _dateProvider.Now)
                .Returns(new DateTimeOffset(2026, 7, 22, 0, 0, 0, TimeSpan.Zero));
            A.CallTo(() => _dateProvider.Today)
                .Returns(_today);
        }

        private SpendingGoal CreateActiveGoal(Guid userId, string? description = "Goal")
            => SpendingGoal.Create(userId, description, null, 500m, 100m, _startDate, _endDate, new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero));

        [Fact]
        public async Task Execute_WhenGoalNotFound_ShouldReturnNotFound()
        {
            var request = new UpdateSpendingGoalRequestModel("Updated", null, null, null, null);

            A.CallTo(() => _goalsRepository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns((SpendingGoal?)null);

            var result = await _sut.Execute(_goalId, _userId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenGoalNotOwned_ShouldReturnNotFound()
        {
            var request = new UpdateSpendingGoalRequestModel("Updated", null, null, null, null);
            var goal = CreateActiveGoal(_otherUserId);

            A.CallTo(() => _goalsRepository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(goal);

            var result = await _sut.Execute(_goalId, _userId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenGoalCompleted_ShouldReturnCompletedError()
        {
            var request = new UpdateSpendingGoalRequestModel("Updated", null, null, null, null);
            var goal = CreateActiveGoal(_userId);
            // Move end date to the past so the lifecycle is Completed
            var pastGoal = SpendingGoal.Create(_userId, "Goal", null, 500m, 100m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 1),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _goalsRepository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(pastGoal);

            var result = await _sut.Execute(_goalId, _userId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(SpendingGoalsErrorCodes.SPENDING_GOAL_COMPLETED, result.Error!.Code);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenOnlyDescriptionUpdated_ShouldNotCheckProgress()
        {
            var request = new UpdateSpendingGoalRequestModel("New description", null, null, null, null);
            var goal = CreateActiveGoal(_userId);

            A.CallTo(() => _goalsRepository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(goal);

            var result = await _sut.Execute(_goalId, _userId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("New description", goal.Description);
            A.CallTo(() => _goalsRepository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceOrMore();
            Assert.Empty(result.Data!.Notifications);
        }

        [Fact]
        public async Task Execute_WhenTargetsUpdatedAndMeetsTargets_ShouldCreateNotification()
        {
            var request = new UpdateSpendingGoalRequestModel(null, 200m, 600m, null, null);
            var goal = CreateActiveGoal(_userId);

            A.CallTo(() => _goalsRepository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(goal);
            A.CallTo(() => _goalsRepository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(new SpendingGoalWithSpent(goal, 300m));

            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(_userId, A<string>._, A<CancellationToken>._))
                .Returns(false);

            var result = await _sut.Execute(_goalId, _userId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data!.Notifications);
            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedTwiceOrMore();
        }

        [Fact]
        public async Task Execute_WhenTargetsUpdatedAndMeetsTargetsButDedupExists_ShouldNotCreateDuplicateNotification()
        {
            var request = new UpdateSpendingGoalRequestModel(null, 200m, 600m, null, null);
            var goal = CreateActiveGoal(_userId);

            A.CallTo(() => _goalsRepository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(goal);
            A.CallTo(() => _goalsRepository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(new SpendingGoalWithSpent(goal, 300m));

            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(_userId, A<string>._, A<CancellationToken>._))
                .Returns(true);

            var result = await _sut.Execute(_goalId, _userId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!.Notifications);
            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceOrMore();
        }

        [Fact]
        public async Task Execute_WhenValidRequest_ShouldUpdateAndReturnSuccess()
        {
            var newStart = new DateOnly(2026, 8, 15);
            var newEnd = new DateOnly(2026, 9, 15);
            var request = new UpdateSpendingGoalRequestModel("Updated description", 200m, 600m, newStart, newEnd);
            var goal = CreateActiveGoal(_userId);

            A.CallTo(() => _goalsRepository.FindAsync(_goalId, A<CancellationToken>._))
                .Returns(goal);

            A.CallTo(() => _goalsRepository.ExistsForPeriodAsync(
                    _userId, goal.CategoryId, newStart, newEnd, _goalId, A<CancellationToken>._))
                .Returns(false);

            A.CallTo(() => _goalsRepository.GetGoalWithSpentAsync(_goalId, _userId, A<CancellationToken>._))
                .Returns(new SpendingGoalWithSpent(goal, 50m));

            var result = await _sut.Execute(_goalId, _userId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Updated description", goal.Description);
            Assert.Equal(200m, goal.MinimumTargetAmount);
            Assert.Equal(600m, goal.MaximumTargetAmount);
            Assert.Equal(newStart, goal.StartsAt);
            Assert.Equal(newEnd, goal.EndsAt);
            Assert.Empty(result.Data!.Notifications);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceOrMore();
        }
    }
}
