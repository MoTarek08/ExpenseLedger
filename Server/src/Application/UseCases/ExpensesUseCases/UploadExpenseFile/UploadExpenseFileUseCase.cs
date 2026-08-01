using Domain.Entities.FileObjectNamespace;
using Application.ApplicationConstantsNamesapce;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Application.UseCases.ExpensesUseCases.UploadExpenseFile.Models;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.UploadExpenseFile
{
    public class UploadExpenseFileUseCase
    {
        private readonly IObjectStorageService _objectStorageService;
        private readonly IDateProvider _dateTimeProvider;
        private readonly IExpensesFileObjectsRepository _fileObjectsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UploadExpenseFileUseCase> _logger;

        public UploadExpenseFileUseCase(
            IUnitOfWork unitOfWork,
            IExpensesFileObjectsRepository fileObjectsRepository,
            IObjectStorageService objectStorageService,
            IDateProvider dateTimeProvider,
            ILogger<UploadExpenseFileUseCase> logger
            )
        {
            _objectStorageService = objectStorageService;
            _dateTimeProvider = dateTimeProvider;
            _fileObjectsRepository = fileObjectsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<UploadExpenseFileResponseModel>> Execute(
            Guid userId,
            UploadExpenseFileRequestModel requestModel,
            CancellationToken cancellationToken)

        {
            var now = _dateTimeProvider.Now;
            var objectKey = new ObjectKey(userId, DateOnly.FromDateTime(now.UtcDateTime), requestModel.ContentType, FileObjectConstants.ImagesFolderName);

            var fileObject = ExpenseFileObject.CreatePendingUpload(
                userId,
                objectKey.Value,
                _objectStorageService.Provider,
                requestModel.ContentType,
                requestModel.FileSizeInBytes,
                now,
                now.AddMinutes(_objectStorageService.GetUploadUrlLifeTime()),
                requestModel.OriginalFileName);


            _fileObjectsRepository.Add(fileObject);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var url = await _objectStorageService.GenerateUploadUrlAsync(objectKey, fileObject.StartedProcessingAt, fileObject.UploadUrlExpiresAt);

            _logger.LogInformation("Upload URL generated for file {FileObjectId} for user {UserId}", fileObject.Id, userId);

            return Result<UploadExpenseFileResponseModel>.Success(new UploadExpenseFileResponseModel(url, fileObject.Id));
        }
    }
}
