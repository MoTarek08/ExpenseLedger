using Host.Attributes;
using Host.ProblemDetails.Problems;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Host.Swagger.OperationFilters
{
    public class ProducesErrorOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var errorAttributes = context.MethodInfo
                .GetCustomAttributes<ProducesErrorAttribute>(true);

            foreach (var attr in errorAttributes)
            {
                try
                {
                    var problem = AllProblems.Get(attr.ErrorCode);
                    var statusCode = problem.StatusCode.ToString();
                    var description = $"{problem.Detail} — error code: `{attr.ErrorCode}`";

                    if (operation.Responses!.TryGetValue(statusCode, out var existing))
                        existing.Description += $"\n\n- {description}";
                    else
                        operation.Responses[statusCode] = new OpenApiResponse
                        {
                            Description = description
                        };
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }
        }
    }
}
