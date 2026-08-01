using Application.Interfaces.Repositories;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Application.UseCases.SpendingGoalsUseCases.Helpers;
using Domain.Entities.SpendingGoalNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class SpendingGoalsRepository : ISpendingGoalsRepository
    {
        private readonly AppDbContext _dbContext;

        public SpendingGoalsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(SpendingGoal goal)
        {
            _dbContext.SpendingGoals.Add(goal);
        }

        public void Remove(SpendingGoal goal)
        {
            _dbContext.SpendingGoals.Remove(goal);
        }

        public async Task<SpendingGoal?> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.SpendingGoals.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<SpendingGoal?> FindByIdAndUserIdAsync(Guid goalId, Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.SpendingGoals.FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId, cancellationToken);
        }

        public async Task<List<SpendingGoal>> FindByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.SpendingGoals
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SpendingGoal>> FindAffectedByExpenseAsync(
            Guid userId,
            Guid categoryId,
            DateOnly spentOn,
            Guid lastSeenId,
            int batchSize = 20)
        {
            return await _dbContext.SpendingGoals
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.StartsAt <= spentOn &&
                    x.EndsAt >= spentOn &&
                    (x.CategoryId == null || x.CategoryId == categoryId) &&
                    x.Id > lastSeenId)
                    .Take(batchSize)
                    .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<bool> ExistsForPeriodAsync(
            Guid userId,
            Guid? categoryId,
            DateOnly startDate,
            DateOnly endDate,
            Guid excludedGoalId,
            CancellationToken cancellationToken)
        {
            return 
                await _dbContext.SpendingGoals.AnyAsync(x =>
                x.Id != excludedGoalId &&
                x.UserId == userId &&
                x.CategoryId == categoryId &&
                x.StartsAt == startDate &&
                x.EndsAt == endDate, cancellationToken
                );
        }

        public async Task<SpendingGoalWithSpent?> GetGoalWithSpentAsync(
            Guid goalId, Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.SpendingGoals
                .Where(g => g.Id == goalId && g.UserId == userId)
                .AsExpandable()
                .Select(g => new SpendingGoalWithSpent(
                    g,
                    _dbContext.Expenses
                        .Where(e => SpendingGoalExpressions.ExpenseAffectsGoal.Invoke(e, g.UserId, g.CategoryId, g.StartsAt, g.EndsAt))
                        .Sum(e => (decimal?)e.Amount) ?? 0m))
                .FirstOrDefaultAsync(cancellationToken);
        }


        public async Task<List<SpendingGoalWithSpent>> GetGoalsWithSpentAsync(List<Guid> goalsIds)
        {
            return await _dbContext.SpendingGoals
                .Where(g => goalsIds.Contains(g.Id))
                .AsExpandable()
                .Select(g => new SpendingGoalWithSpent(
                    g,
                    _dbContext.Expenses
                        .Where(e => SpendingGoalExpressions.ExpenseAffectsGoal.Invoke(e, g.UserId, g.CategoryId, g.StartsAt, g.EndsAt))
                        .Sum(e => (decimal?)e.Amount) ?? 0m))
                .OrderByDescending(x => x.CurrentSpent)
                .AsNoTracking()
                .ToListAsync();
        }

        public IQueryable<SpendingGoal> GetAllForUserQuery(Guid userId)
        {
            return _dbContext.SpendingGoals.Where(g => g.UserId == userId);
        }

        public async Task<List<GetSpendingGoalsByStatusDto>> GetSucceededGoalsAsync(
           IQueryable<SpendingGoal> filteredQuery,
           DateOnly today,
           int pageNumber,
           int pageSize,
           CancellationToken cancellationToken)
        {
            return await filteredQuery
                .Where(g => g.EndsAt < today)
                .AsExpandable()
                .Select(g => new
                {
                    Goal = g,
                    CurrentSpent = _dbContext.Expenses
                        .Where(e => SpendingGoalExpressions.ExpenseAffectsGoal.Invoke(e, g.UserId, g.CategoryId, g.StartsAt, g.EndsAt))
                        .Sum(e => (decimal?)e.Amount) ?? 0m
                })
                .Where(x =>
                    x.CurrentSpent >= x.Goal.MinimumTargetAmount &&
                    x.CurrentSpent <= x.Goal.MaximumTargetAmount)
                .OrderByDescending(x => x.Goal.EndsAt)
                .ThenByDescending(x => x.Goal.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetSpendingGoalsByStatusDto(
                    x.Goal.Id,
                    x.Goal.Description ?? string.Empty,
                    x.Goal.CategoryId,
                    x.Goal.MinimumTargetAmount,
                    x.Goal.MaximumTargetAmount,
                    x.CurrentSpent,
                    x.Goal.StartsAt,
                    x.Goal.EndsAt,
                    x.Goal.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GetSpendingGoalsByStatusDto>> GetFailedGoalsAsync(
           IQueryable<SpendingGoal> filteredQuery,
           DateOnly today,
           int pageNumber,
           int pageSize,
           CancellationToken cancellationToken)
        {
            return await filteredQuery
                .Where(g => g.EndsAt < today)
                .AsExpandable()
                .Select(g => new
                {
                    Goal = g,
                    CurrentSpent = _dbContext.Expenses
                        .Where(e => SpendingGoalExpressions.ExpenseAffectsGoal.Invoke(e, g.UserId, g.CategoryId, g.StartsAt, g.EndsAt))
                        .Sum(e => (decimal?)e.Amount) ?? 0m
                })
                .Where(x =>
                    x.CurrentSpent < x.Goal.MinimumTargetAmount ||
                    x.CurrentSpent > x.Goal.MaximumTargetAmount)
                .OrderByDescending(x => x.Goal.EndsAt)
                .ThenByDescending(x => x.Goal.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetSpendingGoalsByStatusDto(
                    x.Goal.Id,
                    x.Goal.Description ?? string.Empty,
                    x.Goal.CategoryId,
                    x.Goal.MinimumTargetAmount,
                    x.Goal.MaximumTargetAmount,
                    x.CurrentSpent,
                    x.Goal.StartsAt,
                    x.Goal.EndsAt,
                    x.Goal.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GetSpendingGoalsByStatusDto>> GetInProgressGoalsAsync(
            IQueryable<SpendingGoal> filteredQuery,
            DateOnly today,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return await filteredQuery
                .Where(g => today >= g.StartsAt && today <= g.EndsAt)
                .AsExpandable()
                .Select(g => new
                {
                    Goal = g,
                    CurrentSpent = _dbContext.Expenses
                        .Where(e => SpendingGoalExpressions.ExpenseAffectsGoal.Invoke(e, g.UserId, g.CategoryId, g.StartsAt, g.EndsAt))
                        .Sum(e => (decimal?)e.Amount) ?? 0m
                })
                .OrderBy(x => x.Goal.EndsAt)
                .ThenByDescending(x => x.Goal.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetSpendingGoalsByStatusDto(
                    x.Goal.Id,
                    x.Goal.Description ?? string.Empty,
                    x.Goal.CategoryId,
                    x.Goal.MinimumTargetAmount,
                    x.Goal.MaximumTargetAmount,
                    x.CurrentSpent,
                    x.Goal.StartsAt,
                    x.Goal.EndsAt,
                    x.Goal.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GetSpendingGoalsByStatusDto>> GetPendingGoalsAsync(
            IQueryable<SpendingGoal> filteredQuery,
            DateOnly today,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return await filteredQuery
                .Where(g => today < g.StartsAt)
                .AsExpandable()
                .Select(g => new
                {
                    Goal = g,
                    CurrentSpent = _dbContext.Expenses
                        .Where(e => SpendingGoalExpressions.ExpenseAffectsGoal.Invoke(e, g.UserId, g.CategoryId, g.StartsAt, g.EndsAt))
                        .Sum(e => (decimal?)e.Amount) ?? 0m
                })
                .OrderBy(x => x.Goal.StartsAt)
                .ThenByDescending(x => x.Goal.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetSpendingGoalsByStatusDto(
                    x.Goal.Id,
                    x.Goal.Description ?? string.Empty,
                    x.Goal.CategoryId,
                    x.Goal.MinimumTargetAmount,
                    x.Goal.MaximumTargetAmount,
                    x.CurrentSpent,
                    x.Goal.StartsAt,
                    x.Goal.EndsAt,
                    x.Goal.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}
