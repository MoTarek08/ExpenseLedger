using Application.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Domain.Entities.ExpenseNamespace;

namespace Application.Interfaces.RepositoriesNamespace
{
    public interface IExpensesRepository
    {
        public void Add(Expense expense);
        public void Remove(Expense expense);

        public Task<Expense?> FindAsync(Guid id, CancellationToken cancellationToken = default);
        public Task<Expense?> FindIncludingFileObjectAsync(Guid id, CancellationToken cancellationToken);
        public Task<Expense?> FindExpenseIncludingCategoriesAndNotesAsync(Guid id, CancellationToken cancellationToken);
        public Task<Expense?> FindExpenseByScheduledExpenseId(Guid scheduledExpenseId, CancellationToken cancellationToken);

        public Task<List<ExpenseDto>> GetAllForUserInDay(Guid userId, DateOnly day, CancellationToken cancellationToken);

        public Task<List<ExpenseDto>> GetExpenseDtoAsync(IQueryable<Expense> query, CancellationToken cancellationToken);

        public IQueryable<Expense> GetAllForUserQuery(Guid userId);
        public IQueryable<Expense> GetAllForUserInDayQuery(Guid userId, DateOnly day);


        public Task<CheckBudgetAfterExpenseCreationModel?> GetCheckBudgetAfterExpenseCreationModelAsync(Guid id, CancellationToken cancellationToken = default);

        public Task<ExpenseDto?> FindExpenseDtoByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        public Task<List<ExpenseDto>> ToExpenseDtoAsync(IQueryable<Expense> query, CancellationToken cancellationToken);

    }
}
