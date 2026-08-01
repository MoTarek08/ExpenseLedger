using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class EducationCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Education.Code,
                    "Education",
                    "Learning-related spending, including school, university, self-improvement, and skill-building costs.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Education.Courses.Code,
                        "Courses",
                        "Online or offline courses, workshops, and training programs."),

                    new SubCategorySeed(
                        CategoryCodes.Education.Books.Code,
                        "Books",
                        "Educational books, references, study materials, and learning resources."),

                    new SubCategorySeed(
                        CategoryCodes.Education.Tuition.Code,
                        "Tuition",
                        "School, university, or program tuition and enrollment fees."),

                    new SubCategorySeed(
                        CategoryCodes.Education.Certifications.Code,
                        "Certifications",
                        "Exam fees, certification fees, and qualification-related payments."),

                    new SubCategorySeed(
                        CategoryCodes.Education.SchoolSupplies.Code,
                        "School Supplies",
                        "Notebooks, stationery, pens, and other study supplies."),

                    new SubCategorySeed(
                        CategoryCodes.Education.Other.Code,
                        "Other",
                        "Any education expense that does not fit the listed education subcategories.")
                    });
            }
        }
    }
