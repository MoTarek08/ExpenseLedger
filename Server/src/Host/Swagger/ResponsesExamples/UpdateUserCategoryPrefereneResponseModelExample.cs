using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateUserCategoryPrefereneResponseModelExample : IExamplesProvider<UpdateUserCategoryPrefereneResponseModel>
    {
        public UpdateUserCategoryPrefereneResponseModel GetExamples() =>
            new(
                Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                Guid.Parse("4f2504e0-4f89-11d3-9a0c-0305e82c3302"),
                CategoryPreferenceLevel.Neutral,
                CategoryPreferenceLevel.Essential);
    }
}