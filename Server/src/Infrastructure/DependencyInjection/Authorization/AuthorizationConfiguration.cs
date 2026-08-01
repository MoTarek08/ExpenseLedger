using Infrastructure.Authorization.Policies.HasFinancialProfileNamespace;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection.Authorization
{
    public static class AuthorizationConfigurationExtensions
    {
        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, HasFinancialProfileHandler>();

            services.AddAuthorization(config =>
            {
                config.AddPolicy(
                    PoliciesNamesConstants.HasFinancialProfile,
                    ploicyBuilder => ploicyBuilder.AddRequirements(new HasFinancialProfileRequirement()));
            });

            return services;
        }
    }
}
