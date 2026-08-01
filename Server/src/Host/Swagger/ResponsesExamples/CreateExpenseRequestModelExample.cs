using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Domain.BusinessInvariants.CategoryCodesNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CreateExpenseRequestModelExample : IExamplesProvider<CreateExpenseRequestModel>
    {
        public CreateExpenseRequestModel GetExamples() => new(
            Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93"),
            "Birthday present for my father",
            1500,
            new DateOnly(2026, 7, 22),
            null);
    }
}
