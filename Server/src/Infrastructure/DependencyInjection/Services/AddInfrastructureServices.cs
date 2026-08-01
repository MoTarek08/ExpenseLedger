using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.HashingService;
using Application.Interfaces.SharedServices;
using Application.Interfaces.TokensServiceNamespace;
using Infrastructure.Authentecation.JwtAuthentication;
using Infrastructure.DateTimeProviderNamespace;
using Infrastructure.HashingServiceNamespace;
using Infrastructure.SharedServices;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection.Services
{
    public static class InfrastructureServicesExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IDateProvider, DateProvider>();
            services.AddScoped<IHashingService, HashingService>();
            services.AddScoped<ITokensService, TokensService>();
            services.AddScoped<ICheckBudgetStateService, CheckBudgetStateService>();
            services.AddScoped<IBuildExpenseService, BuildExpenseService>();
            return services;
        }
    }
}
