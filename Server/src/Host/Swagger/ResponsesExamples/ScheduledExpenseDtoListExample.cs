using Application.UseCases.ScheduledExpensesUseCases.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class ScheduledExpenseDtoListExample : IExamplesProvider<List<ScheduledExpenseDto>>
    {
        public List<ScheduledExpenseDto> GetExamples() => new()
        {
            new ScheduledExpenseDto(
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
                DateTimeOffset.Parse("2026-06-15T10:00:00+00:00")),

            new ScheduledExpenseDto(
                Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                false,
                "Netflix",
                15.99m,
                CadenceInterval.Monthly,
                "ENTERTAINMENT",
                "STREAMING",
                new DateOnly(2026, 1, 15),
                null,
                null,
                DateTimeOffset.Parse("2026-01-10T08:30:00+00:00"))
        };
    }
}
