using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference.Models;
using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.UserCategoryPreferenceNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class UsersCategoryPreferencesControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public UsersCategoryPreferencesControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async Task InitializeAsync() => await _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_Success_ShouldReturn201()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var categoryId = await GetAnyCategoryId();

        var requestModel = new CreateCategoryPreferenceRequestModel(categoryId, CategoryPreferenceLevel.Important);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/UsersCategoryPreferences")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var preference = await db.UserCategoryPreferences.FindAsync(auth.UserId, categoryId);
            Assert.NotNull(preference);
            Assert.Equal(CategoryPreferenceLevel.Important, preference.PreferenceLevel);
        });
    }

    [Fact]
    public async Task Create_AlreadyExists_ShouldReturn409()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreatePreferenceInDb(auth.UserId, categoryId, CategoryPreferenceLevel.Important);

        var requestModel = new CreateCategoryPreferenceRequestModel(categoryId, CategoryPreferenceLevel.Important);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/UsersCategoryPreferences")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidInput_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var requestModel = new CreateCategoryPreferenceRequestModel(Guid.Empty, CategoryPreferenceLevel.Important);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/UsersCategoryPreferences")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Unauthenticated_ShouldReturn401()
    {
        var categoryId = await GetAnyCategoryId();
        var requestModel = new CreateCategoryPreferenceRequestModel(categoryId, CategoryPreferenceLevel.Important);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/UsersCategoryPreferences")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_Success_ShouldReturn200()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreatePreferenceInDb(auth.UserId, categoryId, CategoryPreferenceLevel.Important);

        var requestModel = new UpdateCategoryPreferenceRequestModel(categoryId, CategoryPreferenceLevel.Essential);
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/UsersCategoryPreferences")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UpdateUserCategoryPrefereneResponseModel>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Equal(auth.UserId, body.UserId);
        Assert.Equal(categoryId, body.CategoryId);
        Assert.Equal(CategoryPreferenceLevel.Important, body.OldPreferenceLevel);
        Assert.Equal(CategoryPreferenceLevel.Essential, body.NewPreferenceLevel);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var preference = await db.UserCategoryPreferences.FindAsync(auth.UserId, categoryId);
            Assert.NotNull(preference);
            Assert.Equal(CategoryPreferenceLevel.Essential, preference.PreferenceLevel);
        });
    }

    [Fact]
    public async Task Update_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreatePreferenceInDb(owner.UserId, categoryId, CategoryPreferenceLevel.Important);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var requestModel = new UpdateCategoryPreferenceRequestModel(categoryId, CategoryPreferenceLevel.Essential);
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/UsersCategoryPreferences")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Success_ShouldReturnPreference()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreatePreferenceInDb(auth.UserId, categoryId, CategoryPreferenceLevel.Neutral);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/UsersCategoryPreferences/{categoryId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserCategoryPreferenceDto>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Equal(CategoryPreferenceLevel.Neutral, body.PreferenceLevel);
    }

    [Fact]
    public async Task GetById_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreatePreferenceInDb(owner.UserId, categoryId, CategoryPreferenceLevel.Neutral);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/UsersCategoryPreferences/{categoryId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_NoFilter_ShouldReturnAllPreferences()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var categories = await GetCategories(2);
        var categoryId1 = categories[0];
        var categoryId2 = categories[1];

        await CreatePreferenceInDb(auth.UserId, categoryId1, CategoryPreferenceLevel.Important);
        await CreatePreferenceInDb(auth.UserId, categoryId2, CategoryPreferenceLevel.Avoid);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/UsersCategoryPreferences/search");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<UserCategoryPreferenceDto>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }

    [Fact]
    public async Task Search_WithFilter_ShouldReturnFiltered()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var categories = await GetCategories(2);
        var categoryId1 = categories[0];
        var categoryId2 = categories[1];

        await CreatePreferenceInDb(auth.UserId, categoryId1, CategoryPreferenceLevel.Important);
        await CreatePreferenceInDb(auth.UserId, categoryId2, CategoryPreferenceLevel.Avoid);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/UsersCategoryPreferences/search?PreferenceLevel=Important");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<UserCategoryPreferenceDto>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal(CategoryPreferenceLevel.Important, body[0].PreferenceLevel);
    }

    [Fact]
    public async Task Search_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/UsersCategoryPreferences/search");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Success_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreatePreferenceInDb(auth.UserId, categoryId, CategoryPreferenceLevel.Important);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/UsersCategoryPreferences/{categoryId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var preference = await db.UserCategoryPreferences.FindAsync(auth.UserId, categoryId);
            Assert.Null(preference);
        });
    }

    [Fact]
    public async Task Delete_NotFound_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var categories = await GetCategories(2);
        var categoryId1 = categories[0];
        var categoryId2 = categories[1];

        await CreatePreferenceInDb(auth.UserId, categoryId1, CategoryPreferenceLevel.Important);


        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/UsersCategoryPreferences/{categoryId2}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NoProfile_ShouldReturn403()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreatePreferenceInDb(auth.UserId, categoryId, CategoryPreferenceLevel.Important);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/UsersCategoryPreferences/{categoryId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }




    private async Task<Guid> GetAnyCategoryId()
    {
        using var scope = _fixture.Factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.ExpenseCategories.Select(c => c.Id).FirstAsync();
    }

    private async Task<List<Guid>> GetCategories(int count)
    {
        using var scope = _fixture.Factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.ExpenseCategories.Take(2).Select(c => c.Id).ToListAsync();
    }

    private async Task CreatePreferenceInDb(Guid userId, Guid categoryId, CategoryPreferenceLevel level)
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var repo = sp.GetRequiredService<IUserCategoryPreferencesRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
        var preference = UserCategoryPreference.Create(userId, categoryId, level, DateTimeOffset.UtcNow);
        repo.Add(preference);
        await unitOfWork.SaveChangesAsync();
    }
}
