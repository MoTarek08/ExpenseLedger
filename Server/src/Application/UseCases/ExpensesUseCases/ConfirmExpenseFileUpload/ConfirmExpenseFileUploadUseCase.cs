using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Domain.Entities.DomainEnums;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.ConfirmImageUpload
{
    public class ConfirmExpenseFileUploadUseCase
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensesFileObjectsRepository _expensesFileObjectsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private IObjectStorageService _objectStorageService;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<ConfirmExpenseFileUploadUseCase> _logger;

        public ConfirmExpenseFileUploadUseCase(
            IExpensesRepository expensesRepository,
            IExpensesFileObjectsRepository expensesFileObjectsRepository,
            IUnitOfWork unitOfWork,
            IObjectStorageService objectStorageService,
            IBackgroundJobsService backgroundJobsService,
            IDateProvider dateTimeProvider,
            ILogger<ConfirmExpenseFileUploadUseCase> logger)
        {
            _expensesRepository = expensesRepository;
            _expensesFileObjectsRepository = expensesFileObjectsRepository;
            _unitOfWork = unitOfWork;
            _objectStorageService = objectStorageService;
            _backgroundJobsService = backgroundJobsService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid uploadedFileId, Guid expenseId, Guid userId, CancellationToken cancellationToken)
        {

            var expense = await _expensesRepository.FindAsync(expenseId, cancellationToken);
            if (expense is null || expense.UserId != userId)
            {
                _logger.LogWarning("Confirm failed: expense {ExpenseId} not found for user {UserId}", expenseId, userId);
                return Result.Failure(new Error(ExpensesErrorCodes.EXPENSE_NOT_FOUND));
            }

            var expenseFile = await _expensesFileObjectsRepository.FindAsync(uploadedFileId, cancellationToken);
            if (expenseFile is null || expenseFile.UserId != userId)
            {
                _logger.LogWarning("Confirm failed: file {FileId} not found for user {UserId}", uploadedFileId, userId);
                return Result.Failure(new Error(ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND));
            }

            if (expenseFile.ExpenseId is not null)
            {
                _logger.LogWarning("Confirm failed: file {FileId} already linked to expense {ExpenseId}", uploadedFileId, expenseFile.ExpenseId);
                return Result.Failure(new Error(ExpensesErrorCodes.EXPENSE_FILE_ALREADY_LINKED_TO_OTHER_EXPENSE));
            }

            if (expenseFile.Status is not FileObjectStatus.PendingUpload)
            {
                _logger.LogWarning("Confirm failed: file {FileId} has invalid status {Status}", uploadedFileId, expenseFile.Status);
                return Result.Failure(new Error(ExpensesErrorCodes.EXPENSE_INVALID_FILE_STATE));
            }


            var objectFileInfo = await _objectStorageService.GetFileInfoAsync(expenseFile.ObjectKey, cancellationToken);
            if (!objectFileInfo.Exists)
            {
                _logger.LogWarning("Confirm failed: file {FileId} not uploaded to storage", uploadedFileId);
                return Result.Failure(new Error(ExpensesErrorCodes.EXPENSE_FILE_NOT_UPLOADED_YET));
            }

            var now = _dateTimeProvider.Now;

            expenseFile.MarkAsUploaded(now);
            expenseFile.LinkToExpense(expenseId);
            expenseFile.ChangeFileSize(objectFileInfo.SizeInBytes!.Value);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("File {FileId} confirmed and linked to expense {ExpenseId}", uploadedFileId, expenseId);

            return Result.Success();

        }
    }
}
