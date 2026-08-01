using Application.UseCases.NotificationsUseCases.Models;

namespace Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models
{
    public sealed record UpdateSpendingGoalResponseModel(List<NotificationDto> Notifications);
}
