using Application.Interfaces.Repositories;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal.Models;
using Host.Models;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.UseCases.NotificationsUseCases.Models;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Domain.Entities.DomainEnums;


namespace IntegrationTests;

public class SpendingGoalsControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public SpendingGoalsControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async ValueTask InitializeAsync() => await _fixture.ResetAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Create_Success_ShouldReturn201()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/SpendingGoals")
        {
            Content = JsonContent.Create(new CreateSpendingGoalRequestModel(
                Description: "Save for vacation",
                MaximumTargetAmount: 2000m,
                MinimumTargetAmount: 1000m,
                StartDate: Today,
                EndDate: Today.AddDays(60)))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResourceId<Guid>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);

        using var scope = _fixture.Factory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISpendingGoalsRepository>();
        var goal = await repo.FindAsync(body.Id, CancellationToken.None);
        Assert.NotNull(goal);
        Assert.Equal(1000m, goal.MinimumTargetAmount);
        Assert.Equal(2000m, goal.MaximumTargetAmount);
    }

    [Fact]
    public async Task Create_Duplicate_ShouldReturn409()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        await SpendingGoalBuilder.Create(_fixture, auth.UserId)
            .WithPeriod(Today, Today.AddDays(30))
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/SpendingGoals")
        {
            Content = JsonContent.Create(new CreateSpendingGoalRequestModel(
                Description: null,
                MaximumTargetAmount: 2000m,
                MinimumTargetAmount: 1000m,
                StartDate: Today,
                EndDate: Today.AddDays(30)))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_NoFinancialProfile_ShouldReturn403()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/SpendingGoals")
        {
            Content = JsonContent.Create(new CreateSpendingGoalRequestModel(
                Description: null,
                MaximumTargetAmount: 2000m,
                MinimumTargetAmount: 1000m,
                StartDate: Today,
                EndDate: Today.AddDays(30)))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Unauthenticated_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/SpendingGoals")
        {
            Content = JsonContent.Create(new CreateSpendingGoalRequestModel(
                Description: null,
                MaximumTargetAmount: 2000m,
                MinimumTargetAmount: 1000m,
                StartDate: Today,
                EndDate: Today.AddDays(30)))
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

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/SpendingGoals")
        {
            Content = JsonContent.Create(new CreateSpendingGoalRequestModel(
                Description: null,
                MaximumTargetAmount: 100m,
                MinimumTargetAmount: 500m,
                StartDate: Today,
                EndDate: Today.AddDays(30)))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_DescriptionOnly_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var goalId = await SpendingGoalBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/SpendingGoals/{goalId}")
        {
            Content = JsonContent.Create(new UpdateSpendingGoalRequestModel(
                Description: "Updated description",
                MinimumTargetAmount: null,
                MaximumTargetAmount: null,
                StartDate: null,
                EndDate: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_TargetsAndMeetsGoal_ShouldReturn200WithNotification()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var spentOn = Today;
        await ExpenseBuilder.Create(_fixture, auth.UserId)
            .WithAmount(300m)
            .WithSpentOn(spentOn)
            .BuildAsync();

        var goalId = await SpendingGoalBuilder.Create(_fixture, auth.UserId)
            .WithTargets(400m, 600m)
            .WithPeriod(spentOn, spentOn.AddDays(30))
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/SpendingGoals/{goalId}")
        {
            Content = JsonContent.Create(new UpdateSpendingGoalRequestModel(
                Description: null,
                MinimumTargetAmount: 200m,
                MaximumTargetAmount: 500m,
                StartDate: null,
                EndDate: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(notifications);
        Assert.Single(notifications);
        Assert.Equal(NotificationReason.GoalAchieved, notifications[0].Reason);
    }

    [Fact]
    public async Task Update_CompletedGoal_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var pastEnd = Today.AddDays(-2);
        var pastStart = pastEnd.AddDays(-10);
        var goalId = await SpendingGoalBuilder.Create(_fixture, auth.UserId)
            .WithPeriod(pastStart, pastEnd)
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/SpendingGoals/{goalId}")
        {
            Content = JsonContent.Create(new UpdateSpendingGoalRequestModel(
                Description: "trying to update",
                MinimumTargetAmount: null,
                MaximumTargetAmount: null,
                StartDate: null,
                EndDate: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WrongOwner_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, owner.UserId).BuildAsync();
        var goalId = await SpendingGoalBuilder.Create(_fixture, owner.UserId).BuildAsync();

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, other.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/SpendingGoals/{goalId}")
        {
            Content = JsonContent.Create(new UpdateSpendingGoalRequestModel(
                Description: "hacked",
                MinimumTargetAmount: null,
                MaximumTargetAmount: null,
                StartDate: null,
                EndDate: null))
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

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/SpendingGoals/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new UpdateSpendingGoalRequestModel(
                Description: "test",
                MinimumTargetAmount: null,
                MaximumTargetAmount: null,
                StartDate: null,
                EndDate: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_Unauthenticated_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/SpendingGoals/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new UpdateSpendingGoalRequestModel(
                Description: "test",
                MinimumTargetAmount: null,
                MaximumTargetAmount: null,
                StartDate: null,
                EndDate: null))
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

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/SpendingGoals/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new UpdateSpendingGoalRequestModel(
                Description: null,
                MinimumTargetAmount: null,
                MaximumTargetAmount: null,
                StartDate: null,
                EndDate: null))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Success_ShouldReturnGoal()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var goalId = await SpendingGoalBuilder.Create(_fixture, auth.UserId)
            .WithDescription("My goal")
            .WithPeriod(Today.AddDays(1), Today.AddDays(31))
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/SpendingGoals/{goalId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SpendingGoalDto>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(goalId, body.Id);
        Assert.Equal(SpendingGoalStatus.Pending, body.Status);
    }

    [Fact]
    public async Task GetById_NotFound_ShouldReturn404()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/SpendingGoals/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync($"/api/v1/SpendingGoals/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByStatus_ShouldReturnGoalsByStatus()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var pendingGoalId = await SpendingGoalBuilder.Create(_fixture, auth.UserId)
            .WithPeriod(
                Today.AddDays(1),
                Today.AddDays(31))
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/SpendingGoals/Pending");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<GetSpendingGoalsByStatusDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal(pendingGoalId, body[0].Id);
    }


    [Fact]
    public async Task GetByStatus_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/SpendingGoals/Pending", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByStatus_InvalidQueryParams_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/SpendingGoals/Pending?PageSize=-1");
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
        var goalId = await SpendingGoalBuilder.Create(_fixture, auth.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/SpendingGoals/{goalId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _fixture.Factory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISpendingGoalsRepository>();
        var goal = await repo.FindAsync(goalId, CancellationToken.None);
        Assert.Null(goal);
    }

    [Fact]
    public async Task Delete_WrongOwner_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, owner.UserId).BuildAsync();
        var goalId = await SpendingGoalBuilder.Create(_fixture, owner.UserId).BuildAsync();

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, other.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/SpendingGoals/{goalId}");
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

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/SpendingGoals/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.DeleteAsync($"/api/v1/SpendingGoals/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
