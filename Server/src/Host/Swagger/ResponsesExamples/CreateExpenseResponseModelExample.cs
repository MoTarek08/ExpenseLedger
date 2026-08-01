using Application.UseCases.ExpensesUseCases.CreateExpense.Models;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.DomainEnums;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CreateExpenseResponseModelExample : IExamplesProvider<CreateExpenseResponseModel>
    {
        public CreateExpenseResponseModel GetExamples() => new(
            Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a"),
            new List<NotificationDto>
            {
                new(
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
                    null)
            });
    }
}
