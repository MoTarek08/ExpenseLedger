using Application.Interfaces.Repositories;
using Application.UseCases.ScheduledExpensesUseCases.Models;
using Domain.Entities.ScheduledExpenseNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class ScheduledExpensesRepository : IScheduledExpensesRepository
    {
        private readonly AppDbContext _dbContext;

        public ScheduledExpensesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(ScheduledExpense scheduledExpense)
        {
            _dbContext.ScheduledExpenses.Add(scheduledExpense);
        }

        public void Remove(ScheduledExpense scheduledExpense)
        {
            _dbContext.ScheduledExpenses.Remove(scheduledExpense);
        }

        public async Task<ScheduledExpense?> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ScheduledExpenses.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<ScheduledExpense?> FindIncludingCategoriesAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.ScheduledExpenses
                .Include(se => se.Category)
                .Include(se => se.SubCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(se => se.Id == id, cancellationToken);
        }

        public IQueryable<ScheduledExpense> GetAllForUserQuery(Guid userId)
        {
            return _dbContext.ScheduledExpenses
                .AsNoTracking()
                .Where(se => se.UserId == userId);
        }

        public async Task<List<ScheduledExpenseDto>> GetScheduledExpenseDtoAsync(IQueryable<ScheduledExpense> query, CancellationToken cancellationToken)
        {
            return await query
                .Select(se => new ScheduledExpenseDto(
                    se.Id,
                    se.IsActive,
                    se.Title,
                    se.Amount,
                    se.Cadence,
                    se.Category.Code,
                    se.SubCategory != null ? se.SubCategory.Code : null,
                    se.FirstDueOn,
                    se.NextDueOn,
                    se.LastProcessedAt,
                    se.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}
