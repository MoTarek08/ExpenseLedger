using Application.ApplicationConstantsNamesapce;
using Application.Models;
using Domain.Entities.DomainEnums;

namespace Application.UseCases.NotificationsUseCases.SearchNotifications.Models
{
    public sealed record SearchNotificationsQueryParameters(
        NotificationType? NotificationType,
        DateOnly? From,
        DateOnly? To,
        bool UnreadOnly = false,
        bool ReadOnly = false,
        string SortBy = ApplicationConstants.NotificationsSortOptions.CreationDate,
        string SortOrder = ApplicationConstants.SortOrders.Descending) 
        : PaginationParameters;
}
