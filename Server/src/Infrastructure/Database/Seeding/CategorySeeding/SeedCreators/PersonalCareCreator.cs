using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class PersonalCareCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.PersonalCare.Code,
                    "Personal Care",
                    "Spending on grooming, appearance, hygiene, and personal maintenance.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.PersonalCare.Haircut.Code,
                        "Haircut",
                        "Haircuts, barbershop visits, and hair styling services."),

                    new SubCategorySeed(
                        CategoryCodes.PersonalCare.Cosmetics.Code,
                        "Cosmetics",
                        "Makeup, beauty products, and appearance-related items."),

                    new SubCategorySeed(
                        CategoryCodes.PersonalCare.Skincare.Code,
                        "Skincare",
                        "Skin treatment products and skincare routines or services."),

                    new SubCategorySeed(
                        CategoryCodes.PersonalCare.Spa.Code,
                        "Spa",
                        "Spa visits, massages, and relaxation-focused personal care spending."),

                    new SubCategorySeed(
                        CategoryCodes.PersonalCare.Gym.Code,
                        "Gym",
                        "General body-care or wellness-related gym-adjacent personal maintenance spending if you choose to classify it here."),

                    new SubCategorySeed(
                        CategoryCodes.PersonalCare.Supplements.Code,
                        "Supplements",
                        "General wellness supplements when they are used more for personal care than for training."),

                    new SubCategorySeed(
                        CategoryCodes.PersonalCare.Other.Code,
                        "Other",
                        "Any personal care expense that does not fit the listed subcategories.")
                    });
            }
        }
    }
