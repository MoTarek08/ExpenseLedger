using Microsoft.EntityFrameworkCore;

namespace Host.Middlewares
{
    public class ContentTypeChecker
    {
        private readonly RequestDelegate _next;

        public ContentTypeChecker(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var hasBody = request.ContentLength > 0 || request.Headers.TransferEncoding.Count > 0;

            if (hasBody)
            {
                if (!request.HasJsonContentType())
                {
                    httpContext.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return;
                }
            }

                await _next.Invoke(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseContentTypeChecker(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ContentTypeChecker>();
        }
    }
}
