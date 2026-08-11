using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.ObjectStorage.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ExpensesFileObjectsUseCases.ConfirmExpenseFileUpload.Models;
using Application.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.FileObjectNamespace;
using FakeItEasy;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class ExpensesFileObjectsControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public ExpensesFileObjectsControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Upload_Success_ShouldReturnPresignedUrl()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var requestModel = new UploadExpenseFileRequestModel(
            "image/jpeg", 1024 * 100, "test.jpg");

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/ExpensesFileObjects/upload")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UploadExpenseFileResponseModel>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.NotEmpty(body.UploadUrl);
        Assert.NotEqual(Guid.Empty, body.FileObjectId);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var file = await db.ExpensesFileObjects.FindAsync(body.FileObjectId);
            Assert.NotNull(file);
            Assert.Equal(ExpenseFileObjectStatus.PendingUpload, file.Status);
        });
    }

    [Fact]
    public async Task Upload_InvalidContentType_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var requestModel = new UploadExpenseFileRequestModel(
            "application/pdf", 1024, "test.pdf");

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/ExpensesFileObjects/upload")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Confirm_ExpenseAlreadyHasFile_ShouldReturnConflict()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId);

        var now = DateTimeOffset.UtcNow;
        using (var scope = _fixture.Factory.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var repo = sp.GetRequiredService<IExpensesFileObjectsRepository>();
            var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
            var linkedFile = ExpenseFileObject.CreatePendingUpload(
                auth.UserId,
                $"integration-test/{auth.UserId}/{Guid.NewGuid()}.jpg",
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                now.AddHours(-1),
                now.AddMinutes(15));
            linkedFile.MarkAsUploaded(now);
            linkedFile.LinkToExpense(expenseId);
            repo.Add(linkedFile);
            await unitOfWork.SaveChangesAsync();
        }

        var fileObjectId = await CreatePendingFileObjectInDb(auth.UserId);

        var requestModel = new ConfirmExpenseFileUploadRequestModel(fileObjectId, expenseId);
        var request = new HttpRequestMessage(HttpMethod.Post,
            "/api/v1/ExpensesFileObjects/confirm-upload")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Confirm_FileNotUploaded_ShouldReturnError()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId);

        var fileObjectId = await CreatePendingFileObjectInDb(auth.UserId);

        var requestModel = new ConfirmExpenseFileUploadRequestModel(fileObjectId, expenseId);
        var request = new HttpRequestMessage(HttpMethod.Post,
            "/api/v1/ExpensesFileObjects/confirm-upload")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .GetFileObjectInfoAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .Returns(new FileObjectInfo(false));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Confirm_FileUploaded_ShouldUpdateAndReturnSuccess()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId);

        var fileObjectId = await CreatePendingFileObjectInDb(auth.UserId);

        var requestModel = new ConfirmExpenseFileUploadRequestModel(fileObjectId, expenseId);
        var request = new HttpRequestMessage(HttpMethod.Post,
            "/api/v1/ExpensesFileObjects/confirm-upload")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        long megabyte = 1024 * 1024 * 1024;
        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .GetFileObjectInfoAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .Returns(new FileObjectInfo(true, megabyte));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var file = await db.ExpensesFileObjects.FindAsync(fileObjectId);
            Assert.Equal(file!.ExpenseId, expenseId);
            Assert.Equal(ExpenseFileObjectStatus.Uploaded, file.Status);
            Assert.Equal(megabyte, file.FileSizeInBytes);
        });
    }

    [Fact]
    public async Task Delete_OwnedFile_ShouldDeleteFromStorageAndRemoveFromDb()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var objectKey = $"integration-test/{auth.UserId}/{Guid.NewGuid()}.jpg";
        var fileId = await CreatePendingFileObjectInDb(auth.UserId, objectKey);

        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync("test-bucket", objectKey, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/ExpensesFileObjects/{fileId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync("test-bucket", objectKey, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var file = await db.ExpensesFileObjects.FindAsync(fileId);
            Assert.Null(file);
        });
    }

    [Fact]
    public async Task Delete_FileAlreadyRemovedFromStorage_ShouldStillRemoveFromDb()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var objectKey = $"integration-test/{auth.UserId}/{Guid.NewGuid()}.jpg";
        var fileId = await CreatePendingFileObjectInDb(auth.UserId, objectKey);

        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync("test-bucket", objectKey, A<CancellationToken>._))
            .Throws<FileObjectAlreadyDeleted>();

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/ExpensesFileObjects/{fileId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync("test-bucket", objectKey, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var file = await db.ExpensesFileObjects.FindAsync(fileId);
            Assert.Null(file);
        });
    }

    [Fact]
    public async Task Delete_FileNotOwnedByUser_ShouldReturnNotFoundAndNotTouchStorage()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var otherUser = await AuthenticationScenarioBuilder.Create(_fixture).BuildAsync();

        var objectKey = $"integration-test/{otherUser.UserId}/{Guid.NewGuid()}.jpg";
        var fileId = await CreatePendingFileObjectInDb(otherUser.UserId, objectKey);

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/ExpensesFileObjects/{fileId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var file = await db.ExpensesFileObjects.FindAsync(fileId);
            Assert.NotNull(file);
        });
    }

    [Fact]
    public async Task Delete_FileThatDoesNotExist_ShouldReturnNoContentIdempotently()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/ExpensesFileObjects/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
    



    private Task<Guid> GetAnyCategoryId() => CategoryHelpers.GetAnyCategoryId(_fixture);

    private async Task<Guid> CreateExpenseInDb(
        Guid userId, Guid categoryId, decimal amount = 100m,
        DateOnly? spentOn = null)
    {
        return await ExpenseBuilder.Create(_fixture, userId)
            .WithCategory(categoryId)
            .WithAmount(amount)
            .WithSpentOn(spentOn ?? DateOnly.FromDateTime(DateTime.UtcNow))
            .BuildAsync();
    }

    private async Task<Guid> CreatePendingFileObjectInDb(Guid userId, string? objectKey = null)
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var repo = sp.GetRequiredService<IExpensesFileObjectsRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var now = DateTimeOffset.UtcNow;
        var file = ExpenseFileObject.CreatePendingUpload(
            userId,
            objectKey ?? $"integration-test/{userId}/{Guid.NewGuid()}.jpg",
            StorageProvider.MinIO,
            "image/jpeg",
            1024,
            now,
            now.AddMinutes(15));

        repo.Add(file);
        await unitOfWork.SaveChangesAsync();
        return file.Id;
    }
}