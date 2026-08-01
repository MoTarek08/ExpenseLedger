using Domain.Entities.Notification;

namespace Application.Interfaces.Repositories.Extensions
{
    public static class IQueryableExtenstions
    {
        public static IQueryable<Notification> GetBetweenDates(this IQueryable<Notification> query, DateOnly from, DateOnly to)
        {
            var fromDateTime = new DateTimeOffset(
                from.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);

            var toExclusive = new DateTimeOffset(
                to.AddDays(1).ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);

            return query
                .Where(n =>
                    n.CreatedAt >= fromDateTime &&
                    n.CreatedAt < toExclusive);
        }
    }
}
