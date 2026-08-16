using Application.Interfaces.BackgroundJobs;
using FakeItEasy;
using Infrastructure.ObjectStorage.Clients;
using IntegrationTests.BackgroundJobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


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
            // UseSetting flows those configuration values early enough so that the builder can handle the early Program.cs reads successfully;
            builder.UseSetting("DbSettings:ConnectionString", _fixture.Db.GetConnectionString());
            builder.UseSetting("HangfireDbSettings:ConnectionString", _fixture.HangfireDb.GetConnectionString());
            builder.UseSetting("ObjectStorageSettings:Endpoint", $"{_fixture.Minio.Hostname}:{_fixture.Minio.GetMappedPublicPort(9000)}");
            builder.UseSetting("ObjectStorageSettings:AccessKey", _fixture.Minio.GetAccessKey());
            builder.UseSetting("ObjectStorageSettings:SecretKey", _fixture.Minio.GetSecretKey());
            builder.UseSetting("ObjectStorageSettings:BucketName", "test-bucket");
            builder.UseSetting("AccessTokenSettings:SigningKey", "wwx7lyNbTyzTNI4ud50IL7V3fhBtnOCdMZuhsDXHREp");

            builder.ConfigureTestServices
            (services =>
            {
                // This was already added when configuring the background job client 
                services.RemoveAll<IBackgroundJobsService>();
                services.AddSingleton<IBackgroundJobsService, FakeBackgroundJobsService>();


                services.RemoveAll<IObjectStorageClient>();
                services.AddScoped<MinioApplicationClient>();

                // FakeItEasy stubs should be singelton
                services.AddSingleton<IObjectStorageClient>(sp =>
                {
                    using var scope = sp.CreateScope();
                    var concreteClient = scope.ServiceProvider.GetRequiredService<MinioApplicationClient>();
                    FakeObjectStorageClient = A.Fake<IObjectStorageClient>(o =>
                    {
                        o.Wrapping(concreteClient);
                    });
                    return FakeObjectStorageClient;
                });

            }
             );
            builder.UseEnvironment("Development");
        }

        public IServiceScope CreateScope()
        {
            return Services.CreateScope();
        }
    }
}