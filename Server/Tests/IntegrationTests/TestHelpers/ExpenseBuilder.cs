using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.ExpenseNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.TestHelpers;

public class ExpenseBuilder
{
    private readonly IntegrationTestFixture _fixture;
    private readonly Guid _userId;
    private Guid _categoryId;
    private decimal _amount = 100m;
    private DateOnly _spentOn = DateOnly.FromDateTime(DateTime.UtcNow);

    private ExpenseBuilder(IntegrationTestFixture fixture, Guid userId)
    {
        _fixture = fixture;
        _userId = userId;
    }

    public static ExpenseBuilder Create(IntegrationTestFixture fixture, Guid userId)
        => new(fixture, userId);

    public ExpenseBuilder WithCategory(Guid categoryId)
    { _categoryId = categoryId; return this; }

    public ExpenseBuilder WithAmount(decimal amount)
    { _amount = amount; return this; }

    public ExpenseBuilder WithSpentOn(DateOnly spentOn)
    { _spentOn = spentOn; return this; }

    public async Task<Guid> BuildAsync()
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;

        if (_categoryId == Guid.Empty)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            var category = await db.ExpenseCategories.FirstAsync();
            _categoryId = category.Id;
        }

        var expensesRepo = sp.GetRequiredService<IExpensesRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var expense = Expense.CreateManualExpense(
            _userId, _categoryId, null, _amount, _spentOn, DateTimeOffset.UtcNow);
        expensesRepo.Add(expense);
        await unitOfWork.SaveChangesAsync();

        return expense.Id;
    }
}
