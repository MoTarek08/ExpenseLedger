using Application.UseCases.NotificationsUseCases.SearchNotifications.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class SearchNotificationsQueryParametersExample : IExamplesProvider<SearchNotificationsQueryParameters>
    {
        public SearchNotificationsQueryParameters GetExamples() => new SearchNotificationsQueryParameters(
            NotificationType: NotificationType.Warning,
            From: new DateOnly(2026, 7, 1),
            To: new DateOnly(2026, 7, 21),
            UnreadOnly: true,
            ReadOnly: false,
            SortBy: "CreationDate",
            SortOrder: "Descending")
        {
            PageNumber = 1,
            PageSize = 20
        };
    }
}
