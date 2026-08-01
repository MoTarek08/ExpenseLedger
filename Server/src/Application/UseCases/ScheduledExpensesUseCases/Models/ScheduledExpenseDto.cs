using Domain.Entities.DomainEnums;

namespace Application.UseCases.ScheduledExpensesUseCases.Models
{
    public sealed record ScheduledExpenseDto(
        Guid Id,
        bool IsActive,
        string? Title,
        decimal Amount,
        CadenceInterval Cadence,
        string CategoryCode,
        string? SubCategoryCode,
        DateOnly FirstDueOn,
        DateOnly? NextDueOn,
        DateOnly? LastProcessedAt,
        DateTimeOffset CreatedAt);
}
