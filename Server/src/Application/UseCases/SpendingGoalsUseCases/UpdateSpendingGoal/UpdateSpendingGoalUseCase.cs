using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.NotificationsUseCases.Models;
using Application.UseCases.SpendingGoalsUseCases.Helpers;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.Notification;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal
{
    public class UpdateSpendingGoalUseCase
    {
        private readonly ISpendingGoalsRepository _goalsRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<UpdateSpendingGoalUseCase> _logger;

        public UpdateSpendingGoalUseCase(
            ISpendingGoalsRepository goalsRepository,
            INotificationsRepository notificationsRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateProvider,
            ILogger<UpdateSpendingGoalUseCase> logger)
        {
            _goalsRepository = goalsRepository;
            _notificationsRepository = notificationsRepository;
            _unitOfWork = unitOfWork;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result<UpdateSpendingGoalResponseModel>> Execute(
        Guid goalId,
        Guid userId,
        UpdateSpendingGoalRequestModel requestModel,
        CancellationToken cancellationToken)
        {
            var goal = await _goalsRepository.FindAsync(goalId, cancellationToken);

            if (goal is null || goal.UserId != userId)
            {
                _logger.LogWarning("Spending goal not found for user {UserId}, goal {GoalId}", userId, goalId);
                return Result<UpdateSpendingGoalResponseModel>.Failure(new Error(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND));
            }

            if (goal.GetLifecycle(DateOnly.FromDateTime(_dateProvider.Now.UtcDateTime)) == GoalLifecycle.Completed)
            {
                _logger.LogWarning("Spending goal {GoalId} is completed for user {UserId}", goalId, userId);
                return Result<UpdateSpendingGoalResponseModel>.Failure(new Error(SpendingGoalsErrorCodes.SPENDING_GOAL_COMPLETED));
            }

            var onlyDescriptionUpdated = requestModel.Description is not null
                && requestModel.MinimumTargetAmount is null
                && requestModel.MaximumTargetAmount is null
                && requestModel.StartDate is null
                && requestModel.EndDate is null;

            if (requestModel.Description is not null)
                goal.UpdateDescription(requestModel.Description);

            if (requestModel.MinimumTargetAmount is not null || requestModel.MaximumTargetAmount is not null)
            {
                var newMin = requestModel.MinimumTargetAmount ?? goal.MinimumTargetAmount;
                var newMax = requestModel.MaximumTargetAmount ?? goal.MaximumTargetAmount;
                goal.UpdateTargets(newMin, newMax);
            }

            var proposedStartDate = requestModel.StartDate ?? goal.StartsAt;
            var proposedEndDate = requestModel.EndDate ?? goal.EndsAt;

            if (requestModel.StartDate is not null || requestModel.EndDate is not null)
            {
                if (await _goalsRepository.ExistsForPeriodAsync(
                        userId,
                        goal.CategoryId,
                        proposedStartDate,
                        proposedEndDate,
                        goalId))
                {
                    return Result<UpdateSpendingGoalResponseModel>.Failure(new Error(SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS));
                }
            }

            if (requestModel.StartDate is not null && requestModel.EndDate is not null)
                goal.Reschedule(proposedStartDate, proposedEndDate);

            else if (requestModel.StartDate is not null)
                goal.UpdateStartDate(proposedStartDate);

            else if (requestModel.EndDate is not null)
                goal.UpdateEndDate(proposedEndDate);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var notifications = new List<NotificationDto>();

            if (!onlyDescriptionUpdated)
            {
                var goalWithSpent = await _goalsRepository.GetGoalWithSpentAsync(goalId, userId, cancellationToken);
                if (goalWithSpent is not null)
                {
                    if (SpendingGoalsHelpers.CurrentlyMeetsTargets(goalWithSpent.Goal, goalWithSpent.CurrentSpent))
                    {
                        var notification = Notification.GoalAchieved(
                            goal.Id,
                            userId,
                            goal.CategoryId,
                            goal.StartsAt,
                            goal.EndsAt,
                            _dateProvider.Now);

                        if (!await _notificationsRepository.ExistsByDedupKeyAsync(notification.UserId, notification.DedupKey, cancellationToken))
                        {
                            _notificationsRepository.Add(notification);
                            try
                            {
                                await _unitOfWork.SaveChangesAsync(cancellationToken);
                                notifications.Add(new NotificationDto(
                                    notification.Id,
                                    notification.UserId,
                                    notification.Reason,
                                    notification.Type,
                                    notification.Title,
                                    notification.Body,
                                    notification.ReadAt,
                                    notification.ExpenseId,
                                    notification.SpendingGoalId,
                                    notification.ScheduledExpenseId,
                                    notification.CategoryId));
                            }
                            catch (NotificationDeuplicationKeyAlreadyExists) { }
                            }
                        }
                    }
                }

            _logger.LogInformation("Spending goal updated {GoalId} for user {UserId}", goalId, userId);
            return Result<UpdateSpendingGoalResponseModel>.Success(new UpdateSpendingGoalResponseModel(notifications));
        }
    }
}