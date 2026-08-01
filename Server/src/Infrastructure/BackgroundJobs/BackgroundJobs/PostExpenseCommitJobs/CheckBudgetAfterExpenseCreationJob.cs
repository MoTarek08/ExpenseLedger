using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.Repositories;
using Application.Interfaces.SharedServices;
using Application.Interfaces.UnitOfWork;

namespace Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs
{
    public class CheckBudgetAfterExpenseCreationJob
    {
        private readonly ICheckBudgetStateService _checkBudgetState;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CheckBudgetAfterExpenseCreationJob(
            ICheckBudgetStateService checkBudgetState,
            INotificationsRepository notificationsRepository,
            IUnitOfWork unitOfWork)
        {
            _checkBudgetState = checkBudgetState;
            _notificationsRepository = notificationsRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(Guid expenseId)
        {
            var notification = await _checkBudgetState.EvaluateAsync(expenseId);
            if (notification is null)
                return;

            if (await _notificationsRepository.ExistsByDedupKeyAsync(notification.UserId, notification.DedupKey))
                return;

            _notificationsRepository.Add(notification);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is NotificationDeuplicationKeyAlreadyExists)
            {
                return;
            }
        }
    }
}
