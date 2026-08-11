using FakeItEasy;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Minio.DataModel.Args;
using Npgsql;
using Respawn;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace IntegrationTests.CustomWebApplicationFactoryNamespace
{
    public sealed class IntegrationTestFixture : IAsyncDisposable
    {
        public PostgreSqlContainer Db { get; }
        public PostgreSqlContainer HangfireDb { get; }
        public MinioContainer Minio { get; }
        public CustomWebApplicationFactory Factory { get; }

        private Respawner? _mainRespawner;
        private Respawner? _hangfireRespawner;

        public IntegrationTestFixture()
        {
            Db = new PostgreSqlBuilder("postgres:16-alpine").Build();
            HangfireDb = new PostgreSqlBuilder("postgres:16-alpine").Build();
            Minio = new MinioBuilder("minio/minio:RELEASE.2025-04-22T22-12-26Z").Build();

            Task.WhenAll(
                Db.StartAsync(),
                HangfireDb.StartAsync(),
                Minio.StartAsync()).GetAwaiter().GetResult();

            Factory = new CustomWebApplicationFactory(this);

            using var scope = Factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.MigrateAsync().GetAwaiter().GetResult();
        }

        public async Task ResetAsync()
        {
            await using var mainConnection = new NpgsqlConnection(Db.GetConnectionString());
            await mainConnection.OpenAsync();
            if (_mainRespawner is null)
            {
                _mainRespawner = await Respawner.CreateAsync(mainConnection, new RespawnerOptions
                {
                    TablesToIgnore = new Respawn.Graph.Table[]
                    {
                        new("__EFMigrationsHistory"),
                        new("expense_categories"),
                        new("expense_sub_categories")
                    }
                });
            }

            await _mainRespawner.ResetAsync(mainConnection);

            await using var hangfireConnection = new NpgsqlConnection(HangfireDb.GetConnectionString());
            await hangfireConnection.OpenAsync();
            var shouldResetHangfire = true;

            if (_hangfireRespawner is null)
            {
                try
                {
                    _hangfireRespawner = await Respawner.CreateAsync(hangfireConnection);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("No tables found"))
                {
                    shouldResetHangfire = false;
                }
            }

            if(shouldResetHangfire)
                await _hangfireRespawner!.ResetAsync(hangfireConnection);

            Fake.ClearRecordedCalls(Factory.FakeObjectStorageClient);

            var endpoint = $"{Minio.Hostname}:{Minio.GetMappedPublicPort(9000)}";
            var minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(Minio.GetAccessKey(), Minio.GetSecretKey())
                .WithSSL(false)
                .Build();

            var bucketName = "test-bucket";

            var bucketExists = await minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName));
            if (bucketExists)
            {
                await foreach (var obj in minioClient.ListObjectsEnumAsync(
                    new ListObjectsArgs().WithBucket(bucketName).WithRecursive(true)))
                    await minioClient.RemoveObjectAsync(
                        new RemoveObjectArgs().WithBucket(bucketName).WithObject(obj.Key));
                await minioClient.RemoveBucketAsync(
                    new RemoveBucketArgs().WithBucket(bucketName));
            }

            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucketName));
        }

        public async ValueTask DisposeAsync()
        {
            Factory.Dispose();

            await Db.DisposeAsync();
            await HangfireDb.DisposeAsync();
            await Minio.DisposeAsync();
        }
    }
}
