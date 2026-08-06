using Infrastructure.ObjectStorage;
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
                var bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(settings.BucketName));

                if (!bucketExists)
                {
                    await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(settings.BucketName));
                    Log.Information("Bucket {BucketName} created", settings.BucketName);
                }

                await minioClient.SetBucketLifecycleAsync(new SetBucketLifecycleArgs()
                        .WithBucket(settings.BucketName)
                        .WithLifecycleConfiguration(config));

                Log.Information("MinIO bucket lifecycle policy configured successfully.");
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to configure MinIO bucket lifecycle policy: {Message}", ex.Message);
                // Non-fatal — app should still start
            }
        }
    }
}
