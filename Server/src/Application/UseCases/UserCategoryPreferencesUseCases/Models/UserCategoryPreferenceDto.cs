using Domain.Entities.DomainEnums;

namespace Application.UseCases.UserCategoryPreferencesUseCases.Models
{
    public sealed record UserCategoryPreferenceDto(
        string CategoryCode,
        string CategoryName,
        CategoryPreferenceLevel PreferenceLevel,
        DateTimeOffset CreatedAt);
}
