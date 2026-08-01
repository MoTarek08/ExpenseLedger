using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Models.Result;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Domain.Entities.DomainEnums;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById
{
    public class GetSpendingGoalByIdUseCase
    {
        private readonly ISpendingGoalsRepository _spendingGoalsRepository;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<GetSpendingGoalByIdUseCase> _logger;

        public GetSpendingGoalByIdUseCase(
            ISpendingGoalsRepository spendingGoalsRepository,
            IDateProvider dateProvider,
            ILogger<GetSpendingGoalByIdUseCase> logger)
        {
            _spendingGoalsRepository = spendingGoalsRepository;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result<SpendingGoalDto>> Execute(Guid userId, Guid goalId, CancellationToken cancellationToken)
        {
            var goalWithSpent = await _spendingGoalsRepository.GetGoalWithSpentAsync(goalId, userId, cancellationToken);

            if (goalWithSpent is null)
            {
                _logger.LogWarning("Spending goal {GoalId} not found for user {UserId}", goalId, userId);
                return Result<SpendingGoalDto>.Failure(new Error(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND));
            }

            var today = _dateProvider.Today;
            var lifecycle = goalWithSpent.Goal.GetLifecycle(today);

            SpendingGoalStatus status = lifecycle switch
            {
                GoalLifecycle.Pending => SpendingGoalStatus.Pending,
                GoalLifecycle.Active => SpendingGoalStatus.InProgress,
                GoalLifecycle.Completed => goalWithSpent.Goal.Evaluate(goalWithSpent.CurrentSpent, today) switch
                {
                    GoalOutcome.Succeeded => SpendingGoalStatus.Succeeded,
                    _ => SpendingGoalStatus.Failed
                },
                _ => throw new ArgumentOutOfRangeException(nameof(status))
            };

            var dto = new SpendingGoalDto(
                goalWithSpent.Goal.Id,
                goalWithSpent.Goal.Description ?? string.Empty,
                goalWithSpent.Goal.CategoryId,
                goalWithSpent.Goal.MinimumTargetAmount,
                goalWithSpent.Goal.MaximumTargetAmount,
                goalWithSpent.CurrentSpent,
                goalWithSpent.Goal.StartsAt,
                goalWithSpent.Goal.EndsAt,
                goalWithSpent.Goal.CreatedAt,
                status);

            return Result<SpendingGoalDto>.Success(dto);
        }
    }
}
