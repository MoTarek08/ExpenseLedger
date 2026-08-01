using Application.Interfaces.Repositories;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.Notification;
using Domain.Entities.UserNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class NotificationsRepository : INotificationsRepository
    {
        private readonly AppDbContext _dbContext;

        public NotificationsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Notification notification)
        {
            _dbContext.Notifications.Add(notification);
        }

        public async Task<Notification?> FindAsync(Guid notificationId, CancellationToken cancellationToken)
        {
            return await _dbContext.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
        }

        public async Task<Notification?> FindVisibleAsync(Guid notificationId, CancellationToken cancellationToken)
        {
            return await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.DeletedAt == null, cancellationToken);
        }

        public async Task<bool> ExistsByDedupKeyAsync(Guid userId, string dedupKey, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Notifications.AnyAsync(x =>
                x.UserId == userId &&
                x.DedupKey == dedupKey, cancellationToken);
        }

        public async Task<List<NotificationDto>> GetNotificationDtoAsync(IQueryable<Notification> query, CancellationToken cancellationToken)
        {
            return await query.Select(n => new NotificationDto(
                n.Id,
                n.UserId,
                n.Reason,
                n.Type,
                n.Title,
                n.Body,
                n.ReadAt,
                n.ExpenseId,
                n.SpendingGoalId,
                n.ScheduledExpenseId,
                n.CategoryId)).ToListAsync(cancellationToken);
        }

        public IQueryable<Notification> GetAllVisibleForUserQuery(Guid userId)
        {
            return _dbContext.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.UserId == userId &&
                    n.DeletedAt == null)
                .OrderByDescending(n => n.CreatedAt);
        }

        public IQueryable<Notification> GetVisibleInPeriodQuery(
            Guid userId,
            DateOnly from, DateOnly to)
        {
            var fromDateTime = new DateTimeOffset(
                from.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);

            var toExclusive = new DateTimeOffset(
                to.AddDays(1).ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);

            return _dbContext.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.UserId == userId &&
                    n.DeletedAt == null &&
                    n.CreatedAt >= fromDateTime &&
                    n.CreatedAt < toExclusive)
                .OrderByDescending(n => n.CreatedAt);      
        }

        public async Task<List<Notification>> ToListAsync(IQueryable<Notification> query, CancellationToken cancellationToken)
        {
            return await query.ToListAsync(cancellationToken);
        }
    }
}
