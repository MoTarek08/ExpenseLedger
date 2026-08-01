using Application.Interfaces.RefreshTokenSettings;
using Infrastructure.Authentecation.JwtAuthentication.AccessToken;
using Infrastructure.Authentecation.JwtAuthentication.RefreshToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace Infrastructure.DependencyInjection.Authentication
{
    public static class AuthenticationConfigurationExtensions
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var accessTokenSettings = configuration.GetSection("AccessTokenSettings").Get<AccessTokenSettings>();
            if (accessTokenSettings is null || accessTokenSettings.LifeTime == 0)
            {
                Log.Logger.Error("CONFIGURATION FAILED: Failed to configure access token settings");
                throw new InvalidOperationException("CONFIGURATION FAILED: Failed to configure access token settings");
            }

            var refreshTokenSettings = configuration.GetSection("RefreshTokenSettings").Get<RefreshTokenSettings>();
            if (refreshTokenSettings is null || refreshTokenSettings.LifeTimeInDays == 0)
            {
                Log.Logger.Error("CONFIGURATION FAILED: Failed to configure refresh token settings");
                throw new InvalidOperationException("CONFIGURATION FAILED: Failed to configure refresh token settings");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSettings.SigningKey));
            key.KeyId = accessTokenSettings.KeyId;

            var tokenValidationParams = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = accessTokenSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = accessTokenSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key
            };

            services.AddSingleton(accessTokenSettings);
            services.AddSingleton(tokenValidationParams);
            services.AddSingleton<IRefreshTokenSettings>(refreshTokenSettings);
            services.AddScoped<JsonWebTokenHandler>();

            services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => options.TokenValidationParameters = tokenValidationParams);

            return services;
        }
    }
}
