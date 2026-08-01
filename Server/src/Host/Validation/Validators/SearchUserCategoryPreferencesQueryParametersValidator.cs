using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public class SearchUserCategoryPreferencesQueryParametersValidator
        : PaginationParametersValidator<SearchUserCategoryPreferencesQueryParameters>
    {
        public SearchUserCategoryPreferencesQueryParametersValidator()
        {
            RuleFor(x => x.SortOrder)
                .ValidSortOrder();
        }
    }
}
