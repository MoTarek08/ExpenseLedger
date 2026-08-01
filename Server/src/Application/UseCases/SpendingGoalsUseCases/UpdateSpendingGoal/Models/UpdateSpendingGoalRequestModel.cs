namespace Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models
{
    public sealed record UpdateSpendingGoalRequestModel(
        string? Description,
        decimal? MinimumTargetAmount,
        decimal? MaximumTargetAmount,
        DateOnly? StartDate,
        DateOnly? EndDate);
}
