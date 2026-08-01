using Infrastructure.DependencyInjection.ObjectStorageClientConfiguration.Minio;
using Infrastructure.ObjectStorage;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Infrastructure.DependencyInjection.ObjectStorageClientConfiguration
{
    public class ObjectStorageClientLifecycleConfiguration
    {
        public static async Task AddLifecycleConfiguration(IServiceProvider servicesProvider)
        {
            var settigns = servicesProvider.GetRequiredService<ObjectStorageSettings>();
            if(settigns is not null)
            {
                var minIoClient = servicesProvider.GetRequiredService<IMinioClient>();
                await ConfigureMinioLifecycle.SetLifecycleConfiguration(minIoClient, settigns);
            }

        }
    }
}
