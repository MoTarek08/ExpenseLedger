using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace Infrastructure.Logging.SerilogNamespace
{
    public static class SerilogExtensions
    {
        public static void ConfigureRequestLogging(this IApplicationBuilder builder)
        {
            builder.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (ctx, elapsed, ex) =>
                {
                    if (ex is not null)
                        return LogEventLevel.Error;

                    var httpMethod = ctx.Request.Method;

                    if (httpMethod == HttpMethod.Post.Method || httpMethod == HttpMethod.Put.Method || httpMethod == HttpMethod.Patch.Method)
                    {
                        if (elapsed > 2000)
                            return LogEventLevel.Warning;
                    }
                    else
                    {
                        if (elapsed > 1000)
                            return LogEventLevel.Warning;
                    }
                    return LogEventLevel.Information;
                };
            });
        }
    }
}
