using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Host.Swagger.OperationFilters
{
    public class AuthorizationResponsesOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var hasAuthorize = context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any()
                    ||
                    context.MethodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any() == true;

                if (!hasAuthorize) return;

                operation.Responses!.TryAdd("401", new OpenApiResponse
                {
                    Description = "Unauthorized"
                });

                operation.Responses.TryAdd("403", new OpenApiResponse
                {
                    Description = "Forbidden"
                });
            }
        }
}
