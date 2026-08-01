using Infrastructure.ObjectStorage;
using Infrastructure.ObjectStorage.Clients;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.ILM;
using Serilog;
using System.Collections.ObjectModel;

namespace Infrastructure.DependencyInjection.ObjectStorageClientConfiguration.Minio
{
    public static class ConfigureMinioLifecycle
    {
        public static async Task SetLifecycleConfiguration(IMinioClient minioClient, ObjectStorageSettings settings)
        {
            var config = new LifecycleConfiguration()
            {
                Rules = new Collection<LifecycleRule>()
                {
                    new LifecycleRule
                    {
                        ID = "UploadAgeManagement",
                        Status = "Enabled",
                        Expiration = new Expiration { Days = 90, }
                    }
                }
            };

            try
            {
                await minioClient.SetBucketLifecycleAsync(new SetBucketLifecycleArgs()
                        .WithBucket(settings.BucketName)
                        .WithLifecycleConfiguration(config));

                Log.Information("MinIO bucket lifecycle policy configured successfully.");
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to configuer MinIO bucket lifecycle policy: {Message}", ex.Message);
                // Non-fatal — app should still start
            }
        }
    }
}
