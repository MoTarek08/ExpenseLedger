using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class NotificationDtoListExample : IExamplesProvider<List<NotificationDto>>
    {
        public List<NotificationDto> GetExamples()
        {
            return new List<NotificationDto>()
            {
                new NotificationDto(
                    Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c"),
                    Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930"),
                    NotificationReason.BudgetWentBelowQuarter,
                    NotificationType.Warning,
                    "Budget is low",
                    "Your budget went below quarter",
                    null,
                    Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a"),
                    null,
                    null,
                    null),

                new NotificationDto(
                    Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                    Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930"),
                    NotificationReason.SpendingOnAvoidPreference,
                    NotificationType.Warning,
                    "Avoided category spent on",
                    "You spent on Entertainment, which you marked as avoid.",
                    DateTimeOffset.Parse("2026-07-20T14:30:00+00:00"),
                    Guid.Parse("7e32359e-c0ec-4f10-bec1-f161d0ae2569"),
                    null,
                    null,
                    Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"))
            };
        }
    }
}
