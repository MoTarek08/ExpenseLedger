using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Domain.Entities.DomainEnums;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesFileObjectsUseCases.DeleteExpenseFile
{
    public class DeleteExpenseFileObjectUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteExpenseFileObjectUseCase> _logger;
        private readonly IExpensesFileObjectsRepository _expensesFileObjectsRepository;
        private readonly IObjectStorageService _objectStorageService;

        public DeleteExpenseFileObjectUseCase(
            IExpensesFileObjectsRepository expensesFileObjectsRepository,
            IObjectStorageService objectStorageService,
            IUnitOfWork unitOfWork,
            ILogger<DeleteExpenseFileObjectUseCase> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _expensesFileObjectsRepository = expensesFileObjectsRepository;
            _objectStorageService = objectStorageService;
        }

        public async Task<Result> Execute(Guid userId, Guid fileId, CancellationToken cancellationToken)
        {
            var file = await _expensesFileObjectsRepository.FindAsync(fileId,cancellationToken);
            if(file is null)
            {
                _logger.LogInformation("Delete expense file : Expense file not found {ExpenseFileObjectId} but returned 204 idempotently", fileId);
                return Result.Success();
            }
            if(file.UserId != userId)
            {
                _logger.LogWarning("Delete expense file: Wrong ownership for file {ExpenseFileObjectId} by user {UserId}", fileId, userId);
                return Result.Failure(new Error(ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND));
            }

            if (file.Status is ExpenseFileObjectStatus.Failed)
                return Result.Success();
            try
            {
                await _objectStorageService.DeleteAsync(file.ObjectKey, cancellationToken);
                _logger.LogInformation("Delete expense file: Expense file deleted successfully {ExpenseFileObjectId}", fileId);
            }
            catch (FileObjectAlreadyDeleted)
            {
                _logger.LogInformation("Delete expense file: Expense file already deleted {ExpenseFileObjectId}", fileId);
            }

            _expensesFileObjectsRepository.Remove(file);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Delete expense file: Expense file object deleted from db successfully {ExpenseFileObjectId}",fileId);
            return Result.Success();
        }
    }
}
