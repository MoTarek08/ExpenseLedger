using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.SpendingGoalsUseCases.Helpers;
using Domain.Entities.Notification;
using Domain.Entities.SpendingGoalNamespace;

namespace Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs
{
    public class CheckGoalAchievement
    {
        private readonly ISpendingGoalsRepository _goalsRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;

        public CheckGoalAchievement(
            ISpendingGoalsRepository goalsRepository,
            INotificationsRepository notificationsRepository,
            IExpensesRepository expensesRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider)
        {
            _goalsRepository = goalsRepository;
            _notificationsRepository = notificationsRepository;
            _expensesRepository = expensesRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task Execute(Guid expenseId)
        {
            var expense = await _expensesRepository.FindAsync(expenseId);
            if (expense is null)
                return;

            var lastSeenId = Guid.Empty;

            while (true)
            {
                var batch = await _goalsRepository.FindAffectedByExpenseAsync(
                    expense.UserId,
                    expense.CategoryId,
                    expense.SpentOn,
                    lastSeenId,
                    20);

                if (batch.Count == 0)
                    break;

                lastSeenId = batch.Last().Id;

                var goalsWithSpent = await _goalsRepository.GetGoalsWithSpentAsync(batch.Select(g => g.Id).ToList());
                foreach (var goalWithSpent in goalsWithSpent)
                {
                    if (!SpendingGoalsHelpers.CurrentlyMeetsTargets(goalWithSpent.Goal, goalWithSpent.CurrentSpent))
                        continue;

                    var notification = Notification.GoalAchieved(
                        goalWithSpent.Goal.Id,
                        expense.UserId,
                        goalWithSpent.Goal.CategoryId,
                        goalWithSpent.Goal.StartsAt,
                        goalWithSpent.Goal.EndsAt,
                        _dateTimeProvider.Now);

                    if (await _notificationsRepository.ExistsByDedupKeyAsync(expense.UserId, notification.DedupKey))
                        continue;

                    _notificationsRepository.Add(notification);

                    try
                    {
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (NotificationDeuplicationKeyAlreadyExists) { }
                }
            }
            }

    }
}
