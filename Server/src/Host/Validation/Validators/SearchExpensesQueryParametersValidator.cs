using Application.ApplicationConstantsNamesapce;
using Application.UseCases.ExpensesUseCases.SearchExpenses.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class SearchExpensesQueryParametersValidator
        : PaginationParametersValidator<SearchExpensesQueryParameters>
    {
        public SearchExpensesQueryParametersValidator()
        {
            When(x => x.Title is not null, () =>
            {
                RuleFor(x => x.Title)
                    .MaximumLength(BusinessConstants.MaxTitleLength)
                    .WithMessage($"Title cannot be more that {BusinessConstants.MaxTitleLength} characters");
            });

            When(x => x.From.HasValue && x.To.HasValue, () =>
            {
                RuleFor(x => x.To)
                    .GreaterThanOrEqualTo(x => x.From!.Value)
                    .WithMessage("Invalid date range");
            });

            When(x => x.From.HasValue, () =>
            {
                RuleFor(x => x.From!.Value)
                    .ValidDateOnlyRange();
            });

            When(x => x.To.HasValue, () =>
            {
                RuleFor(x => x.To!.Value)
                    .ValidDateOnlyRange();
            });

            When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue, () =>
            {
                RuleFor(x => x.MaxAmount)
                    .GreaterThanOrEqualTo(x => x.MinAmount!.Value)
                    .WithMessage("MaxAmount must be greater than or equal to MinAmount.");
            });

            When(x => x.MinAmount.HasValue, () =>
            {
                RuleFor(x => x.MinAmount)
                    .GreaterThan(0)
                    .WithMessage("MinAmount must be greater than zero.");
            });

            When(x => x.MaxAmount.HasValue, () =>
            {
                RuleFor(x => x.MaxAmount)
                    .GreaterThan(0)
                    .WithMessage("MaxAmount must be greater than zero.");
            });
            RuleFor(x => x.SortBy)
                .Must(v => ApplicationConstants.ExpensesSortOptions.All.Contains(v, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"SortBy must be one of: {string.Join(", ", ApplicationConstants.ExpensesSortOptions.All)}.");

            RuleFor(x => x.SortOrder)
                .ValidSortOrder();

        }
    }
}
