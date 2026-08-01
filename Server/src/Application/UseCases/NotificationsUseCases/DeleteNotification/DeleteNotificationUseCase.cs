using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationsUseCases.DeleteNotification
{
    public class DeleteNotificationUseCase
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<DeleteNotificationUseCase> _logger;

        public DeleteNotificationUseCase(
            INotificationsRepository notificationsRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider,
            ILogger<DeleteNotificationUseCase> logger)
        {
            _notificationsRepository = notificationsRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, Guid notificationId, CancellationToken cancellationToken)
        {
            var notification = await _notificationsRepository.FindAsync(notificationId, cancellationToken);
            if (notification is null || notification.DeletedAt.HasValue)
                return Result.Success();

            if (notification.UserId != userId)
                return Result.Failure(new Error(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND));

            notification.MarkAsDeleted(_dateTimeProvider.Now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification deleted {NotificationId} {UserId}", notificationId, userId);
            return Result.Success();
        }
    }
}
