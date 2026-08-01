using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class GiftsCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Gifts.Code,
                    "Gifts",
                    "Money spent on other people as gifts for occasions, celebrations, or personal giving.",
                    new List<SubCategorySeed>());
            }
        }
    }
