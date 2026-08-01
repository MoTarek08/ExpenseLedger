using Domain.Entities.Notification;

namespace Application.Interfaces.SharedServices
{
    public interface ICheckBudgetStateService
    {
        public Task<Notification?> EvaluateAsync(Guid expenseId, CancellationToken cancellationToken = default);
    }
}