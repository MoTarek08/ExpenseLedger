using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateCategoryPreferenceRequestModelExample : IExamplesProvider<UpdateCategoryPreferenceRequestModel>
    {
        public UpdateCategoryPreferenceRequestModel GetExamples() =>
            new(Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"), CategoryPreferenceLevel.Essential);
    }
}