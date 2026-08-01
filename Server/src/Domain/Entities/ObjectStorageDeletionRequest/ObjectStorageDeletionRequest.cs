using Domain.Entities.DomainEnums;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.ObjectStorageDeletionRequestNamespace
{
    public sealed class ObjectStorageDeletionRequest
    {
        public Guid Id { get; private set; }
        public string ObjectKey { get; private set; } = null!;
        public StorageProvider StorageProvider { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? ProcessedAt { get; private set; }

        private ObjectStorageDeletionRequest() { }

        private ObjectStorageDeletionRequest(
            string objectKey,
            StorageProvider storageProvider,
            DateTimeOffset createdAt)
        {
            Id = Guid.NewGuid();
            ObjectKey = objectKey;
            StorageProvider = storageProvider;
            CreatedAt = createdAt;
        }

        public static ObjectStorageDeletionRequest Create(
            string objectKey,
            StorageProvider storageProvider,
            DateTimeOffset createdAt)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
                throw new DomainException("Object key is required.");

            return new ObjectStorageDeletionRequest(objectKey, storageProvider, createdAt);
        }

        public void MarkAsProcessed(DateTimeOffset processedAt)
        {
            if (processedAt < CreatedAt)
                throw new DomainException("Invalid processing timestamp");

            ProcessedAt = processedAt;
        }
    }
}
