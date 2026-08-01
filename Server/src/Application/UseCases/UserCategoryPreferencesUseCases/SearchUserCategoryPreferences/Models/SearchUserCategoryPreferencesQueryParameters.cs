using Application.ApplicationConstantsNamesapce;
using Application.Models;
using Domain.Entities.DomainEnums;

namespace Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences.Models
{
    public sealed record SearchUserCategoryPreferencesQueryParameters(
        CategoryPreferenceLevel? PreferenceLevel,
        string SortOrder = ApplicationConstants.SortOrders.Descending
    ) : PaginationParameters;
}
