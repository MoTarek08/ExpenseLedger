using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.GetExpenseById
{
    public class GetExpenseByIdUseCase
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly ILogger<GetExpenseByIdUseCase> _logger;

        public GetExpenseByIdUseCase(
            IExpensesRepository expensesRepository,
            ILogger<GetExpenseByIdUseCase> logger)
        {
            _expensesRepository = expensesRepository;
            _logger = logger;
        }

        public async Task<Result<ExpenseDto>> Execute(Guid userId, Guid expenseId, CancellationToken cancellationToken)
        {
            var dto = await _expensesRepository.FindExpenseDtoByIdAsync(expenseId, userId, cancellationToken);
            if (dto is null)
            {
                _logger.LogWarning("Expense {ExpenseId} not found for user {UserId}", expenseId, userId);
                return Result<ExpenseDto>.Failure(new Error(ExpensesErrorCodes.EXPENSE_NOT_FOUND));
            }

            return Result<ExpenseDto>.Success(dto);
        }
    }
}