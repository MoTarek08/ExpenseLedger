using Domain.Entities.FileObjectNamespace;

namespace Application.Interfaces.Repositories
{
    public interface IExpensesFileObjectsRepository
    {
        public void Add(ExpenseFileObject file);
        public void Remove(ExpenseFileObject file);  
        public Task<int> BulkDeleteAsync(List<Guid> ids);

        public Task<ExpenseFileObject?> FindAsync(Guid fileId, CancellationToken cancellationToken);

        public Task<List<ExpenseFileObject>> FindsStaleUploadsAsync(Guid lastSeenId, DateTimeOffset staleDateTime, int batchSize = 50);
    }
}
