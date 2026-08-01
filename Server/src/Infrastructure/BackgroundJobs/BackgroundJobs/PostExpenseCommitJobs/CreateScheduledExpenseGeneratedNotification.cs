using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.Notification;

namespace Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs
{
    public class CreateScheduledExpenseGeneratedNotification
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public CreateScheduledExpenseGeneratedNotification(
            IExpensesRepository expensesRepository,
            INotificationsRepository notificationsRepository,
            IDateProvider dateTimeProvider,
            IUnitOfWork unitOfWork)
        {
            _expensesRepository = expensesRepository;
            _notificationsRepository = notificationsRepository;
            _dateTimeProvider = dateTimeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(Guid expenseId)
        {
            var expense = await _expensesRepository.FindAsync(expenseId);
            if (expense is null || expense.ScheduledExpenseId is null || expense.ScheduledGenerationDate is null)
                return;

            var now = _dateTimeProvider.Now;
            var notification = Notification.ScheduledExpenseProcessed(
                expense.UserId,
                expense.Id,
                expense.ScheduledExpenseId.Value,
                expense.Title,
                expense.ScheduledGenerationDate.Value,
                now);

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
