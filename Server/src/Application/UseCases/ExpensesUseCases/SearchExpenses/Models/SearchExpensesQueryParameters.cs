using Application.ApplicationConstantsNamesapce;
using Application.Models;

namespace Application.UseCases.ExpensesUseCases.SearchExpenses.Models
{
    public sealed record SearchExpensesQueryParameters(
        List<Guid>? CategoryIds,
        List<Guid>? SubCategoryIds,
        string? Title,
        DateOnly? From,
        DateOnly? To,
        decimal? MinAmount,
        decimal? MaxAmount,
        string SortBy = ApplicationConstants.ExpensesSortOptions.SpentOn,
        string SortOrder = ApplicationConstants.SortOrders.Descending
        ) : PaginationParameters ;
}
