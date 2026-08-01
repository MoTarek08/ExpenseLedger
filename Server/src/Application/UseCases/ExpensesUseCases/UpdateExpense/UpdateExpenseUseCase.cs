using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.SharedServices;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.BudgetUseCases.Helpers;
using Application.UseCases.ExpensesUseCases.UpdateExpense.Models;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.Notification;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.UpdateExpense
{
    public class UpdateExpenseUseCase
    {
        private readonly IExpensesRepository _expensesRepository;
        private IUsersRepository _usersRepository;
        private ICategoriesRepository _categoriesRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckBudgetStateService _checkBudget;
        private readonly ILogger<UpdateExpenseUseCase> _logger;

        public UpdateExpenseUseCase(
            IExpensesRepository expensesRepository,
            ICategoriesRepository categoriesRepository,
            IUsersRepository usersRepository,
            INotificationsRepository notificationsRepository,
            IDateProvider dateTimeProvider,
            IBackgroundJobsService backgroundJobsService,
            IUnitOfWork unitOfWork,
            ICheckBudgetStateService checkBudget,
            ILogger<UpdateExpenseUseCase> logger)
        {
            _expensesRepository = expensesRepository;
            _usersRepository = usersRepository;
            _categoriesRepository = categoriesRepository;
            _notificationsRepository = notificationsRepository;
            _dateTimeProvider = dateTimeProvider;
            _backgroundJobsService = backgroundJobsService;
            _unitOfWork = unitOfWork;
            _checkBudget = checkBudget;
            _logger = logger;
        }

        public async Task<Result<List<NotificationDto>>> Execute(Guid userId, Guid expenseId, UpdateExpenseRequestModel requestModel, CancellationToken cancellationToken)
        {
       
            var expense = await _expensesRepository.FindAsync(expenseId, cancellationToken);
            if (expense is null || expense.UserId != userId)
            {
                _logger.LogWarning("Update failed: expense {ExpenseId} not found for user {UserId}", expenseId, userId);
                return Result<List<NotificationDto>>.Failure(new Error(ExpensesErrorCodes.EXPENSE_NOT_FOUND));
            }

            Notification? notification = null;
            var shouldBackgroundJobsBeTriggered = false;
            var shouldGoalAchievementChecksBeTriggered = false;


            if (requestModel.CategoryId is not null && requestModel.SubCategoryId is not null)
            {
                if (!await _categoriesRepository.SubBelongsToMainAsync(requestModel.CategoryId.Value, requestModel.SubCategoryId.Value, cancellationToken))
                    return Result<List<NotificationDto>>.Failure(new Error(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER));

                expense.ChangeMainCategory(requestModel.CategoryId.Value);
                expense.ChangeSubCategory(requestModel.SubCategoryId.Value);

                shouldBackgroundJobsBeTriggered = true;

            }

            else if (requestModel.CategoryId is not null)
            {
                expense.ChangeMainCategory(requestModel.CategoryId.Value);
                expense.ChangeSubCategory(null);

                shouldBackgroundJobsBeTriggered = true;

            }

            else if (requestModel.SubCategoryId is not null)
            {
                if (!await _categoriesRepository.SubBelongsToMainAsync(expense.CategoryId, requestModel.SubCategoryId.Value, cancellationToken))
                    return Result<List<NotificationDto>>.Failure(new Error(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER));

                expense.ChangeSubCategory(requestModel.SubCategoryId.Value);
            }

            if (requestModel.Title is not null)
                expense.ChangeTitle(requestModel.Title.Trim());


            if (requestModel.Amount is not null)
            {
                var orgAmount = expense.Amount;
                expense.ChangeAmount(requestModel.Amount.Value);

                if (requestModel.Amount.Value > orgAmount)
                {
                    notification = await _checkBudget.EvaluateAsync(expenseId, cancellationToken);

                    shouldBackgroundJobsBeTriggered = true;
                }
            }

            if (requestModel.SpentOn.HasValue)
            {
                if (expense.SpentOn != requestModel.SpentOn.Value)
                {
                    expense.ChangeSpentOn(requestModel.SpentOn.Value);

                    var userFinancialProfile = await _usersRepository.GetFinancialProfileByUserIdAsync(userId, cancellationToken);

                    var today = DateOnly.FromDateTime(_dateTimeProvider.Now.UtcDateTime);
                    var lastPayDay = BudgetComputingHelpers.GetLastPayDay(userFinancialProfile!.ResetDay, today);
                    if (expense.SpentOn >= lastPayDay && expense.SpentOn <= lastPayDay.AddMonths(1))
                    {
                        if (notification is null)
                            notification = await _checkBudget.EvaluateAsync(expenseId, cancellationToken);

                        shouldGoalAchievementChecksBeTriggered = true;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Expense updated {ExpenseId} for user {UserId}", expenseId, userId);

            var result = new List<NotificationDto>();

            if (notification is not null)
            {
                if (!await _notificationsRepository.ExistsByDedupKeyAsync(userId, notification.DedupKey, cancellationToken))
                {
                    _notificationsRepository.Add(notification);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    result.Add(new NotificationDto(
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
            }

            if (shouldGoalAchievementChecksBeTriggered && !shouldBackgroundJobsBeTriggered)
                _backgroundJobsService.TriggerBackgroundCheckGoalAchivement(expenseId);

            else if (shouldBackgroundJobsBeTriggered)
                _backgroundJobsService.TriggerAfterBackgroundExpenseCreationJobs(expenseId);

            else { };

            return Result<List<NotificationDto>>.Success(result);
        }
    }
}
