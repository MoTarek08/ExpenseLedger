using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class CharityCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Charity.Code,
                    "Charity",
                    "Voluntary giving to people, organizations, or causes without expecting anything back.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Charity.Donations.Code,
                        "Donations",
                        "General donations to charities, non-profits, campaigns, or people in need."),

                    new SubCategorySeed(
                        CategoryCodes.Charity.CommunitySupport.Code,
                        "Community Support",
                        "Support given to local community initiatives, informal help, or neighborhood causes."),

                    new SubCategorySeed(
                        CategoryCodes.Charity.DisasterRelief.Code,
                        "Disaster Relief",
                        "Giving to emergency aid, disaster recovery, or relief campaigns."),

                    new SubCategorySeed(
                        CategoryCodes.Charity.Other.Code,
                        "Other",
                        "Any charitable expense that does not fit the listed charity subcategories.")
                    });
            }
        }
    }
