using Application.UseCases.ExpensesUseCases.GetExpensesByDay.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class GetExpensesByDayQueryParametersExample : IExamplesProvider<GetExpensesByDayRequestModel>
    {
        public GetExpensesByDayRequestModel GetExamples()
        {
            return new GetExpensesByDayRequestModel(new DateOnly(2026, 7, 22));
        }
    }
}
