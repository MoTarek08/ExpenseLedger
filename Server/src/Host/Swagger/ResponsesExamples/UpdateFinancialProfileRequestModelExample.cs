using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace.ModelsNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateFinancialProfileRequestModelExample : IExamplesProvider<UpdateFinancialProfileRequestModel>
    {
        public UpdateFinancialProfileRequestModel GetExamples() => new(6000m, 15);
    }
}