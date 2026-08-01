using Host.SetupExtensions.Models;
using Serilog;

namespace Host.SetupExtensions
{
    public static class SecurityPolicies
    {
        public static IServiceCollection AddSecurityPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });

            var corsConfig = configuration.GetSection("CorsConfiguration").Get<CorsConfiguration>();
            if (corsConfig is null)
                Log.Logger.Warning("Failed to configure cors");

            services.AddCors(options =>
            {
                options.AddPolicy("Origins", policy =>
                {
                    policy.WithOrigins()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            
            return services;
        }
    }
}
