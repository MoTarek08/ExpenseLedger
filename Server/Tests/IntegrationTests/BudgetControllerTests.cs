using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Host.Models;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;

namespace IntegrationTests;

public class BudgetControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public BudgetControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async Task InitializeAsync() => await _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetRemaining_Success_ShouldReturnBudget()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithMonthlyIncome(5000m)
            .WithResetDay(15)
            .BuildAsync();

        await ExpenseBuilder.Create(_fixture, auth.UserId)
            .WithAmount(100m)
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/budget/remaining");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetRemainingBudgetResponse>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.True(body.Budget > 0);
        Assert.True(body.Budget <= 5000m);
    }

    [Fact]
    public async Task GetRemaining_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/budget/remaining");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRemaining_NoFinancialProfile_ShouldReturn403()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/budget/remaining");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
