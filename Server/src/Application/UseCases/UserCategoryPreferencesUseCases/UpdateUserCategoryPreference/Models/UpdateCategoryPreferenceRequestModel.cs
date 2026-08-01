using Domain.Entities.DomainEnums;

namespace Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models
{
    public sealed record UpdateCategoryPreferenceRequestModel(
        Guid CategoryId,
        CategoryPreferenceLevel PreferenceLevel);
}
