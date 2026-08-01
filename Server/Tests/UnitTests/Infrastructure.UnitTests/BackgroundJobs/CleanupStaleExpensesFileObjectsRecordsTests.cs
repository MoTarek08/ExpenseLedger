using Domain.Entities.DomainEnums;
using Domain.Entities.FileObjectNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using FakeItEasy;
using Infrastructure.BackgroundJobs.BackgroundJobs;
using Microsoft.Extensions.Logging;

namespace UnitTests.Infrastructure.UnitTests.BackgroundJobs
{
    public class CleanupStaleExpensesFileObjectsRecordsTests
    {
        private readonly IExpensesFileObjectsRepository _repository;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CleanupStaleExpensesFileObjectsRecords> _logger;
        private readonly CleanupStaleExpensesFileObjectsRecords _sut;

        private readonly DateTimeOffset _now = new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);

        public CleanupStaleExpensesFileObjectsRecordsTests()
        {
            _repository = A.Fake<IExpensesFileObjectsRepository>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<CleanupStaleExpensesFileObjectsRecords>>();

            A.CallTo(() => _dateProvider.Now).Returns(_now);

            _sut = new CleanupStaleExpensesFileObjectsRecords(
                _repository, _dateProvider, _logger);
        }

        [Fact]
        public async Task Execute_NoStaleUploads_DoesNothing()
        {
            A.CallTo(() => _repository.FindsStaleUploadsAsync(A<Guid>._, A<DateTimeOffset>._, A<int>._))
                .Returns([]);

            await _sut.Execute();

            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_SingleBatch_DeletesAll()
        {
            var items = CreateItems(3);
            var ids = items.Select(i => i.Id).ToList();

            A.CallTo(() => _repository.FindsStaleUploadsAsync(A<Guid>._, A<DateTimeOffset>._, A<int>._))
                .ReturnsNextFromSequence(items, []);
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(ids)))
                .Returns(3);

            await _sut.Execute();

            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(ids)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_MultipleBatches_AccumulatesTotal()
        {
            var firstBatch = CreateItems(2);
            var secondBatch = CreateItems(3);
            var firstIds = firstBatch.Select(i => i.Id).ToList();
            var secondIds = secondBatch.Select(i => i.Id).ToList();

            A.CallTo(() => _repository.FindsStaleUploadsAsync(A<Guid>._, A<DateTimeOffset>._, A<int>._))
                .ReturnsNextFromSequence(firstBatch, secondBatch, []);
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(firstIds)))
                .Returns(2);
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(secondIds)))
                .Returns(3);

            await _sut.Execute();

            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(firstIds)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(secondIds)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_LastSeenIdAdvances_AfterEachBatch()
        {
            var firstBatch = CreateItems(3);
            var lastIdOfFirstBatch = firstBatch.Last().Id;
            var secondBatch = CreateItems(2);

            var capturedLastSeenIds = new List<Guid>();
            A.CallTo(() => _repository.FindsStaleUploadsAsync(A<Guid>._, A<DateTimeOffset>._, A<int>._))
                .Invokes(call => capturedLastSeenIds.Add(call.GetArgument<Guid>(0)))
                .ReturnsNextFromSequence(firstBatch, secondBatch, []);
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>._))
                .Returns(3).Once()
                .Then.Returns(2).Once();

            await _sut.Execute();

            Assert.Equal(Guid.Empty, capturedLastSeenIds[0]);
            Assert.Equal(lastIdOfFirstBatch, capturedLastSeenIds[1]);
        }

        [Fact]
        public async Task Execute_FullBatchBoundary_ProcessesAllItems()
        {
            var firstBatch = CreateItems(50);
            var secondBatch = CreateItems(3);
            var firstIds = firstBatch.Select(i => i.Id).ToList();
            var secondIds = secondBatch.Select(i => i.Id).ToList();

            A.CallTo(() => _repository.FindsStaleUploadsAsync(A<Guid>._, A<DateTimeOffset>._, A<int>._))
                .ReturnsNextFromSequence(firstBatch, secondBatch, []);
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(firstIds)))
                .Returns(50);
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(secondIds)))
                .Returns(3);

            await _sut.Execute();

            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(firstIds)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _repository.BulkDeleteAsync(A<List<Guid>>.That.IsSameSequenceAs(secondIds)))
                .MustHaveHappenedOnceExactly();
        }

        private static List<ExpenseFileObject> CreateItems(int count)
        {
            var results = new List<ExpenseFileObject>(count);
            for (var i = 0; i < count; i++)
            {
                results.Add(ExpenseFileObject.CreatePendingUpload(
                    Guid.NewGuid(),
                    $"test-object-key-{i}",
                    StorageProvider.MinIO,
                    "image/jpeg",
                    1024 * 10,
                    new DateTimeOffset(2026, 7, 25, 10, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero)));
            }
            return results;
        }
    }
}
