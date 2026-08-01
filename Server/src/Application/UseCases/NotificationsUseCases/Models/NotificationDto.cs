using Domain.Entities.DomainEnums;

namespace Application.UseCases.NotificationsUseCases.Models
{
    public sealed record NotificationDto(
        Guid Id,
        Guid UserId,
        NotificationReason Reason,
        NotificationType Type,
        string Title,
        string Body,
        DateTimeOffset? ReadAt,
        Guid? RelatedExpenseId,
        Guid? RelatedSpendingGoalId,
        Guid? RelatedScheduledExpenseId,
        Guid? RelatedCategoryId);
}
