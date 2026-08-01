using Domain.Entities.SpendingGoalNamespace;

namespace Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models
{
    public sealed record SpendingGoalWithSpent(SpendingGoal Goal, decimal CurrentSpent);
}
