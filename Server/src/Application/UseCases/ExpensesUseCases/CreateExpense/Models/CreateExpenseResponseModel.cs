using Application.UseCases.NotificationsUseCases.Models;

namespace Application.UseCases.ExpensesUseCases.CreateExpense.Models
{
    public sealed record CreateExpenseResponseModel(Guid ExpenseId, List<NotificationDto> Notifications);
}
