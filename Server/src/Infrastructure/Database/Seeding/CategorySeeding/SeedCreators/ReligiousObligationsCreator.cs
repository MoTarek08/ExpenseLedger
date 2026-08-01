using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class ReligiousObligationsCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.ReligiousObligations.Code,
                    "Religious Obligations",
                    "Required or expected religious spending such as giving, offerings, or obligations tied to faith practice.",
                    new List<SubCategorySeed>());
            }
        }
    }
