// COMMENTED OUT: Object storage deletion requests are no longer used.
// Deletion of file objects is now performed immediately when the owning entity is deleted
// (see DeleteExpenseUseCase). This cron job existed to consume persisted deletion requests
// asynchronously. Keep this code for potential future use.
/*
using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs.BackgroundJobs
{
    public class ObjectStorageDeletionCleanupJob
    {
        private readonly IObjectStorageDeletionRequestsRepository _repository;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<ObjectStorageDeletionCleanupJob> _logger;

        public ObjectStorageDeletionCleanupJob(
            IObjectStorageDeletionRequestsRepository repository,
            IObjectStorageService objectStorageService,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider,
            ILogger<ObjectStorageDeletionCleanupJob> logger)
        {
            _repository = repository;
            _objectStorageService = objectStorageService;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task Execute()
        {
            var lastSeenId = Guid.Empty;

            while (true)
            {
                var batch = await _repository.FindPendingAsync(lastSeenId);

                if (batch.Count == 0)
                    break;

                foreach (var request in batch)
                {
                    try
                    {
                        await _objectStorageService.DeleteAsync(request.ObjectKey);
                        request.MarkAsProcessed(_dateTimeProvider.Now);
                    }
                    catch (FileObjectAlreadyDeleted)
                    {
                        request.MarkAsProcessed(_dateTimeProvider.Now);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete object from storage {ObjectKey}", request.ObjectKey);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                lastSeenId = batch.Last().Id;
            }
        }
    }
}
*/