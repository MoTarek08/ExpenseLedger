using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class WorkCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Work.Code,
                    "Work",
                    "Business or professional spending, including tools, services, and work-related purchases.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Work.Software.Code,
                        "Software",
                        "Professional software subscriptions, licenses, or digital tools used for work."),

                    new SubCategorySeed(
                        CategoryCodes.Work.Tools.Code,
                        "Tools",
                        "Work-related tools, utilities, and practical items used to do a job or run a business."),

                    new SubCategorySeed(
                        CategoryCodes.Work.Equipment.Code,
                        "Equipment",
                        "Larger work-related items such as devices, gear, machines, or office equipment."),

                    new SubCategorySeed(
                        CategoryCodes.Work.BusinessTravel.Code,
                        "Business Travel",
                        "Travel costs that are specifically related to work, business, or professional duties."),

                    new SubCategorySeed(
                        CategoryCodes.Work.Training.Code,
                        "Training",
                        "Work-related learning, training, certifications, and development costs."),

                    new SubCategorySeed(
                        CategoryCodes.Work.Services.Code,
                        "Services",
                        "Professional services paid for work or business purposes."),

                    new SubCategorySeed(
                        CategoryCodes.Work.Other.Code,
                        "Other",
                        "Any work expense that does not fit the listed work subcategories.")
                    });
            }
        }
    }
