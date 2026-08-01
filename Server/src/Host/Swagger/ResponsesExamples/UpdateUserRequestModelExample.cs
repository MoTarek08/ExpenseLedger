using Application.UseCases.UsersUseCases.UpdateUser.ModelsNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateUserRequestModelExample : IExamplesProvider<UpdateUserRequestModel>
    {
        public UpdateUserRequestModel GetExamples() => new("John Doe");
    }
}