using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.SharedServices;
using Application.UseCases.BudgetUseCases.GetRemainingBudget;
using Application.UseCases.BudgetUseCases.Helpers;
using Domain.Entities.Notification;

namespace Infrastructure.SharedServices
{
    public class CheckBudgetStateService : ICheckBudgetStateService
    {
        private readonly GetRemainingBudgetUseCase _getBudgetUseCase;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IDateProvider _dateProvider;

        public CheckBudgetStateService(
            GetRemainingBudgetUseCase getBudgetUseCase,
            IExpensesRepository expensesRepository,
            IDateProvider dateProvider)
        {
            _getBudgetUseCase = getBudgetUseCase;
            _expensesRepository = expensesRepository;
            _dateProvider = dateProvider;
        }

        public async Task<Notification?> EvaluateAsync(Guid expenseId, CancellationToken cancellationToken = default)
        {
            var expenseBudgetDetails = await _expensesRepository.GetCheckBudgetAfterExpenseCreationModelAsync(expenseId, cancellationToken);
            if (expenseBudgetDetails is null)
                return null;

            var now = _dateProvider.Now;
            var today = DateOnly.FromDateTime(now.UtcDateTime);
            var userLastPayDay = BudgetComputingHelpers.GetLastPayDay(expenseBudgetDetails.ResetDay, today);

            if (expenseBudgetDetails.ExpenseSpentOn < userLastPayDay)
                return null;

            var result = await _getBudgetUseCase.Execute(expenseBudgetDetails.UserId, cancellationToken);
            var remaining = result.Data;

            if (remaining < 0)
            {
                return Notification.BudgetWentNegative(
                    expenseBudgetDetails.UserId,
                    expenseId,
                    remaining,
                    BudgetComputingHelpers.GetLastPayDay(expenseBudgetDetails.ResetDay, today),
                    now);
            }

            if (remaining < 0.1m * expenseBudgetDetails.MonthlyNetIncome)
            {
                return Notification.BudgentWentBelowTenPercent(
                    expenseBudgetDetails.UserId,
                    expenseId,
                    BudgetComputingHelpers.GetLastPayDay(expenseBudgetDetails.ResetDay, today),
                    now);
            }

            if (remaining < 0.25m * expenseBudgetDetails.MonthlyNetIncome)
            {
                return Notification.BudgentWentBelowQuarter(
                    expenseBudgetDetails.UserId,
                    expenseId,
                    BudgetComputingHelpers.GetLastPayDay(expenseBudgetDetails.ResetDay, today),
                    now);
            }

            return null;
        }
    }
}
