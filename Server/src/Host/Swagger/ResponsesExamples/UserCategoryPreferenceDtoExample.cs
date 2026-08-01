using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UserCategoryPreferenceDtoExample : IExamplesProvider<UserCategoryPreferenceDto>
    {
        public UserCategoryPreferenceDto GetExamples()
        {
            return new UserCategoryPreferenceDto(
                "FOOD",
                "Food & Dining",
                CategoryPreferenceLevel.Essential,
                new DateTimeOffset(2026, 7, 20, 14, 30, 0, TimeSpan.Zero));
        }
    }
}
