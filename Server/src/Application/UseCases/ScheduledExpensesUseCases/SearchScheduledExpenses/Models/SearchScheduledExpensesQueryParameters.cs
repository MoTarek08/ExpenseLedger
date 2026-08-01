using Application.ApplicationConstantsNamesapce;
using Application.Models;

namespace Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses.Models
{
    public sealed record SearchScheduledExpensesQueryParameters(
        bool? ActiveOnly,
        string SortBy = ApplicationConstants.ScheduledExpensesSortOptions.NextDueOn,
        string SortOrder = ApplicationConstants.SortOrders.Ascending
    ) : PaginationParameters;
}
