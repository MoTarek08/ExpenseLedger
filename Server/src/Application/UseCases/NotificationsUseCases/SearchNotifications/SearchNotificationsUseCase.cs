using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Models.Result;
using Application.UseCases.NotificationsUseCases.Models;
using Application.UseCases.NotificationsUseCases.SearchNotifications.Models;
using Domain.Entities.Notification;
using Application.Interfaces.Repositories.Extensions;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using static Application.ApplicationConstantsNamesapce.ApplicationConstants;

namespace Application.UseCases.NotificationsUseCases.SearchNotifications
{
    public class SearchNotificationsUseCase
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<SearchNotificationsUseCase> _logger;

        public SearchNotificationsUseCase(
            INotificationsRepository notificationsRepository,
            IDateProvider dateTimeProvider,
            ILogger<SearchNotificationsUseCase> logger)
        {
            _notificationsRepository = notificationsRepository;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<Result<List<NotificationDto>>> Execute(Guid userId, SearchNotificationsQueryParameters queryParameters, CancellationToken cancellationToken)
        {
            var query = _notificationsRepository.GetAllVisibleForUserQuery(userId);

            if (queryParameters.NotificationType.HasValue)
                query = query.Where(n => n.Type == queryParameters.NotificationType.Value);

            if (queryParameters.ReadOnly)
                query = query.Where(n => n.ReadAt != null);

            else if (queryParameters.UnreadOnly)
                query = query.Where(n => n.ReadAt == null);

            var today = _dateTimeProvider.Today;

            if (queryParameters.From.HasValue && queryParameters.To.HasValue)
                query = query.GetBetweenDates(queryParameters.From.Value, queryParameters.To.Value);

            else if (queryParameters.From.HasValue)
                query = query.GetBetweenDates(queryParameters.From.Value,today);

            else if (queryParameters.To.HasValue)
                query = query.GetBetweenDates(_dateTimeProvider.MinDayValue,queryParameters.To.Value);

            query = queryParameters.SortOrder.ToUpperInvariant() == SortOrders.Ascending
                ? query.OrderBy(GetSortExpression(queryParameters.SortBy))
                : query.OrderByDescending(GetSortExpression(queryParameters.SortBy));

            var data = await _notificationsRepository
               .GetNotificationDtoAsync(
               query
               .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
               .Take(queryParameters.PageSize),
               cancellationToken);

            _logger.LogInformation("Searched notifications found {Count} {UserId}", data.Count, userId);
            return Result<List<NotificationDto>>.Success(data);

        }


        private static Expression<Func<Notification, object>> GetSortExpression(string sortBy) =>
        sortBy.ToUpperInvariant() switch
        {
            "CREATIONDATE" => n => n.CreatedAt,
            "NOTIFICATIONTYPE" => n => n.Type,
            _ => n => n.CreatedAt
        };
    }
}
