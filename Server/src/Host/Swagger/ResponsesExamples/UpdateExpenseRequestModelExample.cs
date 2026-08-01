using Application.UseCases.ExpensesUseCases.UpdateExpense.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateExpenseRequestModelExample : IExamplesProvider<UpdateExpenseRequestModel>
    {
        public UpdateExpenseRequestModel GetExamples() => new(
            "Updated title",
            2000,
            null,
            null,
            new DateOnly(2026, 7, 25));
    }
}
