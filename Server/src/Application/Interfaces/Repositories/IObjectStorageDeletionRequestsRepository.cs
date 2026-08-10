// COMMENTED OUT: Object storage deletion requests are no longer used.
// Deletion of file objects is now performed immediately when the owning entity is deleted.
// Keep this code for potential future use.
/*
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
*/