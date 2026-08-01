using Domain.Entities.SpendingGoalNamespace;

namespace Application.UseCases.SpendingGoalsUseCases.Helpers
{
    public static class SpendingGoalsHelpers
    {
        public static bool CurrentlyMeetsTargets(SpendingGoal goal, decimal spentAmount)
        {
            return spentAmount >= goal.MinimumTargetAmount && spentAmount <= goal.MaximumTargetAmount;
        }
    }
}
