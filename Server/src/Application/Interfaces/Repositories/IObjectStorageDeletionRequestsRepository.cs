using Domain.Entities.ObjectStorageDeletionRequestNamespace;

namespace Application.Interfaces.Repositories
{
    public interface IObjectStorageDeletionRequestsRepository
    {
        void Add(ObjectStorageDeletionRequest request);
        void Remove(ObjectStorageDeletionRequest request);

        Task<ObjectStorageDeletionRequest?> FindAsync(Guid id, CancellationToken cancellationToken = default);

        Task<List<ObjectStorageDeletionRequest>> FindPendingAsync(Guid lastSeenId, int batchSize = 50);
    }
}
