using Host.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class GetRemainingBudgetResponseExample : IExamplesProvider<GetRemainingBudgetResponse>
    {
        public GetRemainingBudgetResponse GetExamples()
        {
            return new GetRemainingBudgetResponse(1250.50m);
        }
    }
}
