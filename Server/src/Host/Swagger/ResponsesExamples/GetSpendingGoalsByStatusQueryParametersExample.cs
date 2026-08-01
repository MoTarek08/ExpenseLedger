using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class GetSpendingGoalsByStatusQueryParametersExample : IExamplesProvider<GetSpendingGoalsByStatusQueryParameters>
    {
        public GetSpendingGoalsByStatusQueryParameters GetExamples()
        {
            return new GetSpendingGoalsByStatusQueryParameters(
                CategoryId: null,
                EndingDateFrom: new DateOnly(2026, 7, 1),
                EndingDateTo: new DateOnly(2026, 7, 31))
            {
                PageNumber = 1,
                PageSize = 20
            };
        }
    }
}
