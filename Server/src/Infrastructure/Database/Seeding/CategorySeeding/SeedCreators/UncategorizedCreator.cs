using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class UncategorizedCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Uncategorized.Code,
                    "Uncategorized",
                    "Anything that does not fit the main category tree cleanly",
                    new List<SubCategorySeed>());
            }
        }
    }
