using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense.Models;
using Application.UseCases.ScheduledExpensesUseCases.Models;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense.Models;
using Domain.Entities.DomainEnums;
using Host.Models;
using IntegrationTests.BackgroundJobs;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class ScheduledExpensesControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public ScheduledExpensesControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.ResetAsync();
        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        fake.ScheduledExpenseGenerationJobs.Clear();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Create_Success_ShouldReturn201()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await CategoryHelpers.GetAnyCategoryId(_fixture);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ScheduledExpenses")
        {
            Content = JsonContent.Create(new CreateScheduledExpenseRequestModel(
                Title: "Monthly groceries",
                Amount: 500m,
                CategoryId: categoryId,
                SubCategoryId: null,
                Cadence: CadenceInterval.Monthly,
                FirstDueOn: Today))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResourceId<Guid>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);

        using var scope = _fixture.Factory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IScheduledExpensesRepository>();
        var se = await repo.FindAsync(body.Id, CancellationToken.None);
        Assert.NotNull(se);
        Assert.Equal(500m, se.Amount);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Contains((body.Id, Today), fake.ScheduledExpenseGenerationJobs);
    }

    [Fact]
    public async Task Create_SubCategoryMismatch_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var (mainId, subId) = await CategoryHelpers.GetCategoryWithSubCategory(_fixture);
        var otherSubId = await CategoryHelpers.GetSubCategoryForDifferentMain(_fixture, mainId);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ScheduledExpenses")
        {
            Content = JsonContent.Create(new CreateScheduledExpenseRequestModel(
                Title: null,
                Amount: 300m,
                CategoryId: mainId,
                SubCategoryId: otherSubId,
                Cadence: CadenceInterval.Weekly,
                FirstDueOn: Today))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NoFinancialProfile_ShouldReturn403()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ScheduledExpenses")
        {
            Content = JsonContent.Create(new CreateScheduledExpenseRequestModel(
                Title: null,
                Amount: 100m,
                CategoryId: Guid.NewGuid(),
                SubCategoryId: null,
                Cadence: CadenceInterval.Once,
                FirstDueOn: Today))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Unauthenticated_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ScheduledExpenses")
        {
            Content = JsonContent.Create(new CreateScheduledExpenseRequestModel(
                Title: null, Amount: 100m, CategoryId: Guid.NewGuid(),
                SubCategoryId: null, Cadence: CadenceInterval.Once, FirstDueOn: Today))
        };
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidRequest_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ScheduledExpenses")
        {
            Content = JsonContent.Create(new CreateScheduledExpenseRequestModel(
                Title: null, Amount: 0, CategoryId: Guid.NewGuid(),
                SubCategoryId: null, Cadence: CadenceInterval.Once, FirstDueOn: Today))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_TitleOnly_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var seId = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/ScheduledExpenses/{seId}")
        {
            Content = JsonContent.Create(new UpdateScheduledExpenseRequestModel(
                Title: "Updated title", Amount: null, FirstDue: null, Cadence: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Empty(fake.ScheduledExpenseGenerationJobs);
    }

    [Fact]
    public async Task Update_Cadence_ShouldTriggerBackgroundJob()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var seId = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId)
            .WithCadence(CadenceInterval.Monthly)
            .BuildAsync();

        using (var scope = _fixture.Factory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IScheduledExpensesRepository>();
            var se = await repo.FindAsync(seId, CancellationToken.None);
            Assert.NotNull(se);
        }

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/ScheduledExpenses/{seId}")
        {
            Content = JsonContent.Create(new UpdateScheduledExpenseRequestModel(
                Title: null, Amount: null, FirstDue: null, Cadence: CadenceInterval.Weekly))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope2 = _fixture.Factory.CreateScope();
        var repo2 = scope2.ServiceProvider.GetRequiredService<IScheduledExpensesRepository>();
        var updated = await repo2.FindAsync(seId, CancellationToken.None);
        Assert.NotNull(updated);
        var expectedNextDue = Today.AddDays(7);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Contains((seId, expectedNextDue), fake.ScheduledExpenseGenerationJobs);
    }

    [Fact]
    public async Task Update_WrongOwner_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, owner.UserId).BuildAsync();
        var seId = await ScheduledExpenseBuilder.Create(_fixture, owner.UserId).BuildAsync();

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, other.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/ScheduledExpenses/{seId}")
        {
            Content = JsonContent.Create(new UpdateScheduledExpenseRequestModel(
                Title: "hacked", Amount: null, FirstDue: null, Cadence: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_NoFinancialProfile_ShouldReturn403()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/ScheduledExpenses/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new UpdateScheduledExpenseRequestModel(
                Title: "test", Amount: null, FirstDue: null, Cadence: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_Unauthenticated_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/ScheduledExpenses/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new UpdateScheduledExpenseRequestModel(
                Title: "test", Amount: null, FirstDue: null, Cadence: null))
        };
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_InvalidRequest_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/ScheduledExpenses/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new UpdateScheduledExpenseRequestModel(
                Title: null, Amount: null, FirstDue: null, Cadence: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Success_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var seId = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/ScheduledExpenses/{seId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _fixture.Factory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IScheduledExpensesRepository>();
        var se = await repo.FindAsync(seId, CancellationToken.None);
        Assert.Null(se);
    }

    [Fact]
    public async Task Delete_WrongOwner_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, owner.UserId).BuildAsync();
        var seId = await ScheduledExpenseBuilder.Create(_fixture, owner.UserId).BuildAsync();

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, other.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/ScheduledExpenses/{seId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NoFinancialProfile_ShouldReturn403()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/ScheduledExpenses/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.DeleteAsync($"/api/v1/ScheduledExpenses/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Success_ShouldReturnExpense()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var (mainId, subId) = await CategoryHelpers.GetCategoryWithSubCategory(_fixture);
        var seId = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId)
            .WithCategory(mainId, subId)
            .WithTitle("My scheduled")
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/ScheduledExpenses/{seId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ScheduledExpenseDto>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(seId, body.Id);
        Assert.Equal("My scheduled", body.Title);
        Assert.True(body.IsActive);
        Assert.Equal(CadenceInterval.Monthly, body.Cadence);
    }

    [Fact]
    public async Task GetById_NotFound_ShouldReturn404()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/ScheduledExpenses/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync($"/api/v1/ScheduledExpenses/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Search_Success_ShouldReturnList()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var id1 = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var id2 = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ScheduledExpenses/search");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ScheduledExpenseDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Contains(body, e => e.Id == id1);
        Assert.Contains(body, e => e.Id == id2);
    }

    [Fact]
    public async Task Search_ActiveOnly_ShouldReturnOnlyActive()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var activeId = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var cancelledId = await ScheduledExpenseBuilder.Create(_fixture, auth.UserId).BuildInactiveAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ScheduledExpenses/search?ActiveOnly=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ScheduledExpenseDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Contains(body, e => e.Id == activeId);
        Assert.DoesNotContain(body, e => e.Id == cancelledId);
    }

    [Fact]
    public async Task Search_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/ScheduledExpenses/search", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Search_InvalidQueryParams_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ScheduledExpenses/search?SortBy=InvalidColumn");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
