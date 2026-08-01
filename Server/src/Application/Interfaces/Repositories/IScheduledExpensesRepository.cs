using Application.UseCases.ScheduledExpensesUseCases.Models;
using Domain.Entities.ScheduledExpenseNamespace;

namespace Application.Interfaces.Repositories
{
    public interface IScheduledExpensesRepository
    {
        public void Add(ScheduledExpense scheduledExpense);
        public void Remove(ScheduledExpense scheduledExpense);

        public Task<ScheduledExpense?> FindAsync(Guid id, CancellationToken cancellationToken = default);

        public Task<ScheduledExpense?> FindIncludingCategoriesAsync(Guid id, CancellationToken cancellationToken);

        public IQueryable<ScheduledExpense> GetAllForUserQuery(Guid userId);

        public Task<List<ScheduledExpenseDto>> GetScheduledExpenseDtoAsync(IQueryable<ScheduledExpense> query, CancellationToken cancellationToken);
    }
}
