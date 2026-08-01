using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedingCatalogNamespace;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seeding.CategorySeeding.CategorySeederNamespace
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(DbContext dbContext)
        {

            foreach (var seed in CategorySeedCatalog.All)
            {
                var categoryEntry = ExpenseCategory.Create
                    (
                    seed.Code,
                    seed.Name,
                    seed.Desctiption
                    );

                dbContext.Add(categoryEntry);

                var subCategories = seed.SubCategories
                    .Select(s => ExpenseSubCategory.Create(categoryEntry.Id, s.Code, s.Name, s.Description))
                    .ToArray();

                dbContext.AddRange(subCategories);
            }
            await dbContext.SaveChangesAsync();
        }

        public static void Seed(DbContext dbContext)
        {
            foreach (var seed in CategorySeedCatalog.All)
            {
                var categoryEntry = ExpenseCategory.Create
                    (
                    seed.Code,
                    seed.Name,
                    seed.Desctiption
                    );
                dbContext.Add(categoryEntry);

                var subCategories = seed.SubCategories
                    .Select(s => ExpenseSubCategory.Create(categoryEntry.Id, s.Code, s.Name, s.Description))
                    .ToArray();

                dbContext.AddRange(subCategories);
            }
            dbContext.SaveChanges();
        }
    }
}
