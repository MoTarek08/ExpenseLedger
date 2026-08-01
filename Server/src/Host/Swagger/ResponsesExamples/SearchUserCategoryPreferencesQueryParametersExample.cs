using Application.ApplicationConstantsNamesapce;
using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class SearchUserCategoryPreferencesQueryParametersExample : IExamplesProvider<SearchUserCategoryPreferencesQueryParameters>
    {
        public SearchUserCategoryPreferencesQueryParameters GetExamples()
        {
            return new SearchUserCategoryPreferencesQueryParameters(
                PreferenceLevel: CategoryPreferenceLevel.Important,
                SortOrder: ApplicationConstants.SortOrders.Descending)
            {
                PageNumber = 1,
                PageSize = 20
            };
        }
    }
}
