using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ScheduledExpensesUseCases.DeleteScheduledExpense
{
    public class DeleteScheduledExpenseUseCase
    {
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteScheduledExpenseUseCase> _logger;

        public DeleteScheduledExpenseUseCase(
            IScheduledExpensesRepository scheduledExpensesRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteScheduledExpenseUseCase> logger)
        {
            _scheduledExpensesRepository = scheduledExpensesRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(
            Guid scheduledExpenseId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var scheduledExpense = await _scheduledExpensesRepository.FindAsync(scheduledExpenseId, cancellationToken);
            if (scheduledExpense is null || scheduledExpense.UserId != userId)
            {
                _logger.LogWarning("Scheduled expense not found {ScheduledExpenseId} {UserId}", scheduledExpenseId, userId);
                return Result.Failure(new Error(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND));
            }

            _scheduledExpensesRepository.Remove(scheduledExpense);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scheduled expense deleted {ScheduledExpenseId} {UserId}", scheduledExpenseId, userId);
            return Result.Success();
        }
    }
}
