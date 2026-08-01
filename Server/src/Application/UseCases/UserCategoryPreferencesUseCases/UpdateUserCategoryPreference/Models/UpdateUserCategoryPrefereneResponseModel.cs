using Domain.Entities.DomainEnums;

namespace Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models
{
    public sealed record UpdateUserCategoryPrefereneResponseModel(
        Guid UserId,
        Guid CategoryId,
        CategoryPreferenceLevel OldPreferenceLevel,
        CategoryPreferenceLevel NewPreferenceLevel);
}
