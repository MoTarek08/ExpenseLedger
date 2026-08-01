using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.Notification;

namespace Application.Interfaces.Repositories
{
public interface INotificationsRepository
    {
        public void Add(Notification notification);

        public Task<Notification?> FindAsync(Guid notificationId, CancellationToken cancellationToken);

        public Task<Notification?> FindVisibleAsync(Guid notificationId, CancellationToken cancellationToken);

        public Task<bool> ExistsByDedupKeyAsync(Guid userId, string dedupKey, CancellationToken cancellationToken = default);

        public Task<List<NotificationDto>> GetNotificationDtoAsync(IQueryable<Notification> query, CancellationToken cancellationToken);

        public IQueryable<Notification> GetAllVisibleForUserQuery(Guid userId);
        public IQueryable<Notification> GetVisibleInPeriodQuery(Guid userId, DateOnly from, DateOnly to);

        public Task<List<Notification>> ToListAsync(IQueryable<Notification> query, CancellationToken cancellationToken);
    }
}
