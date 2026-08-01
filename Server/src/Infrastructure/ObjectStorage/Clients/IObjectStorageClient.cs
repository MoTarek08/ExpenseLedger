using Application.Interfaces.ObjectStorage.Models;

namespace Infrastructure.ObjectStorage.Clients
{
    public interface IObjectStorageClient
    {
        public Task<FileObjectInfo> GetFileObjectInfoAsync(string bucketName, string objectKey, CancellationToken cancellationToken);
        public Task<string> GeneratePreSignedUrlAsync(string bucketName, string objectKey, int expiryInMinuites);
        public Task RemoveObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken);
    }
}
