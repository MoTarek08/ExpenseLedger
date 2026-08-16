using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.UseCases.UsersUseCases.GetUserProfile.ModelsNamespace;
using Application.UseCases.UsersUseCases.UpdateUser.ModelsNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;

namespace IntegrationTests;

public class UsersControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public UsersControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async ValueTask InitializeAsync() => await _fixture.ResetAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetProfileWithFinancialProfile_Success_ShouldReturnProfileWithFinancialProfileDto()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithMonthlyIncome(20000)
            .WithResetDay(1)
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserProfileDto>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(auth.Email, body.Email);
        Assert.NotEqual(default, body.RegisteredAt);
        Assert.NotNull(body.FinancialProfile);
    }

    [Fact]
    public async Task GetProfile_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/users/profile", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_Success_ShouldUpdate()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var requestModel = new UpdateUserRequestModel("NewDisplayName");
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/users")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var user = await db.Users.FindAsync(auth.UserId);
            Assert.NotNull(user);
            Assert.Equal("NewDisplayName", user.DisplayName);
        });
    }

    [Fact]
    public async Task Update_Unauthenticated_ShouldReturn401()
    {
        var requestModel = new UpdateUserRequestModel("NewName");
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/users")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_EmptyDisplayName_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var requestModel = new UpdateUserRequestModel("");
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/users")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
