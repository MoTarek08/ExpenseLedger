using Application.UseCases.ScheduledExpensesUseCases.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class ScheduledExpenseDtoExample : IExamplesProvider<ScheduledExpenseDto>
    {
        public ScheduledExpenseDto GetExamples() => new ScheduledExpenseDto(
            Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c"),
            true,
            "Rent",
            1500m,
            CadenceInterval.Monthly,
            "HOUSING",
            null,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 7, 1),
            DateTimeOffset.Parse("2026-06-15T10:00:00+00:00"));
    }
}
