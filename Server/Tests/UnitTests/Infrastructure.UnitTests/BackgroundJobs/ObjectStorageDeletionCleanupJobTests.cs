// COMMENTED OUT: these tests cover ObjectStorageDeletionCleanupJob which is no longer used.
// Deletion of file objects is now performed immediately when the owning entity is deleted.
// Keep this code for potential future use (uncomment together with the job itself).
/*
using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.DomainEnums;
using Domain.Entities.ObjectStorageDeletionRequestNamespace;
using FakeItEasy;
using Infrastructure.BackgroundJobs.BackgroundJobs;
using Microsoft.Extensions.Logging;

namespace UnitTests.Infrastructure.UnitTests.BackgroundJobs
{
    public class ObjectStorageDeletionCleanupJobTests
    {
        private readonly IObjectStorageDeletionRequestsRepository _repository;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<ObjectStorageDeletionCleanupJob> _logger;
        private readonly ObjectStorageDeletionCleanupJob _sut;

        private readonly DateTimeOffset _now = new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);

        public ObjectStorageDeletionCleanupJobTests()
        {
            _repository = A.Fake<IObjectStorageDeletionRequestsRepository>();
            _objectStorageService = A.Fake<IObjectStorageService>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<ObjectStorageDeletionCleanupJob>>();

            A.CallTo(() => _dateTimeProvider.Now).Returns(_now);

            _sut = new ObjectStorageDeletionCleanupJob(
                _repository, _objectStorageService, _unitOfWork, _dateTimeProvider, _logger);
        }

        [Fact]
        public async Task Execute_NoPendingRequests_CompletesWithoutCalls()
        {
            A.CallTo(() => _repository.FindPendingAsync(A<Guid>._, A<int>._))
                .Returns([]);

            await _sut.Execute();

            A.CallTo(() => _objectStorageService.DeleteAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_SingleRequestDeleteSucceeds_MarksAsProcessed()
        {
            var request = ObjectStorageDeletionRequest.Create("obj-key", StorageProvider.MinIO, _now.AddHours(-1));

            A.CallTo(() => _repository.FindPendingAsync(A<Guid>._, A<int>._))
                .ReturnsNextFromSequence([request], []);

            await _sut.Execute();

            Assert.NotNull(request.ProcessedAt);
            Assert.Equal(_now, request.ProcessedAt!.Value);
            A.CallTo(() => _objectStorageService.DeleteAsync(request.ObjectKey, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_MultipleRequestsAllSucceed_AllMarkedProcessed()
        {
            var requests = new[]
            {
                ObjectStorageDeletionRequest.Create("key-1", StorageProvider.MinIO, _now.AddHours(-2)),
                ObjectStorageDeletionRequest.Create("key-2", StorageProvider.MinIO, _now.AddHours(-2)),
                ObjectStorageDeletionRequest.Create("key-3", StorageProvider.MinIO, _now.AddHours(-2)),
            };

            A.CallTo(() => _repository.FindPendingAsync(A<Guid>._, A<int>._))
                .ReturnsNextFromSequence(requests.ToList(), []);

            await _sut.Execute();

            foreach (var request in requests)
            {
                Assert.NotNull(request.ProcessedAt);
                Assert.Equal(_now, request.ProcessedAt!.Value);
            }
            A.CallTo(() => _objectStorageService.DeleteAsync(A<string>._, A<CancellationToken>._))
                .MustHaveHappened(3, Times.Exactly);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_DeleteThrowsFileObjectAlreadyDeleted_MarksAsProcessed()
        {
            var request = ObjectStorageDeletionRequest.Create("obj-key", StorageProvider.MinIO, _now.AddHours(-1));

            A.CallTo(() => _repository.FindPendingAsync(A<Guid>._, A<int>._))
                .ReturnsNextFromSequence([request], []);
            A.CallTo(() => _objectStorageService.DeleteAsync(request.ObjectKey, A<CancellationToken>._))
                .Throws<FileObjectAlreadyDeleted>();

            await _sut.Execute();

            Assert.NotNull(request.ProcessedAt);
            Assert.Equal(_now, request.ProcessedAt!.Value);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_MultipleRequestsMixedOutcome_ProcessesCorrectly()
        {
            var successRequest = ObjectStorageDeletionRequest.Create("key-success", StorageProvider.MinIO, _now.AddHours(-2));
            var alreadyDeletedRequest = ObjectStorageDeletionRequest.Create("key-already-deleted", StorageProvider.MinIO, _now.AddHours(-2));
            var failedRequest = ObjectStorageDeletionRequest.Create("key-failed", StorageProvider.MinIO, _now.AddHours(-2));

            A.CallTo(() => _repository.FindPendingAsync(A<Guid>._, A<int>._))
                .ReturnsNextFromSequence(new List<ObjectStorageDeletionRequest> { successRequest, alreadyDeletedRequest, failedRequest }, new List<ObjectStorageDeletionRequest>());

            A.CallTo(() => _objectStorageService.DeleteAsync(successRequest.ObjectKey, A<CancellationToken>._))
                .Returns(Task.CompletedTask);
            A.CallTo(() => _objectStorageService.DeleteAsync(alreadyDeletedRequest.ObjectKey, A<CancellationToken>._))
                .Throws<FileObjectAlreadyDeleted>();
            A.CallTo(() => _objectStorageService.DeleteAsync(failedRequest.ObjectKey, A<CancellationToken>._))
                .Throws(new InvalidOperationException("Unexpected error"));

            await _sut.Execute();

            Assert.NotNull(successRequest.ProcessedAt);
            Assert.NotNull(alreadyDeletedRequest.ProcessedAt);
            Assert.Null(failedRequest.ProcessedAt);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_BatchPagination_ProcessesAllPending()
        {
            var firstBatch = CreateRequests(50);
            var secondBatch = CreateRequests(3);
            var lastIdOfFirstBatch = firstBatch.Last().Id;

            var capturedLastSeenIds = new List<Guid>();
            A.CallTo(() => _repository.FindPendingAsync(A<Guid>._, A<int>._))
                .Invokes(call => capturedLastSeenIds.Add(call.GetArgument<Guid>(0)))
                .ReturnsNextFromSequence(firstBatch, secondBatch, []);

            await _sut.Execute();

            Assert.All(firstBatch.Concat(secondBatch), r => Assert.NotNull(r.ProcessedAt));
            Assert.Equal(Guid.Empty, capturedLastSeenIds[0]);
            Assert.Equal(lastIdOfFirstBatch, capturedLastSeenIds[1]);
            A.CallTo(() => _objectStorageService.DeleteAsync(A<string>._, A<CancellationToken>._))
                .MustHaveHappened(53, Times.Exactly);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappened(2, Times.Exactly);
        }

        private static List<ObjectStorageDeletionRequest> CreateRequests(int count)
        {
            var results = new List<ObjectStorageDeletionRequest>(count);
            for (var i = 0; i < count; i++)
            {
                results.Add(ObjectStorageDeletionRequest.Create(
                    $"test-key-{i}",
                    StorageProvider.MinIO,
                    new DateTimeOffset(2026, 7, 25, 10, 0, 0, TimeSpan.Zero)));
            }
            return results;
        }
    }
}

*/