using Application.Interfaces.BackgroundJobs;
using Domain.Entities.DomainEnums;
using FakeItEasy;
using Infrastructure.Database.DatabaseSettings;
using Infrastructure.ObjectStorage;
using Infrastructure.ObjectStorage.Clients;
using Infrastructure.Scheduling;
using IntegrationTests.BackgroundJobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Minio;


namespace IntegrationTests.CustomWebApplicationFactoryNamespace

{
    public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly IntegrationTestFixture _fixture;
        public IObjectStorageClient FakeObjectStorageClient { get; private set; } = null!;

        public CustomWebApplicationFactory(
            IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbSettings>();
                services.AddSingleton(new DbSettings
                {
                    ConnectionString =
                        _fixture.Db.GetConnectionString()
                });

                services.RemoveAll<BackgroundJobsClientDbSettings>();
                services.AddSingleton(
                    new BackgroundJobsClientDbSettings(
                        _fixture.HangfireDb.GetConnectionString()));

                var endpoint = $"{_fixture.Minio.Hostname}:{_fixture.Minio.GetMappedPublicPort(9000)}";

                services.RemoveAll<ObjectStorageSettings>();
                services.AddSingleton(
                    new ObjectStorageSettings(
                        endpoint,
                        StorageProvider.MinIO,
                        _fixture.Minio.GetAccessKey(),
                        _fixture.Minio.GetSecretKey(),
                        "test-bucket",
                        15,
                        "us-east-1",
                        true));

                var minioClient = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(
                        _fixture.Minio.GetAccessKey(),
                        _fixture.Minio.GetSecretKey())
                    .WithSSL(false)
                    .Build();

                services.RemoveAll<IMinioClient>();
                services.AddSingleton(minioClient);

                services.RemoveAll<IBackgroundJobsService>();
                services.AddSingleton<IBackgroundJobsService, FakeBackgroundJobsService>();

                services.RemoveAll<IObjectStorageClient>();

                var sp = services.BuildServiceProvider();

                var realClient = new MinioApplicationClient(
                    sp.GetRequiredService<IMinioClient>(),
                    sp.GetRequiredService<ILogger<MinioApplicationClient>>());

                FakeObjectStorageClient = A.Fake<IObjectStorageClient>(options =>
                {
                    options.Wrapping(realClient);
                });


                services.AddSingleton(FakeObjectStorageClient);
            });
        }

        public IServiceScope CreateScope()
        {
            return Services.CreateScope();
        }
    }
}