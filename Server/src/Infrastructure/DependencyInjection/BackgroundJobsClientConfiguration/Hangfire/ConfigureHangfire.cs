using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection.BackgroundJobsClientConfiguration.Hangfire
{
    public static class ConfigureHangfire
    {
        public static IServiceCollection AddHangfire(IServiceCollection services, BackgroundJobsClientDbSettings hangfireSettings)
        {
            services.AddHangfire(cfg =>
                cfg.UsePostgreSqlStorage(cfg => cfg.UseNpgsqlConnection(hangfireSettings.ConnectionString))
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer());

            services.AddHangfireServer();
            return services;
        }
    }
}
