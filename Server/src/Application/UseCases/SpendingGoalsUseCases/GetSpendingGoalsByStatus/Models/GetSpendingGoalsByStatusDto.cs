namespace Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models
{
    public record GetSpendingGoalsByStatusDto(
        Guid Id,
        string Description,
        Guid? CategoryId,
        decimal MinimumTargetAmount,
        decimal MaximumTargetAmount,
        decimal CurrentSpent,
        DateOnly StartsAt,
        DateOnly EndsAt,
        DateTimeOffset CreatedAt);
}
