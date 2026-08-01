using Host.SetupExtensions.Models;
using Serilog;

namespace Host.SetupExtensions
{
    public static partial class HttpPipelineConfiguration
    {
        public static WebApplicationBuilder AddHttpPipelineConfiguration(this WebApplicationBuilder builder)
        {
            var pipelineMetrics = builder.Configuration.GetSection("PiplelineMetrics").Get<PipelineMetrics>();
            if(pipelineMetrics is null)
            {
                Log.Logger.Warning("Failed to configure the pipeline metrics");
                return builder;
            }

            builder.Services.AddSingleton(pipelineMetrics);
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = pipelineMetrics.MaxRequestBodySizeInBytes;
                options.Limits.MaxRequestBufferSize = pipelineMetrics.MaxRequestBufferSizeInBytes;
            });
            return builder;
        }
    }
}
