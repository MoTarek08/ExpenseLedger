using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.ProblemDetails.Problems;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Host.Swagger.OperationFilters
{
    public class ValidationErrorOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();

            var shouldAdd = method is "POST" or "PUT" or "PATCH"
                || (method is "GET" && context.ApiDescription.ParameterDescriptions
                    .Any(p => p.Source == BindingSource.Query));

            if (!shouldAdd) return;

            var problem = AllProblems.Get(OtherErrorCodes.VALIDATION_ERROR);
            operation.Responses!.TryAdd("400", new OpenApiResponse
            {
                Description = $"{problem.Detail} - {problem.ErrorCode}"
            });
        }
    }
}