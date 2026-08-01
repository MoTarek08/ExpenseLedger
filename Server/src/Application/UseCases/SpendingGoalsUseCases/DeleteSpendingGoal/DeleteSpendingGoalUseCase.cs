using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.SpendingGoalsUseCases.DeleteSpendingGoal
{
    public class DeleteSpendingGoalUseCase
    {
        private readonly ISpendingGoalsRepository _spendingGoalsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteSpendingGoalUseCase> _logger;

        public DeleteSpendingGoalUseCase(
            ISpendingGoalsRepository spendingGoalsRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteSpendingGoalUseCase> logger)
        {
            _spendingGoalsRepository = spendingGoalsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid goalId, Guid userId, CancellationToken cancellationToken)
        {
            var goal = await _spendingGoalsRepository.FindAsync(goalId,cancellationToken);
            if (goal is null)
                return Result.Success();
            if(goal.UserId != userId)
                return Result.Failure(new Error(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND));

            _spendingGoalsRepository.Remove(goal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Goal {GoalId} deleted for user {UserId}", goalId, userId);

            return Result.Success();
        }
    }
}
