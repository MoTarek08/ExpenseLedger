using Application.Interfaces.Repositories;
using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Microsoft.Extensions.Logging;
using System.Threading;
namespace Application.UseCases.NotificationsUseCases.RestoreNotification
{
    public class RestoreNotificationUseCase
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RestoreNotificationUseCase> _logger;

        public RestoreNotificationUseCase(
            INotificationsRepository notificationsRepository,
            IDateProvider dateTimeProvider,
            IUnitOfWork unitOfWork,
            ILogger<RestoreNotificationUseCase> logger)
        {
            _notificationsRepository = notificationsRepository;
            _dateTimeProvider = dateTimeProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, Guid notificationId, CancellationToken cancellationToken)
        {
            var notification = await _notificationsRepository.FindAsync(notificationId, cancellationToken);
            if (notification is null || notification.UserId != userId)
            {
                _logger.LogWarning("Notification not found for restore {NotificationId} {UserId} {ErrorCode}", notificationId, userId, NotificationsErrorCodes.NOTIFICATION_NOT_FOUND);
                return Result.Failure(new Error(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND));
            }

            if (notification.DeletedAt is null)
            {
                _logger.LogInformation("Notification is not deleted {NotificationId} {UserId}", notificationId, userId);
                return Result.Success();
            }

            if (notification.DeletedAt < _dateTimeProvider.Now.AddHours(-1))
            {
                _logger.LogWarning("Notification restore window expired {NotificationId} {UserId} {ErrorCode}", notificationId, userId, NotificationsErrorCodes.NOTIFICATION_NOT_FOUND);
                return Result.Failure(new Error(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND));
            }

            notification.Undelete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Notification restored {NotificationId} {UserId}", notificationId, userId);
            return Result.Success();
        }
    }
}
