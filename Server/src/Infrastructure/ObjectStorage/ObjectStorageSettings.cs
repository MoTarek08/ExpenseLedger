using Domain.Entities.DomainEnums;

namespace Infrastructure.ObjectStorage
{
    public sealed record ObjectStorageSettings(
        string Endpoint,
        StorageProvider StorageProvider,
        string AccessKey,
        string SecretKey,
        string BucketName,
        double UploadUrlLifeTimeInMinuites,
        string Region,
        bool ForcePathStyle);
}
