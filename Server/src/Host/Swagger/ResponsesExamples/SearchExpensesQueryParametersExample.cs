using Application.ApplicationConstantsNamesapce;
using Application.UseCases.ExpensesUseCases.SearchExpenses.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class SearchExpensesQueryParametersExample : IExamplesProvider<SearchExpensesQueryParameters>
    {
        public SearchExpensesQueryParameters GetExamples()
        {
            var categoryIds = new List<Guid> { Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93"), Guid.Parse("93a165b9-c608-4906-a8af-c10e99c6b3c3") };
            var subCategoryIds = new List<Guid> { Guid.Parse("4bf4e511-9194-429b-9968-0bc1295b0fd5"), Guid.Parse("18e6a6eb-95d9-4762-ace2-c0f58b8b7527") };

            return new SearchExpensesQueryParameters(
                categoryIds,
                subCategoryIds,
                null,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 31),
                500,
                null,
                ApplicationConstants.ExpensesSortOptions.Amount,
                ApplicationConstants.SortOrders.Descending)
            {
                PageNumber = 2,
                PageSize = 30
            };
        }
    }
}
