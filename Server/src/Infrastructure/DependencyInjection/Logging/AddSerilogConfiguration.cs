using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure.DependencyInjection.Logging
{
    public static class SerilogConfigurationExtensions
    {
        public static IHostBuilder AddSerilogConfiguration(this IHostBuilder hostBuilder, IConfiguration configuration)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
                hostBuilder.UseSerilog();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"CONFIGURATION FAILED: Failed to configure Serilog settings, Exception: {ex.Message}");
            }

            return hostBuilder;
        }
    }
}
