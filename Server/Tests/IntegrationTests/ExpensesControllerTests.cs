using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ExpensesUseCases.CreateExpense.Models;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Application.UseCases.ExpensesUseCases.UpdateExpense.Models;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.FileObjectNamespace;
using FakeItEasy;
using IntegrationTests.BackgroundJobs;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace IntegrationTests;

public class ExpensesControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public ExpensesControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.Factory.CreateClient();
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.ResetAsync();
        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        fake.ManualExpensesIdsThatTriggeredBackgroundJobs.Clear();
        fake.ExpensesIdsThatTriggeredCheckGoalAchievement.Clear();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Search_NoFilter_ShouldReturnAll()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var id1 = await CreateExpenseInDb(auth.UserId, categoryId);
        var id2 = await CreateExpenseInDb(auth.UserId, categoryId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Expenses/search");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ExpenseDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }

    [Fact]
    public async Task Search_WithCategoryFilter_ShouldReturnFiltered()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categories = await GetCategories(2);
        var catId1 = categories[0];
        var catId2 = categories[1];
        await CreateExpenseInDb(auth.UserId, catId1);
        await CreateExpenseInDb(auth.UserId, catId2);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/v1/Expenses/search?CategoryIds={catId1}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ExpenseDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Fact]
    public async Task Search_Unauthenticated_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/Expenses/search", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByDay_HasExpenses_ShouldReturnList()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await CreateExpenseInDb(auth.UserId, categoryId, spentOn: today);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/v1/Expenses?Day={today:yyyy-MM-dd}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ExpenseDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Single(body);
    }

    [Fact]
    public async Task GetByDay_NoExpenses_ShouldReturnEmptyList()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/v1/Expenses?Day={today:yyyy-MM-dd}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ExpenseDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task GetById_Success_ShouldReturnExpense()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/v1/Expenses/{expenseId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ExpenseDto>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(expenseId, body.Id);
        Assert.Equal(auth.UserId, body.UserId);
    }

    [Fact]
    public async Task GetById_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, owner.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(owner.UserId, categoryId);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/v1/Expenses/{expenseId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Success_ShouldReturn201()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var requestModel = new CreateExpenseRequestModel(
            categoryId, "Test expense", 50m, today, null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Expenses")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateExpenseResponseModel>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var expense = await db.Expenses.FindAsync(body.ExpenseId);
            Assert.NotNull(expense);
            Assert.Equal(auth.UserId, expense.UserId);
            Assert.Equal(50m, expense.Amount);
        });
    }

    [Fact]
    public async Task Create_Success_ShouldTriggerBackgroundJobs()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var requestModel = new CreateExpenseRequestModel(
            categoryId, "Test", 100m, today, null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Expenses")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateExpenseResponseModel>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Contains(body.ExpenseId, fake.ManualExpensesIdsThatTriggeredBackgroundJobs);
    }

    [Fact]
    public async Task Create_InvalidInput_ShouldReturn400()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var requestModel = new CreateExpenseRequestModel(
            Guid.Empty, "Test", -10m, default, null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Expenses")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Unauthenticated_ShouldReturn401()
    {
        var categoryId = await GetAnyCategoryId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var requestModel = new CreateExpenseRequestModel(
            categoryId, "Test", 50m, today, null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Expenses")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_IncreaseAmount_ShouldTriggerBudgetJobs()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithMonthlyIncome(5000m).WithResetDay(DateTime.UtcNow.Day).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId, amount: 50m);

        var requestModel = new UpdateExpenseRequestModel(
            null, 150m, null, null, null);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Contains(expenseId, fake.GeneratedExpensesIdsThatTriggeredBackgroundJobs);
    }

    [Fact]
    public async Task Update_IncreaseAmountOverBudget_ShouldReturnNotifications()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithMonthlyIncome(100m).WithResetDay(DateTime.UtcNow.Day).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        await CreateExpenseInDb(auth.UserId, categoryId, amount: 50m);
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId, amount: 50m);

        var requestModel = new UpdateExpenseRequestModel(
            null, 200m, null, null, null);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonHelper.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.NotEmpty(body);
    }

    [Fact]
    public async Task Update_ChangeCategory_ShouldTriggerBackgroundJobs()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categories = await GetCategories(2);
        var catId1 = categories[0];
        var catId2 = categories[1];
        var expenseId = await CreateExpenseInDb(auth.UserId, catId1);

        var requestModel = new UpdateExpenseRequestModel(
            null, null, catId2, null, null);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Contains(expenseId, fake.GeneratedExpensesIdsThatTriggeredBackgroundJobs);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var expense = await db.Expenses.FindAsync(expenseId);
            Assert.NotNull(expense);
            Assert.Equal(catId2, expense.CategoryId);
        });
    }

    [Fact]
    public async Task Update_ChangeCategoryAndSubCategory_ShouldTriggerBackgroundJobs()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var (mainId, subId) = await GetCategoryWithSubCategory();
        var expenseId = await CreateExpenseInDb(auth.UserId, mainId);

        var requestModel = new UpdateExpenseRequestModel(
            null, null, mainId, subId, null);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Contains(expenseId, fake.GeneratedExpensesIdsThatTriggeredBackgroundJobs);
    }

    [Fact]
    public async Task Update_ChangeSubCategoryOnly_ShouldUpdateInPlace()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var (mainId, subId) = await GetCategoryWithSubCategory();
        var expenseId = await CreateExpenseInDb(auth.UserId, mainId);

        var requestModel = new UpdateExpenseRequestModel(
            null, null, null, subId, null);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.DoesNotContain(expenseId, fake.GeneratedExpensesIdsThatTriggeredBackgroundJobs);
        Assert.DoesNotContain(expenseId, fake.ExpensesIdsThatTriggeredCheckGoalAchievement);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var expense = await db.Expenses.Include(e => e.SubCategory).FirstAsync(e => e.Id == expenseId);
            Assert.NotNull(expense.SubCategory);
            Assert.Equal(subId, expense.SubCategory.Id);
        });
    }

    [Fact]
    public async Task Update_SpentOnWithinPeriod_ShouldTriggerGoalCheck()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId)
            .WithResetDay(DateTime.UtcNow.Day).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId,
            spentOn: DateOnly.FromDateTime(DateTime.UtcNow));

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var requestModel = new UpdateExpenseRequestModel(
            null, null, null, null, tomorrow);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fake = (FakeBackgroundJobsService)_fixture.Factory.Services
            .GetRequiredService<IBackgroundJobsService>();
        Assert.Contains(expenseId, fake.ExpensesIdsThatTriggeredCheckGoalAchievement);
    }

    [Fact]
    public async Task Update_CategorySubMismatch_ShouldReturnError()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId);

        var (otherMainId, otherSubId) = await GetCategoryWithSubCategory();
        var unrelatedSubId = await GetSubCategoryForDifferentMain(otherMainId);

        var requestModel = new UpdateExpenseRequestModel(
            null, null, otherMainId, unrelatedSubId, null);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, owner.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(owner.UserId, categoryId);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, other.UserId).BuildAsync();

        var requestModel = new UpdateExpenseRequestModel(
            "hacked", null, null, null, null);
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/v1/Expenses/{expenseId}")
        {
            Content = JsonHelper.Serialize(requestModel)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Success_ShouldReturn204()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId);

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/Expenses/{expenseId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var expense = await db.Expenses.FindAsync(expenseId);
            Assert.Null(expense);
        });
    }

    [Fact]
    public async Task Delete_NotOwned_ShouldReturn404()
    {
        var owner = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, owner.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(owner.UserId, categoryId);

        var other = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, other.UserId).BuildAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/Expenses/{expenseId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExpenseWithLinkedFile_ShouldDeleteObjectFromStorageImmediately()
    {
        var auth = await AuthenticationScenarioBuilder.Create(_fixture)
            .WithRefreshToken()
            .BuildAsync();
        await FinancialProfileBuilder.Create(_fixture, auth.UserId).BuildAsync();
        var categoryId = await GetAnyCategoryId();
        var expenseId = await CreateExpenseInDb(auth.UserId, categoryId);

        var now = DateTimeOffset.UtcNow;
        var objectKey = $"integration-test/{auth.UserId}/{Guid.NewGuid()}.jpg";
        using (var scope = _fixture.Factory.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var repo = sp.GetRequiredService<IExpensesFileObjectsRepository>();
            var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
            var file = ExpenseFileObject.CreatePendingUpload(
                auth.UserId,
                objectKey,
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                now.AddHours(-1),
                now.AddMinutes(15));
            file.MarkAsUploaded(now);
            file.LinkToExpense(expenseId);
            repo.Add(file);
            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync(A<string>._, objectKey, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/Expenses/{expenseId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        A.CallTo(() => _fixture.Factory.FakeObjectStorageClient
            .RemoveObjectAsync(A<string>._, objectKey, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        await DatabaseAssertions.Verify(_fixture, async db =>
        {
            var expense = await db.Expenses.FindAsync(expenseId);
            Assert.Null(expense);
        });
    }




    private Task<Guid> GetAnyCategoryId() => CategoryHelpers.GetAnyCategoryId(_fixture);

    private Task<List<Guid>> GetCategories(int count) => CategoryHelpers.GetCategories(_fixture, count);

    private Task<(Guid mainId, Guid subId)> GetCategoryWithSubCategory() => CategoryHelpers.GetCategoryWithSubCategory(_fixture);

    private Task<Guid> GetSubCategoryForDifferentMain(Guid excludeMainId) => CategoryHelpers.GetSubCategoryForDifferentMain(_fixture, excludeMainId);

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
}
