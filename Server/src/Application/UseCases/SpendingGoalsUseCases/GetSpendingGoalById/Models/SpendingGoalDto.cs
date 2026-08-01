using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Domain.Entities.DomainEnums;

namespace Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models
{
    public sealed record SpendingGoalDto(
        Guid Id,
        string Description,
        Guid? CategoryId,
        decimal MinimumTargetAmount,
        decimal MaximumTargetAmount,
        decimal CurrentSpent,
        DateOnly StartsAt,
        DateOnly EndsAt,
        DateTimeOffset CreatedAt,
        SpendingGoalStatus Status)
        : GetSpendingGoalsByStatusDto(Id, Description, CategoryId,
            MinimumTargetAmount, MaximumTargetAmount, CurrentSpent,
            StartsAt, EndsAt, CreatedAt);
}
