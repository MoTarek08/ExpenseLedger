using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Domain.Entities.Notification;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationsUseCases.MarkNotificationAsRead
{
    public class MarkNotificationAsReadUseCase
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<MarkNotificationAsReadUseCase> _logger;

        public MarkNotificationAsReadUseCase(
            INotificationsRepository notificationsRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider,
            ILogger<MarkNotificationAsReadUseCase> logger)
        {
            _notificationsRepository = notificationsRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, Guid notificationId, CancellationToken cancellationToken)
        {
            var notification = await _notificationsRepository.FindVisibleAsync(notificationId, cancellationToken);
            if (notification is null || notification.UserId != userId)
            {
                _logger.LogWarning("Notification not found for marking as read {NotificationId} {UserId} {ErrorCode}", notificationId, userId, NotificationsErrorCodes.NOTIFICATION_NOT_FOUND);
                return Result.Failure(new Error(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND));
            }

            if (notification.ReadAt is not null)
                return Result.Success();

            notification.MarkAsRead(_dateTimeProvider.Now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification marked as read {NotificationId} {UserId}", notificationId, userId);
            return Result.Success();
        }
    }
}
