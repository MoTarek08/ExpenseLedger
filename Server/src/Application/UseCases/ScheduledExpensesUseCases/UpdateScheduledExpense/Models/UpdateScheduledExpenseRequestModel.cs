using Domain.Entities.DomainEnums;

namespace Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense.Models
{
    public sealed record UpdateScheduledExpenseRequestModel(
        string? Title,
        decimal? Amount,
        DateOnly? FirstDue,
        CadenceInterval? Cadence);
}
