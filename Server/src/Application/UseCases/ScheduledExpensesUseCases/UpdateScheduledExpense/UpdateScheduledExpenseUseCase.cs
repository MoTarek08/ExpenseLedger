using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense
{
    public class UpdateScheduledExpenseUseCase
    {
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly ILogger<UpdateScheduledExpenseUseCase> _logger;

        public UpdateScheduledExpenseUseCase(
            IScheduledExpensesRepository scheduledExpensesRepository,
            IUnitOfWork unitOfWork,
            IBackgroundJobsService backgroundJobsService,
            ILogger<UpdateScheduledExpenseUseCase> logger)
        {
            _scheduledExpensesRepository = scheduledExpensesRepository;
            _unitOfWork = unitOfWork;
            _backgroundJobsService = backgroundJobsService;
            _logger = logger;
        }

        public async Task<Result> Execute(
            Guid scheduledExpenseId,
            Guid userId,
            UpdateScheduledExpenseRequestModel requestModel,
            CancellationToken cancellationToken)
        {
            var scheduledExpense = await _scheduledExpensesRepository.FindAsync(scheduledExpenseId, cancellationToken);
            if (scheduledExpense is null || scheduledExpense.UserId != userId)
            {
                _logger.LogWarning("Scheduled expense not found {ScheduledExpenseId} {UserId}", scheduledExpenseId, userId);
                return Result.Failure(new Error(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND));
            }

            if (!scheduledExpense.IsActive)
            {
                _logger.LogWarning("Scheduled expense not active {ScheduledExpenseId} {UserId}", scheduledExpenseId, userId);
                return Result.Failure(new Error(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_ACTIVE));
            }

            var previousNextDueOn = scheduledExpense.NextDueOn;

            if (requestModel.FirstDue is not null)
            {
                if (scheduledExpense.LastProcessedAt is not null)
                    return Result.Failure(new Error(ExpensesErrorCodes.SCHEDULED_EXPENSE_PROCESSED_BEFORE_AND_CANNOT_CHANGE_FIRST_DUE));

                scheduledExpense.ChangeFirstDue(requestModel.FirstDue.Value);
            }

            if (requestModel.Title is not null)
                scheduledExpense.UpdateTitle(requestModel.Title);

            if (requestModel.Amount is not null)
                scheduledExpense.UpdateAmount(requestModel.Amount.Value);

            if (requestModel.Cadence is not null)
                scheduledExpense.ChangeCadence(requestModel.Cadence.Value);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (scheduledExpense.NextDueOn is not null && scheduledExpense.NextDueOn != previousNextDueOn)
            {
                _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                    scheduledExpense.Id,
                    scheduledExpense.NextDueOn.Value);
            }

            _logger.LogInformation("Scheduled expense updated {ScheduledExpenseId} {UserId}", scheduledExpenseId, userId);
            return Result.Success();
        }
    }
}
