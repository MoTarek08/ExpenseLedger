using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace.ModelsNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CreateUserFinancialProfileRequestExample : IExamplesProvider<CreateUserFinancialProfileRequest>
    {
        public CreateUserFinancialProfileRequest GetExamples() => new(5000m, 1);
    }
}