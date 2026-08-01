using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;

namespace Application.UseCases.UsersUseCases.GetUserProfile.ModelsNamespace
{
    public sealed record UserProfileDto(
        Guid Id,
        string Email,
        string DisplayName,
        DateTimeOffset RegisteredAt,
        FinancialProfileDto? FinancialProfile
    );
}
