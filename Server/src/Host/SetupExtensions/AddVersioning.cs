using Asp.Versioning;

namespace Host.SetupExtensions
{
    public static class Versioning
    {
        public static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            var defaultVersion = new ApiVersion(1, minorVersion: 0);
            services.AddApiVersioning(options =>
            {           
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = defaultVersion;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            }).AddMvc(options =>
            {

            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.DefaultApiVersion = defaultVersion;
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
