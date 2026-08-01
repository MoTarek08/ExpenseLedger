using Application.Interfaces.BusinessQueries;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.BusinessQueries
{
    public class BudgetQueries : IBudgetQueries
    {
        private readonly AppDbContext _dbContext;

        public BudgetQueries(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> GetTotalSpentAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Expenses.Where(x => x.UserId == userId && x.SpentOn >= from && x.SpentOn <= to).SumAsync(x => x.Amount, cancellationToken);
        }

        public async Task<decimal> GetTotalSpentForCategoryAsync(Guid userId, Guid categoryId ,DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Expenses.Where(x =>
                x.UserId == userId &&
                x.CategoryId == categoryId &&
                x.SpentOn >= from && x.SpentOn <= to)
                .SumAsync(x => x.Amount, cancellationToken);
        }
    }
}
