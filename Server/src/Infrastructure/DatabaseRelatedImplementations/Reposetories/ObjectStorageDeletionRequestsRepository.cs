// COMMENTED OUT: Object storage deletion requests are no longer used.
// Deletion of file objects is now performed immediately when the owning entity is deleted.
// Keep this code for potential future use.
/*
using Application.Interfaces.Repositories;
using Domain.Entities.ObjectStorageDeletionRequestNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class ObjectStorageDeletionRequestsRepository : IObjectStorageDeletionRequestsRepository
    {
        private readonly AppDbContext _dbContext;

        public ObjectStorageDeletionRequestsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(ObjectStorageDeletionRequest request)
        {
            _dbContext.ObjectStorageDeletionRequests.Add(request);
        }

        public async Task<List<ObjectStorageDeletionRequest>> FindPendingAsync(Guid lastSeenId, int batchSize = 50)
        {
            return await _dbContext.ObjectStorageDeletionRequests
                .Where(r => r.ProcessedAt == null && r.Id > lastSeenId)
                .OrderBy(r => r.Id)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task<ObjectStorageDeletionRequest?> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ObjectStorageDeletionRequests.FindAsync(new object[] { id }, cancellationToken);
        }

        public void Remove(ObjectStorageDeletionRequest request)
        {
            _dbContext.Remove(request);
        }

    }
}
*/