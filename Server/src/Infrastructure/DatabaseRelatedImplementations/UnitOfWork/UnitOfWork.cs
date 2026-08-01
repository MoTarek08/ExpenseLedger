using Application.Interfaces.UnitOfWork;
using Infrastructure.Database.AppDbContextNamespace;
using Infrastructure.Database.DatabaseExceptionHandlersNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            catch (Exception ex)
            {
                if (ex is DbUpdateException dbUpdateException)
                    HandlingDatabaseExceptionsService.Handle(dbUpdateException);

                else throw;
            }
        }
    }
}
