using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.Notification;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class NotificationsControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public NotificationsControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async Task InitializeAsync() => await _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCurrentPeriodNotifications_NoProfile_ShouldReturnEmptyList()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Notifications/current-period");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task GetCurrentPeriodNotifications_NotInPeriod_ShouldReturnEmptyList()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithResetDay(DateTime.UtcNow.Day)
            .BuildAsync();
        await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow.AddMonths(-2));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Notifications/current-period");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task GetCurrentPeriodNotifications_HasNotifications_ShouldReturnList()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithResetDay(DateTime.UtcNow.Day)
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Notifications/current-period");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal(notificationId, body[0].Id);
    }

    [Fact]
    public async Task GetCurrentPeriodNotifications_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/Notifications/current-period");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Success_ShouldReturnNotification()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Notifications/{notificationId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<NotificationDto>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Equal(notificationId, body.Id);
        Assert.Equal(auth.UserId, body.UserId);
    }

    [Fact]
    public async Task GetById_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(owner.UserId, DateTimeOffset.UtcNow);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Notifications/{notificationId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync($"/api/v1/Notifications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_Success_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/Notifications/{notificationId}/read");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var notification = await db.Notifications.FindAsync(notificationId);
            Assert.NotNull(notification);
            Assert.NotNull(notification.ReadAt);
        });
    }

    [Fact]
    public async Task MarkAsRead_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(owner.UserId, DateTimeOffset.UtcNow);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/Notifications/{notificationId}/read");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_AlreadyRead_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        var request1 = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/Notifications/{notificationId}/read");
        request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response1 = await _client.SendAsync(request1);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);

        var request2 = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/Notifications/{notificationId}/read");
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response2 = await _client.SendAsync(request2);

        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.PatchAsync($"/api/v1/Notifications/{Guid.NewGuid()}/read", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Success_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/Notifications/{notificationId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var notification = await db.Notifications.FindAsync(notificationId);
            Assert.NotNull(notification);
            Assert.NotNull(notification.DeletedAt);
        });
    }

    [Fact]
    public async Task Delete_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(owner.UserId, DateTimeOffset.UtcNow);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/Notifications/{notificationId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.DeleteAsync($"/api/v1/Notifications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Restore_Success_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        using (var scope = _fixture.Factory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<INotificationsRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notification = await repo.FindAsync(notificationId, CancellationToken.None);
            Assert.NotNull(notification);
            notification.MarkAsDeleted(DateTimeOffset.UtcNow);
            await unitOfWork.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/Notifications/{notificationId}/restore");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var notification = await db.Notifications.FindAsync(notificationId);
            Assert.NotNull(notification);
            Assert.Null(notification.DeletedAt);
        });
    }

    [Fact]
    public async Task Restore_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(owner.UserId, DateTimeOffset.UtcNow);

        using (var scope = _fixture.Factory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<INotificationsRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notification = await repo.FindAsync(notificationId, CancellationToken.None);
            Assert.NotNull(notification);
            notification.MarkAsDeleted(DateTimeOffset.UtcNow);
            await unitOfWork.SaveChangesAsync();
        }

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/Notifications/{notificationId}/restore");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Restore_ExpiredWindow_ShouldReturn404()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var notificationId = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);

        using (var scope = _fixture.Factory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<INotificationsRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notification = await repo.FindAsync(notificationId, CancellationToken.None);
            Assert.NotNull(notification);
            notification.MarkAsDeleted(DateTimeOffset.UtcNow.AddHours(-2));
            await unitOfWork.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/Notifications/{notificationId}/restore");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Restore_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.PatchAsync($"/api/v1/Notifications/{Guid.NewGuid()}/restore", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Search_NoFilter_ShouldReturnNotifications()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var id1 = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);
        var id2 = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Notifications/search");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }

    [Fact]
    public async Task Search_WithPagination_ShouldReturnPaginated()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        var id1 = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow);
        var id2 = await CreateNotificationInDb(auth.UserId, DateTimeOffset.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Notifications/search?PageSize=1");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonHelper.Options);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Fact]
    public async Task Search_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/Notifications/search");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> CreateNotificationInDb(Guid userId, DateTimeOffset createdAt, DateOnly? budgetPeriodStart = null)
    {
        var expenseId = await ExpenseBuilder.Create(_fixture, userId).BuildAsync();

        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var repo = sp.GetRequiredService<INotificationsRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var notification = Notification.BudgentWentBelowQuarter(
            userId,
            expenseId,
            budgetPeriodStart ?? DateOnly.FromDateTime(DateTime.UtcNow),
            createdAt);

        repo.Add(notification);
        await unitOfWork.SaveChangesAsync();
        return notification.Id;
    }
}
