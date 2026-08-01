using Application.ApplicationConstantsNamesapce;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class SearchScheduledExpensesQueryParametersValidator
        : PaginationParametersValidator<SearchScheduledExpensesQueryParameters>
    {
        public SearchScheduledExpensesQueryParametersValidator()
        {
            RuleFor(x => x.SortBy)
                .Must(v => ApplicationConstants.ScheduledExpensesSortOptions.All.Contains(v, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"SortBy must be one of: {string.Join(", ", ApplicationConstants.ScheduledExpensesSortOptions.All)}.");

            RuleFor(x => x.SortOrder)
                .ValidSortOrder();
        }
    }
}
