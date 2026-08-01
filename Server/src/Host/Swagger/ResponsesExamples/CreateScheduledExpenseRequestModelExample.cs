using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CreateScheduledExpenseRequestModelExample : IExamplesProvider<CreateScheduledExpenseRequestModel>
    {
        public CreateScheduledExpenseRequestModel GetExamples() =>
            new("Rent", 1500m, Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a"), null, CadenceInterval.Monthly, new DateOnly(2026, 8, 1));
    }
}
