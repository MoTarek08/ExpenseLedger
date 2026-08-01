using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Domain.Entities.SpendingGoalNamespace;

namespace Application.Interfaces.Repositories
{
    public interface ISpendingGoalsRepository
    {
        public void Add(SpendingGoal goal);
        public void Remove(SpendingGoal goal);

        public Task<SpendingGoal?> FindAsync(Guid id, CancellationToken cancellationToken);
        public Task<SpendingGoal?> FindByIdAndUserIdAsync(Guid goalId, Guid userId, CancellationToken cancellationToken);

        public Task<List<SpendingGoal>> FindByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        public Task<List<SpendingGoal>> FindAffectedByExpenseAsync(
            Guid userId,
            Guid categoryId,
            DateOnly spentOn,
            Guid lastSeenId,
            int batchSize);

        public Task<bool> ExistsForPeriodAsync(
            Guid userId,
            Guid? categoryId,
            DateOnly startDate,
            DateOnly endDate,
            Guid excludedGoalId = default,
            CancellationToken cancellationToken = default);

        public Task<List<GetSpendingGoalsByStatusDto>> GetSucceededGoalsAsync(
           IQueryable<SpendingGoal> filteredQuery,
           DateOnly today,
           int pageNumber,
           int pageSize,
           CancellationToken cancellationToken);

        public Task<List<GetSpendingGoalsByStatusDto>> GetFailedGoalsAsync(
        IQueryable<SpendingGoal> filteredQuery,
        DateOnly today,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

        public Task<List<GetSpendingGoalsByStatusDto>> GetInProgressGoalsAsync(
            IQueryable<SpendingGoal> filteredQuery,
            DateOnly today,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);

        public Task<List<GetSpendingGoalsByStatusDto>> GetPendingGoalsAsync(
            IQueryable<SpendingGoal> filteredQuery,
            DateOnly today,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);

        public Task<SpendingGoalWithSpent?> GetGoalWithSpentAsync(
            Guid goalId, Guid userId, CancellationToken cancellationToken);

        public Task<List<SpendingGoalWithSpent>> GetGoalsWithSpentAsync(List<Guid> goalsIds);

        public IQueryable<SpendingGoal> GetAllForUserQuery(Guid userId);

    }
}
