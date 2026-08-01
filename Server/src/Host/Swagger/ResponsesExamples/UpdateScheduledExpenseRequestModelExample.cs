using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateScheduledExpenseRequestModelExample : IExamplesProvider<UpdateScheduledExpenseRequestModel>
    {
        public UpdateScheduledExpenseRequestModel GetExamples() =>
            new(Title: "Updated Rent", Amount: 1600m, FirstDue: null, Cadence: null);
    }
}
