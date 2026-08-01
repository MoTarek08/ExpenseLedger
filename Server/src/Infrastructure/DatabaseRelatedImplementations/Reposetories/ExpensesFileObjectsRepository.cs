using Application.Interfaces.Repositories;
using Domain.Entities.DomainEnums;
using Domain.Entities.FileObjectNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class ExpensesFileObjectsRepository : IExpensesFileObjectsRepository
    {
        private readonly AppDbContext _dbContext;

        public ExpensesFileObjectsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(ExpenseFileObject file)
        {
            _dbContext.ExpensesFileObjects.Add(file);
        }

        public async Task<ExpenseFileObject?> FindAsync(Guid fileId, CancellationToken cancellationToken)
        {
            return await _dbContext.ExpensesFileObjects.FindAsync(new object[] { fileId }, cancellationToken);
        }

        public async Task<List<ExpenseFileObject>> FindsStaleUploadsAsync(Guid lastSeenId, DateTimeOffset now, int batchSize)
        {
            var staleBatch = _dbContext.ExpensesFileObjects
                .Where(x => x.Id > lastSeenId && x.Status == FileObjectStatus.PendingUpload && x.UploadUrlExpiresAt < now.AddHours(-2))
                .OrderBy(x => x.Id)
                .Take(batchSize);

            return await staleBatch.ToListAsync();
        }

        public async Task<int> BulkDeleteAsync(List<Guid> ids)
        {
            return await _dbContext.ExpensesFileObjects.Where(f => ids.Contains(f.Id)).ExecuteDeleteAsync();
        }


    }
}
