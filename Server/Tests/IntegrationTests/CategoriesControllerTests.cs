using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Models;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;

namespace IntegrationTests;

public class CategoriesControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public CategoriesControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async ValueTask InitializeAsync() => await _fixture.ResetAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetAll_AsAdmin_ShouldReturnAllCategories()
    {
        var admin = await AuthenticationScenarioBuilder.Create(_fixture)
            .AsAdmin()
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<CategoryDetails>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.NotEmpty(body);
    }

    [Fact]
    public async Task GetAll_AsUser_ShouldReturn403()
    {
        var user = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetByCode_AsAdmin_ShouldReturnExistingCategory()
    {
        var admin = await AuthenticationScenarioBuilder.Create(_fixture)
            .AsAdmin()
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories/FOOD");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CategoryDetails>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("FOOD", body.Code);
        Assert.NotEmpty(body.SubCategories);
    }

    [Fact]
    public async Task GetByCode_NonExistentCode_ShouldReturn404()
    {
        var admin = await AuthenticationScenarioBuilder.Create(_fixture)
            .AsAdmin()
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories/INVALID");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
