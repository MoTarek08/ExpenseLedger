using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.ProblemDetails.Problems;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Host.Swagger.OperationFilters
{
    public class InternalServerErrorOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var problem = AllProblems.Get(StorageErrorCodes.INTERNAL_SERVER_ERROR);
            operation.Responses!.TryAdd("500", new OpenApiResponse
            {
                Description = $"{problem.Detail} - {problem.ErrorCode}"
            });
    }
    }
}
