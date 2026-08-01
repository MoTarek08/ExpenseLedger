using Application.Interfaces.ObjectStorage;
using Application.Interfaces.ObjectStorage.Models;
using Application.Models;
using Domain.Entities.DomainEnums;
using Infrastructure.ObjectStorage.Clients;

namespace Infrastructure.ObjectStorage
{
    public class ObjectStorageService : IObjectStorageService
    {
        private readonly ObjectStorageSettings _settings;
        private readonly IObjectStorageClient _objectStorageClient;

        public ObjectStorageService(
            IObjectStorageClient objectStorageClient,
            ObjectStorageSettings settings)
        {
            _objectStorageClient = objectStorageClient;
            _settings = settings;
        }

        public StorageProvider Provider => _settings.StorageProvider;

        public async Task<FileObjectInfo> GetFileInfoAsync(string objectKey, CancellationToken cancellationToken)
        {
            return await _objectStorageClient.GetFileObjectInfoAsync(_settings.BucketName, objectKey, cancellationToken);
        }


        public async Task<string> GenerateUploadUrlAsync(
            ObjectKey objectKey,
            DateTimeOffset startsProcessingAt,
            DateTimeOffset uploadExpiresAt)
        {
            return await _objectStorageClient.GeneratePreSignedUrlAsync(_settings.BucketName, objectKey.Value, (uploadExpiresAt - startsProcessingAt).Minutes);
        }

        public async Task DeleteAsync(string objectKey,CancellationToken cancellationToken)
        {
            await _objectStorageClient.RemoveObjectAsync(_settings.BucketName, objectKey, cancellationToken);
        }

        public double GetUploadUrlLifeTime() => _settings.UploadUrlLifeTimeInMinuites;
    }
}