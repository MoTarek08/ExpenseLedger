using Infrastructure.DependencyInjection.BackgroundJobsClientConfiguration.Hangfire;
using Infrastructure.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.DependencyInjection.BackgroundJobsClientConfiguration
{
    public static class BackgroundJobsClientConfigurationExtensions
    {
        public static IServiceCollection AddBackgroundJobsClientConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("HangfireDbSettings").Get<BackgroundJobsClientDbSettings>();
            if (settings is null)
            {
                Log.Logger.Error("CONFIGURATION FAILED: Failed to configure hangfire database settings");
                return services;
            }

            ConfigureHangfire.AddHangfire(services, settings);
            return services;
        }
    }
}
