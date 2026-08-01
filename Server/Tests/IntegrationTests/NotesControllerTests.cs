using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.UseCases.NotesUseCases.CreateNote.ModelsNamespace;
using Application.UseCases.NotesUseCases.Models;
using Application.UseCases.NotesUseCases.UpdateNote.Models;
using Host.Models;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Domain.Entities.NoteNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;

namespace IntegrationTests;

public class NotesControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public NotesControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async Task InitializeAsync() => await _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetById_Success_ShouldReturnNote()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var expenseId = await ExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var noteId = await CreateNoteInDb(expenseId, "test content");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/notes/{noteId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<NoteDto>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Equal(noteId, body.Id);
        Assert.Equal(expenseId, body.ExpenseId);
        Assert.Equal("test content", body.Content);
    }

    [Fact]
    public async Task GetById_NotOwned_ShouldReturn404()
    {
        var authA = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var expenseId = await ExpenseBuilder.Create(_fixture, authA.UserId).BuildAsync();
        var noteId = await CreateNoteInDb(expenseId, "secret note");

        var authB = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/notes/{noteId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Success_ShouldReturn201AndCreateNote()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var expenseId = await ExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var requestModel = new CreateNoteRequestModel(expenseId, "new note");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notes")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResourceId<Guid>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var note = await db.Notes.FindAsync(body.Id);
            Assert.NotNull(note);
            Assert.Equal(expenseId, note.ExpenseId);
            Assert.Equal("new note", note.Content);
        });
    }

    [Fact]
    public async Task Create_ExpenseNotFound_ShouldReturn404()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var requestModel = new CreateNoteRequestModel(Guid.NewGuid(), "content");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notes")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidInput_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var requestModel = new CreateNoteRequestModel(Guid.NewGuid(), "");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notes")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_Success_ShouldUpdateContent()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var expenseId = await ExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var noteId = await CreateNoteInDb(expenseId, "original");

        var updateModel = new UpdateNoteRequestModel("updated content");
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/notes/{noteId}")
        {
            Content = JsonHelper.Serialize(updateModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var note = await db.Notes.FindAsync(noteId);
            Assert.NotNull(note);
            Assert.Equal("updated content", note.Content);
        });
    }

    [Fact]
    public async Task Delete_Success_ShouldRemoveNote()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var expenseId = await ExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var noteId = await CreateNoteInDb(expenseId, "to delete");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/notes/{noteId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var note = await db.Notes.FindAsync(noteId);
            Assert.Null(note);
        });
    }

    [Fact]
    public async Task Delete_NotOwned_ShouldReturn404()
    {
        var authA = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var expenseId = await ExpenseBuilder.Create(_fixture, authA.UserId).BuildAsync();
        var noteId = await CreateNoteInDb(expenseId, "not yours");

        var authB = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, authB.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/notes/{noteId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var note = await db.Notes.FindAsync(noteId);
            Assert.NotNull(note);
        });
    }



    private async Task<Guid> CreateNoteInDb(Guid expenseId, string content)
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var notesRepo = sp.GetRequiredService<INotesRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var note = Note.Create(expenseId, content, DateTimeOffset.UtcNow);
        notesRepo.Add(note);
        await unitOfWork.SaveChangesAsync();

        return note.Id;
    }
}
