using Application.Models;

namespace Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models
{
    public sealed record GetSpendingGoalsByStatusQueryParameters(
        Guid? CategoryId,
        DateOnly? EndingDateFrom,
        DateOnly? EndingDateTo) : PaginationParameters;
}
