using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Minio.AspNetCore.HealthChecks;
using Serilog;

namespace Infrastructure.DependencyInjection.HealthChecks
{
    public static class HealthChecks
    {
        public static  IServiceCollection CustomAddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConnectionString = configuration.GetValue<string>("DbSettings:ConnectionString");
            if(dbConnectionString is null)
            {
                Log.Logger.Error("Db connection string is missing");
                dbConnectionString = "";
            }
            var hangfireDbConnectionString = configuration.GetValue<string>("HangfireDbSettings:ConnectionString");
            if (hangfireDbConnectionString is null)
            {
                Log.Logger.Warning("Hangfire db connection string is missing");
                hangfireDbConnectionString = "";
            }


            services.AddHealthChecks()
                .AddNpgSql(connectionString: dbConnectionString,name:"app-db")
                .AddDbContextCheck<AppDbContext>()
                .AddNpgSql(connectionString: hangfireDbConnectionString,name:"hangfire-db")
                .AddMinio(factory => factory.GetRequiredService<IMinioClient>());
            return services;
        }
    }
}
