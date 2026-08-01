using Application.ApplicationConstantsNamesapce;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class SearchScheduledExpensesQueryParametersExample : IExamplesProvider<SearchScheduledExpensesQueryParameters>
    {
        public SearchScheduledExpensesQueryParameters GetExamples()
        {
            return new SearchScheduledExpensesQueryParameters(
                ActiveOnly: true,
                SortBy: ApplicationConstants.ScheduledExpensesSortOptions.NextDueOn,
                SortOrder: ApplicationConstants.SortOrders.Ascending)
            {
                PageNumber = 1,
                PageSize = 20
            };
        }
    }
}
