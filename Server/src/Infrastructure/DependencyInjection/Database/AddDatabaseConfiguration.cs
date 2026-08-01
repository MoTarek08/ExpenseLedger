using Infrastructure.Database.AppDbContextNamespace;
using Infrastructure.Database.DatabaseSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.DependencyInjection.Database
{
    public static class DatabaseConfigurationExtensions
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var dbSettings = configuration.GetSection("DbSettings").Get<DbSettings>();
            if (dbSettings is null)
            {
                Log.Logger.Error("Failed to configure Database settings");
                throw new InvalidOperationException("Failed to configure Database settings");
            }

            services.AddSingleton(dbSettings);
            services.AddDbContext<AppDbContext>();
            return services;
        }
    }
}
