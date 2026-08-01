using Application.Interfaces.ObjectStorage.Models;
using Application.Models;
using Domain.Entities.DomainEnums;
namespace Application.Interfaces.ObjectStorage
{
public interface IObjectStorageService
{
    StorageProvider Provider { get; }
    Task<string> GenerateUploadUrlAsync(ObjectKey objectKey, DateTimeOffset startsProcessingAt, DateTimeOffset uploadExpiresAt);
    Task<FileObjectInfo> GetFileInfoAsync(string objectKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
    double GetUploadUrlLifeTime();
}
}
