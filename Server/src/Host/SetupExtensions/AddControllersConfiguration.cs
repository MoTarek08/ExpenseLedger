using Microsoft.AspNetCore.Mvc;

namespace Host.SetupExtensions
{
    public static class ControllersConfig
    {
        public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
        {
            services
            .AddControllers(options =>
            {options.Filters.Add(new ProducesAttribute("application/json"));})

            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problem = ModelStateProblemFactory.Create(context);
                    return new BadRequestObjectResult(problem);
                };
            });
            return services;
        }
    }
}
