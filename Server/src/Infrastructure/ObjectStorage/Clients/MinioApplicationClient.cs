using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.ObjectStorage.Models;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Infrastructure.ObjectStorage.Clients
{
    public class MinioApplicationClient : IObjectStorageClient
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinioApplicationClient> _logger;

        public MinioApplicationClient(IMinioClient minioClient,ILogger<MinioApplicationClient> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }


        public async Task<FileObjectInfo> GetFileObjectInfoAsync(string bucketName, string objectKey, CancellationToken cancellationToken)
        {
            var statObjectArgs = new StatObjectArgs().WithBucket(bucketName).WithObject(objectKey);
            try
            {
                var objectStat = await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);
                return new FileObjectInfo(true, objectStat.Size);
            }

            catch (MinioException)
            {
                return new FileObjectInfo(false);
            }
        }


        public async Task<string> GeneratePreSignedUrlAsync(string bucketName, string objectKey, int expiryInMinuites)
        {
            var args = new PresignedPutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithExpiry(expiryInMinuites);

            return await _minioClient.PresignedPutObjectAsync(args); ;
        }

        public async Task RemoveObjectAsync(string bucketName, string objectKey,CancellationToken cancellationToken)
        {
            var args = new RemoveObjectArgs().WithBucket(bucketName).WithObject(objectKey);
            try
            {
                await _minioClient.RemoveObjectAsync(args, cancellationToken);
            }
            catch (MinioException)
            {
                _logger.LogInformation("Object already deleted from storage {ObjectKey}", objectKey);
                throw new FileObjectAlreadyDeleted();
            }
        }
    }
}
