namespace Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal.Models
{
    public sealed record CreateSpendingGoalRequestModel(
        string? Description,
        decimal MaximumTargetAmount,
        decimal MinimumTargetAmount,
        DateOnly StartDate,
        DateOnly EndDate,
        Guid? CategoryId = null);
}

