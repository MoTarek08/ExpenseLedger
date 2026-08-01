using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace.ModelsNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace.ModelsNamespace;
using Host.Models;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public class UsersFinancialProfilesControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public UsersFinancialProfilesControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async Task InitializeAsync() => await _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_NoProfile_ShouldReturn404()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/UsersFinancialProfiles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithProfile_ShouldReturnProfileDto()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithMonthlyIncome(5000m)
            .WithResetDay(15)
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/UsersFinancialProfiles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FinancialProfileDto>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Equal(5000m, body.MonthlyNetIncome);
        Assert.Equal(15, body.ResetDay);
        Assert.NotEqual(default, body.CreatedAt);
    }

    [Fact]
    public async Task Get_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/UsersFinancialProfiles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_Success_ShouldReturn201AndCreateProfile()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var requestModel = new CreateUserFinancialProfileRequest(5000m, 15);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/UsersFinancialProfiles")
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
            var profile = await db.UserFinancialProfiles.FindAsync(body.Id);
            Assert.NotNull(profile);
            Assert.Equal(5000m, profile.MonthlyNetIncome);
            Assert.Equal(15, profile.ResetDay);
            Assert.Equal(auth.UserId, profile.UserId);
        });
    }

    [Fact]
    public async Task Create_AlreadyExists_ShouldReturn409()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var requestModel = new CreateUserFinancialProfileRequest(5000m, 15);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/UsersFinancialProfiles")
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

        var requestModel = new CreateUserFinancialProfileRequest(-100m, 15);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/UsersFinancialProfiles")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_Success_ShouldUpdateFields()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithMonthlyIncome(5000m)
            .WithResetDay(15)
            .BuildAsync();

        var requestModel = new UpdateFinancialProfileRequestModel(8000m, 1);
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/UsersFinancialProfiles")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var profile = await db.UserFinancialProfiles
                .FirstOrDefaultAsync(p => p.UserId == auth.UserId);
            Assert.NotNull(profile);
            Assert.Equal(8000m, profile.MonthlyNetIncome);
            Assert.Equal(1, profile.ResetDay);
        });
    }

    [Fact]
    public async Task Update_NoProfile_ShouldReturn403()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var requestModel = new UpdateFinancialProfileRequestModel(8000m, 1);
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/UsersFinancialProfiles")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_Unauthenticated_ShouldReturn401()
    {
        var requestModel = new UpdateFinancialProfileRequestModel(8000m, 1);
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/UsersFinancialProfiles")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
