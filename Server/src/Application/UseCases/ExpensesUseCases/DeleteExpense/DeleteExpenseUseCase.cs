using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Domain.Entities.ObjectStorageDeletionRequestNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.DeleteExpense
{
    public class DeleteExpenseUseCase
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IObjectStorageDeletionRequestsRepository _objectStorageDeletionRequestsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<DeleteExpenseUseCase> _logger;

        public DeleteExpenseUseCase(
            IExpensesRepository expensesRepository,
            IObjectStorageDeletionRequestsRepository objectStorageDeletionRequestsRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider,
            ILogger<DeleteExpenseUseCase> logger)
        {
            _expensesRepository = expensesRepository;
            _objectStorageDeletionRequestsRepository = objectStorageDeletionRequestsRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
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
                var now = _dateTimeProvider.Now;
                var deletionRequest = ObjectStorageDeletionRequest.Create(
                    expense.FileObject.ObjectKey,
                    expense.FileObject.StorageProvider,
                    now);

                _objectStorageDeletionRequestsRepository.Add(deletionRequest);
            }

            _expensesRepository.Remove(expense);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Expense deleted {ExpenseId} for user {UserId}", expenseId, userId);

            return Result.Success();
        }
    }
}
