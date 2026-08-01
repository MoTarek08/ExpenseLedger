using Application.ApplicationConstantsNamesapce;
using Domain.Entities.DomainEnums;
using Domain.Entities.FileObjectNamespace;
using Infrastructure.BackgroundJobs.BackgroundJobs;
using Infrastructure.Database.AppDbContextNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.BackgroundJobs
{
    public class CleanupStaleExpensesFileObjectsRecordsTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public CleanupStaleExpensesFileObjectsRecordsTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Factory.CreateClient();
        }

        public async Task InitializeAsync() => await _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task Execute_WhenStaleFileObject_ShouldHardDelete()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture).BuildAsync();
            var files = CreateStale(5, auth.UserId);

            await AddFilesToDb(files);

            using var scope = _fixture.Factory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<CleanupStaleExpensesFileObjectsRecords>();
            await job.Execute();

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                Assert.False(await db.ExpensesFileObjects.AnyAsync());
            });
        }

        [Fact]
        public async Task Execute_WhenNoStaleFileObjects_ShouldNotDelete()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture).BuildAsync();
            var files = CreateNotStale(5, auth.UserId);
            await AddFilesToDb(files);


            using var scope = _fixture.Factory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<CleanupStaleExpensesFileObjectsRecords>();
            await job.Execute();

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                Assert.Equal(5, await db.ExpensesFileObjects.CountAsync());
            });
        }





        private List<ExpenseFileObject> CreateStale(int count, Guid userId)
        {
            var now = DateTimeOffset.UtcNow;

            List<ExpenseFileObject> fileObjects = new(count);
            for(int i=0; i<count; i += 1)
            {
                fileObjects.Add(ExpenseFileObject.CreatePendingUpload(
                userId,
                $"stale-file-key-{i}",
                StorageProvider.MinIO,
                FileObjectConstants.jpg,
                1024 * 1024 * 500,
                now.AddMinutes(-136),
                now.AddMinutes(-121)
                ));
            }

            return fileObjects;
        }


        private List<ExpenseFileObject> CreateNotStale(int count, Guid userId)
        {
            var now = DateTimeOffset.UtcNow;

            List<ExpenseFileObject> fileObjects = new(count);
            for (int i = 0; i < count; i += 1)
            {
                fileObjects.Add(ExpenseFileObject.CreatePendingUpload(
                userId,
                $"not-stale-file-key-{i}",
                StorageProvider.MinIO,
                FileObjectConstants.jpg,
                1024 * 1024 * 500,
                now.AddMinutes(-5),
                now.AddMinutes(10)
                ));
            }

            return fileObjects;
        }


        private async Task AddFilesToDb(List<ExpenseFileObject> fileObjects)
        {
            using var scope = _fixture.Factory.CreateScope();
            var sp = scope.ServiceProvider;
            var db = sp.GetRequiredService<AppDbContext>();

            db.ExpensesFileObjects.AddRange(fileObjects);
            await db.SaveChangesAsync();
        }

    }
}
