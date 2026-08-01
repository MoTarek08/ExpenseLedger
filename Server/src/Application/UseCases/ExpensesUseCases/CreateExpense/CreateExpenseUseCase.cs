using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.SharedServices;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.CreateExpense.Models;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Application.UseCases.NotificationsUseCases.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.CreateExpenseNamespace
{
    public class CreateExpenseUseCase 
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly ICheckBudgetStateService _checkBudgetState;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IBuildExpenseService _buildExpense;
        private readonly ILogger<CreateExpenseUseCase> _logger;

        public CreateExpenseUseCase(
            IExpensesRepository expensesRepository,
            IUnitOfWork unitOfWork,
            IBackgroundJobsService backgroundJobsService,
            ICheckBudgetStateService budgetEvaluator,
            INotificationsRepository notificationsRepository,
            IBuildExpenseService buildExpenseService,
            ILogger<CreateExpenseUseCase> logger)
        {
            _expensesRepository = expensesRepository;
            _unitOfWork = unitOfWork;
            _backgroundJobsService = backgroundJobsService;
            _checkBudgetState = budgetEvaluator;
            _notificationsRepository = notificationsRepository;
            _buildExpense = buildExpenseService;
            _logger = logger;
        }

        public async Task<Result<CreateExpenseResponseModel>> Execute(Guid userId, CreateExpenseRequestModel requestModel, CancellationToken cancellationToken)
        {
            var buildingResult = await _buildExpense.BuildExpense(userId, requestModel, cancellationToken);
            if (buildingResult.IsFailure)
                return Result<CreateExpenseResponseModel>.Failure(buildingResult.Error!);

            var expense = buildingResult.Data!;
            _expensesRepository.Add(expense);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Expense created {ExpenseId} for user {UserId}", expense.Id, userId);

            var notifications = new List<NotificationDto>();
            var budgetNotification = await _checkBudgetState.EvaluateAsync(expense.Id, cancellationToken);

            if (budgetNotification is not null)
            {
                if (!await _notificationsRepository.ExistsByDedupKeyAsync(userId, budgetNotification.DedupKey, cancellationToken))
                {
                    _notificationsRepository.Add(budgetNotification);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    notifications.Add(new NotificationDto(
                        budgetNotification.Id,
                        budgetNotification.UserId,
                        budgetNotification.Reason,
                        budgetNotification.Type,
                        budgetNotification.Title,
                        budgetNotification.Body,
                        budgetNotification.ReadAt,
                        budgetNotification.ExpenseId,
                        budgetNotification.SpendingGoalId,
                        budgetNotification.ScheduledExpenseId,
                        budgetNotification.CategoryId));
                }
            }

            _backgroundJobsService.TriggerAfterManualExpenseCreationJobs(expense.Id);

            return Result<CreateExpenseResponseModel>.Success(new CreateExpenseResponseModel(expense.Id, notifications));
        }
    }
}
