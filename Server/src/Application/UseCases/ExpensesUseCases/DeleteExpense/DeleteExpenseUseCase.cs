using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.DeleteExpense
{
    public class DeleteExpenseUseCase
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteExpenseUseCase> _logger;

        public DeleteExpenseUseCase(
            IExpensesRepository expensesRepository,
            IObjectStorageService objectStorageService,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider,
            ILogger<DeleteExpenseUseCase> logger)
        {
            _expensesRepository = expensesRepository;
            _objectStorageService = objectStorageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, Guid expenseId, CancellationToken cancellationToken)
        {
            var expense = await _expensesRepository.FindIncludingFileObjectAsync(expenseId, cancellationToken);
            if (expense is null)
            {
                _logger.LogInformation("Delete: expense {ExpenseId} already deleted or not found", expenseId);
                return Result.Success();
            }

            if (expense.UserId != userId)
            {
                _logger.LogWarning("Delete failed: expense {ExpenseId} not owned by user {UserId}", expenseId, userId);
                return Result.Failure(new Error(ExpensesErrorCodes.EXPENSE_NOT_FOUND));
            }

            if (expense.FileObject is not null)
            {
               try 
                {
                    await _objectStorageService.DeleteAsync(expense.FileObject.ObjectKey, cancellationToken);
                }
               catch(FileObjectAlreadyDeleted)
                {
                    _logger.LogInformation("File object {ExpenseFileObjectId} for expense {ExpenseId} already deleted ",
                   expense.FileObject.Id,
                   expense.Id);
                }
            }

            _expensesRepository.Remove(expense);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Expense deleted {ExpenseId} for user {UserId}", expenseId, userId);

            return Result.Success();
        }
    }
}
