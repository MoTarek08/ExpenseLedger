using Host.Swagger.OperationFilters;
using Host.Swagger.ResponsesExamples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace Host.SetupExtensions
{
    public static class SwaggerConfigurationExtensions
    {
        public static IServiceCollection AddSwaggerGenConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerExamplesFromAssemblyOf<ExpenseDtoExample>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ExpenseLedger API",
                    Version = "v1",
                    Description = "Finance & Expenses tracking API"
                });

                options.ExampleFilters();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                options.OperationFilter<AuthorizationResponsesOperationFilter>();
                options.OperationFilter<ValidationErrorOperationFilter>();
                options.OperationFilter<InternalServerErrorOperationFilter>();
                options.OperationFilter<ProducesErrorOperationFilter>();
            });

            return services;
        }
    }
}
