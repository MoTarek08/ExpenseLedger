using Application.UseCases.ExpensesUseCases.Models;
using Domain.BusinessInvariants.CategoryCodesNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class ExpenseDtoExample : IExamplesProvider<ExpenseDto>
    {
        public ExpenseDto GetExamples() => new ExpenseDto(
            Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a"),
            Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930"),
            new DateOnly(2026, 1, 1),
            "Birthday present for my father",
            1500,
            CategoryCodes.Gifts.Code,
            null,
            null,
            2);
    }
}
