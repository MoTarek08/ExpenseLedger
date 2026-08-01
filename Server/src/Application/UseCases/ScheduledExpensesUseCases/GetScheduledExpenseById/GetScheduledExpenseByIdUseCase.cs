using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.Models.Result;
using Application.UseCases.ScheduledExpensesUseCases.Models;
using Domain.Entities.ScheduledExpenseNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ScheduledExpensesUseCases.GetScheduledExpenseById
{
    public class GetScheduledExpenseByIdUseCase
    {
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly ILogger<GetScheduledExpenseByIdUseCase> _logger;

        public GetScheduledExpenseByIdUseCase(
            IScheduledExpensesRepository scheduledExpensesRepository,
            ILogger<GetScheduledExpenseByIdUseCase> logger)
        {
            _scheduledExpensesRepository = scheduledExpensesRepository;
            _logger = logger;
        }

        public async Task<Result<ScheduledExpenseDto>> Execute(Guid userId, Guid scheduledExpenseId, CancellationToken cancellationToken)
        {
            var scheduledExpense = await _scheduledExpensesRepository.FindIncludingCategoriesAsync(scheduledExpenseId, cancellationToken);
            if (scheduledExpense is null || scheduledExpense.UserId != userId)
            {
                _logger.LogWarning("Scheduled expense not found {ScheduledExpenseId} {UserId}", scheduledExpenseId, userId);
                return Result<ScheduledExpenseDto>.Failure(new Error(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND));
            }

            _logger.LogInformation("Scheduled expense retrieved {ScheduledExpenseId} {UserId}", scheduledExpenseId, userId);
            return Result<ScheduledExpenseDto>.Success(new ScheduledExpenseDto(
                scheduledExpense.Id,
                scheduledExpense.IsActive,
                scheduledExpense.Title,
                scheduledExpense.Amount,
                scheduledExpense.Cadence,
                scheduledExpense.Category.Code,
                scheduledExpense.SubCategory?.Code,
                scheduledExpense.FirstDueOn,
                scheduledExpense.NextDueOn,
                scheduledExpense.LastProcessedAt,
                scheduledExpense.CreatedAt));
        }
    }
}
