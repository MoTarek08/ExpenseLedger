using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal.Models;
using Domain.Entities.SpendingGoalNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.SpendingGoalsUseCases.CreateSpendingGoal
{
    public class CreateSpendingGoalUseCaseTests
    {
        private readonly ISpendingGoalsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateSpendingGoalUseCase> _logger;
        private readonly CreateSpendingGoalUseCase _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly DateOnly _startDate = new(2026, 8, 1);
        private readonly DateOnly _endDate = new(2026, 8, 31);

        public CreateSpendingGoalUseCaseTests()
        {
            _repository = A.Fake<ISpendingGoalsRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<CreateSpendingGoalUseCase>>();
            _sut = new CreateSpendingGoalUseCase(_repository, _unitOfWork, _dateProvider, _logger);

            A.CallTo(() => _dateProvider.Now)
                .Returns(new DateTimeOffset(2026, 7, 22, 0, 0, 0, TimeSpan.Zero));
        }

        private CreateSpendingGoalRequestModel MakeRequest(string? description = "Test goal", decimal min = 100m, decimal max = 500m)
            => new(description, max, min, _startDate, _endDate);

        [Fact]
        public async Task Execute_WhenConflictingGoalExists_ShouldReturnFailure()
        {
            var request = MakeRequest();

            A.CallTo(() => _repository.ExistsForPeriodAsync(
                    _userId, request.CategoryId, request.StartDate, request.EndDate, A<Guid>._, A<CancellationToken>._))
                .Returns(true);

            var result = await _sut.Execute(_userId, request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS, result.Error!.Code);
            A.CallTo(() => _repository.Add(A<SpendingGoal>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenDescriptionIsNull_ShouldCreateGoalSuccessfully()
        {
            var request = MakeRequest(description: null);

            A.CallTo(() => _repository.ExistsForPeriodAsync(
                    _userId, request.CategoryId, request.StartDate, request.EndDate, A<Guid>._, A<CancellationToken>._))
                .Returns(false);

            var result = await _sut.Execute(_userId, request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Add(A<SpendingGoal>.That.Matches(g => g.Description == null)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenDescriptionIsValid_ShouldCreateGoalSuccessfully()
        {
            var request = MakeRequest("Valid description");

            A.CallTo(() => _repository.ExistsForPeriodAsync(
                    _userId, request.CategoryId, request.StartDate, request.EndDate, A<Guid>._, A<CancellationToken>._))
                .Returns(false);

            var result = await _sut.Execute(_userId, request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Add(A<SpendingGoal>.That.Matches(g => g.Description == "Valid description")))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenLowerEqualsUpperTarget_ShouldCreateGoalSuccessfully()
        {
            var request = MakeRequest("Equal targets", min: 500m, max: 500m);

            A.CallTo(() => _repository.ExistsForPeriodAsync(
                    _userId, request.CategoryId, request.StartDate, request.EndDate, A<Guid>._, A<CancellationToken>._))
                .Returns(false);

            var result = await _sut.Execute(_userId, request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Add(A<SpendingGoal>.That.Matches(g => g.MinimumTargetAmount == 500m && g.MaximumTargetAmount == 500m)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
