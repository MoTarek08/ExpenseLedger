using Infrastructure.ObjectStorage;
using Infrastructure.ObjectStorage.Clients;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Infrastructure.DependencyInjection.ObjectStorageClientConfiguration.Minio
{
    public static class ConfigureMinio
    {
        public static IServiceCollection ConfigureAndAddMinio(this IServiceCollection services, ObjectStorageSettings settings)
        {
            services.AddMinio(client => client
            .WithEndpoint(settings.Endpoint)
            .WithCredentials(settings.AccessKey, settings.SecretKey)
            .WithSSL(false));

            services.AddScoped<IObjectStorageClient, MinioApplicationClient>();
            return services;
        }
    }
}
