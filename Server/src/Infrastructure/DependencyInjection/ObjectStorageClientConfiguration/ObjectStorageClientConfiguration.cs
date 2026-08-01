using Infrastructure.DependencyInjection.ObjectStorageClientConfiguration.Minio;
using Infrastructure.ObjectStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.DependencyInjection.ObjectStorageClientConfiguration
{
    public static class ObjectStorageClientConfiguration
    {
        public static IServiceCollection AddObjectStorageClient(this IServiceCollection services, IConfiguration configuration)
        {

            var objectStorageSettings = configuration.GetSection("ObjectStorageSettings").Get<ObjectStorageSettings>();

            if (objectStorageSettings is null)
                Log.Logger.Error("CONFIGURATION FAILED: Failed to configure object storage settings");
            else
            {
                services.AddSingleton(objectStorageSettings);

                // !!!!
                services.ConfigureAndAddMinio(objectStorageSettings);
                // !!!
            }

            return services;

        }
    }
}
