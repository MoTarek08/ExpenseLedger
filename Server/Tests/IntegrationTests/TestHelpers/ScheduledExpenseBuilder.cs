using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.DomainEnums;
using Domain.Entities.ScheduledExpenseNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.TestHelpers;

public class ScheduledExpenseBuilder
{
    private readonly IntegrationTestFixture _fixture;
    private readonly Guid _userId;
    private Guid _categoryId;
    private Guid? _subCategoryId;
    private decimal _amount = 500m;
    private CadenceInterval _cadence = CadenceInterval.Monthly;
    private DateOnly _firstDueOn = DateOnly.FromDateTime(DateTime.UtcNow);
    private string? _title = "Test scheduled expense";

    private ScheduledExpenseBuilder(IntegrationTestFixture fixture, Guid userId)
    {
        _fixture = fixture;
        _userId = userId;
    }

    public static ScheduledExpenseBuilder Create(IntegrationTestFixture fixture, Guid userId)
        => new(fixture, userId);

    public ScheduledExpenseBuilder WithCategory(Guid categoryId, Guid? subCategoryId = null)
    { _categoryId = categoryId; _subCategoryId = subCategoryId; return this; }

    public ScheduledExpenseBuilder WithAmount(decimal amount)
    { _amount = amount; return this; }

    public ScheduledExpenseBuilder WithCadence(CadenceInterval cadence)
    { _cadence = cadence; return this; }

    public ScheduledExpenseBuilder WithFirstDue(DateOnly firstDueOn)
    { _firstDueOn = firstDueOn; return this; }

    public ScheduledExpenseBuilder WithTitle(string? title)
    { _title = title; return this; }

    public async Task<Guid> BuildAsync()
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var repo = sp.GetRequiredService<IScheduledExpensesRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        if (_categoryId == Guid.Empty)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            var category = await db.ExpenseCategories.FirstAsync();
            _categoryId = category.Id;
        }

        var entry = ScheduledExpense.Create(
            _userId, _title, _amount, _categoryId, _subCategoryId, _cadence, _firstDueOn, DateTimeOffset.UtcNow);

        repo.Add(entry);
        await unitOfWork.SaveChangesAsync();
        return entry.Id;
    }

    /// Builds a once-cadence scheduled expense that has already been processed, so it is inactive.
    public async Task<Guid> BuildInactiveAsync()
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var repo = sp.GetRequiredService<IScheduledExpensesRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        if (_categoryId == Guid.Empty)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            var category = await db.ExpenseCategories.FirstAsync();
            _categoryId = category.Id;
        }

        var pastDue = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var entry = ScheduledExpense.Create(
            _userId, _title, _amount, _categoryId, _subCategoryId, CadenceInterval.Once, pastDue, DateTimeOffset.UtcNow);

        entry.MarkAsProcessed(pastDue);

        repo.Add(entry);
        await unitOfWork.SaveChangesAsync();
        return entry.Id;
    }
}
