using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.Models.Result;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.Notification;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Application.UseCases.NotificationsUseCases.GetNotificationById
{
    public class GetNotificationByIdUseCase
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly ILogger<GetNotificationByIdUseCase> _logger;

        public GetNotificationByIdUseCase(
            INotificationsRepository notificationsRepository,
            ILogger<GetNotificationByIdUseCase> logger)
        {
            _notificationsRepository = notificationsRepository;
            _logger = logger;
        }

        public async Task<Result<NotificationDto>> Execute(Guid userId, Guid notificationId, CancellationToken cancellationToken)
        {
            var notification = await _notificationsRepository.FindVisibleAsync(notificationId, cancellationToken);
            if (notification is null || notification.UserId != userId)
            {
                _logger.LogWarning("Notification not found {NotificationId} {UserId} {ErrorCode}", notificationId, userId, NotificationsErrorCodes.NOTIFICATION_NOT_FOUND);
                return Result<NotificationDto>.Failure(new Error(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND));
            }

            _logger.LogInformation("Notification retrieved {NotificationId} {UserId}", notificationId, userId);
            return Result<NotificationDto>.Success(new NotificationDto(
                notification.Id,
                notification.UserId,
                notification.Reason,
                notification.Type,
                notification.Title,
                notification.Body,
                notification.ReadAt,
                notification.ExpenseId,
                notification.SpendingGoalId,
                notification.ScheduledExpenseId,
                notification.CategoryId));
        }
    }
}
