using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Application.UseCases.UsersUseCases.GetUserProfile.ModelsNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UserProfileDtoExample : IExamplesProvider<UserProfileDto>
    {
        public UserProfileDto GetExamples()
        {
            return new UserProfileDto(
                Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Email: "user@example.com",
                DisplayName: "John Doe",
                RegisteredAt: new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.Zero),
                FinancialProfile: new FinancialProfileDto(
                    Guid.Parse("f1e2d3c4-b5a6-7980-abcd-ef1234567890"),
                    5000m,
                    1,
                    new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.Zero)));
        }
    }
}
