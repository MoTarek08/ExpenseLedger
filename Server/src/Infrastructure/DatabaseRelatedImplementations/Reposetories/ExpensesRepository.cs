using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Domain.Entities.ExpenseNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class ExpensesRepository : IExpensesRepository
    {
        private readonly AppDbContext _dbContext;

        public ExpensesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Expense expense)
        {
            _dbContext.Expenses.Add(expense);
        }

        public void Remove(Expense expense)
        {
            _dbContext.Expenses.Remove(expense);
        }

        public async Task<Expense?> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Expenses.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Expense?> FindIncludingFileObjectAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Expenses
                .Include(e => e.FileObject)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Expense?> FindExpenseIncludingCategoriesAndNotesAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Expenses
                .Include(e => e.Category)
                .Include(e => e.SubCategory)
                .Include(e => e.Notes)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<List<ExpenseDto>> GetAllForUserInDay(Guid userId, DateOnly day, CancellationToken cancellationToken)
        {
            return await ToExpenseDtoAsync(_dbContext.Expenses.Where(x => x.UserId == userId && x.SpentOn == day).AsNoTracking(), cancellationToken);
        }

        public async Task<List<ExpenseDto>> GetExpenseDtoAsync(IQueryable<Expense> query, CancellationToken cancellationToken)
        {
            return await ToExpenseDtoAsync(query, cancellationToken);
        }

        public IQueryable<Expense> GetAllForUserQuery(Guid userId)
        {
            return _dbContext.Expenses.Where(e => e.UserId == userId).OrderByDescending(p => p.SpentOn);
        }

        public IQueryable<Expense> GetAllForUserInDayQuery(Guid userId, DateOnly day)
        {
            return _dbContext.Expenses.Where(e => e.UserId == userId && e.SpentOn == day).OrderByDescending(p => p.SpentOn).ThenByDescending(e=>e.CreatedAt);
        }

        public async Task<Expense?> FindExpenseByScheduledExpenseId(Guid scheduledExpenseId, CancellationToken cancellationToken)
        {
            return await _dbContext.Expenses.FirstOrDefaultAsync(e => e.ScheduledExpenseId == scheduledExpenseId, cancellationToken);
        }

        public async Task<CheckBudgetAfterExpenseCreationModel?> GetCheckBudgetAfterExpenseCreationModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Expenses
                .Where(e => e.Id == id)
                .Select(e => new CheckBudgetAfterExpenseCreationModel(
                    e.SpentOn,
                    e.UserId,
                    e.User.FinancialProfile!.ResetDay,
                    e.User.FinancialProfile.MonthlyNetIncome))
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<ExpenseDto?> FindExpenseDtoByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Expenses
                .AsNoTracking()
                .Where(e => e.Id == id && e.UserId == userId)
                .Select(e => new ExpenseDto(
                    e.Id,
                    e.UserId,
                    e.SpentOn,
                    e.Title,
                    e.Amount,
                    e.Category.Code,
                    e.SubCategory != null ? e.SubCategory.Code : null,
                    e.ScheduledExpenseId,
                    e.Notes.Count))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<ExpenseDto>> ToExpenseDtoAsync(IQueryable<Expense> query, CancellationToken cancellationToken)
        {
            return await query.Select(x => new ExpenseDto(
                x.Id,
                x.UserId,
                x.SpentOn,
                x.Title,
                x.Amount,
                x.Category.Code,
                x.SubCategory != null ? x.SubCategory.Code : null,
                x.ScheduledExpenseId,
                x.Notes.Count
            )).ToListAsync(cancellationToken);
        }
    }
}
