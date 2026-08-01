using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UserCategoryPreferenceDtoListExample : IExamplesProvider<List<UserCategoryPreferenceDto>>
    {
        public List<UserCategoryPreferenceDto> GetExamples()
        {
            return
            [
                new("FOOD", "Food & Dining", CategoryPreferenceLevel.Essential,
                    new DateTimeOffset(2026, 7, 20, 14, 30, 0, TimeSpan.Zero)),
                new("TRANSPORT", "Transportation", CategoryPreferenceLevel.Important,
                    new DateTimeOffset(2026, 7, 19, 9, 0, 0, TimeSpan.Zero)),
                new("ENTERTAIN", "Entertainment", CategoryPreferenceLevel.Neutral,
                    new DateTimeOffset(2026, 7, 18, 16, 45, 0, TimeSpan.Zero)),
            ];
        }
    }
}
