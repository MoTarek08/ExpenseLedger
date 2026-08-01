using Infrastructure.Database.AppDbContextNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.TestHelpers;

public static class CategoryHelpers
{
    public static async Task<Guid> GetAnyCategoryId(IntegrationTestFixture fixture)
    {
        using var scope = fixture.Factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.ExpenseCategories.Select(c => c.Id).FirstAsync();
    }

    public static async Task<List<Guid>> GetCategories(IntegrationTestFixture fixture, int count)
    {
        using var scope = fixture.Factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.ExpenseCategories.Take(count).Select(c => c.Id).ToListAsync();
    }

    public static async Task<(Guid mainId, Guid subId)> GetCategoryWithSubCategory(IntegrationTestFixture fixture)
    {
        using var scope = fixture.Factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sub = await db.ExpenseSubCategories.Include(s => s.Category).FirstAsync();
        return (sub.Category.Id, sub.Id);
    }

    public static async Task<Guid> GetSubCategoryForDifferentMain(IntegrationTestFixture fixture, Guid excludeMainId)
    {
        using var scope = fixture.Factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sub = await db.ExpenseSubCategories
            .Include(s => s.Category)
            .Where(s => s.Category.Id != excludeMainId)
            .FirstAsync();
        return sub.Id;
    }
}
