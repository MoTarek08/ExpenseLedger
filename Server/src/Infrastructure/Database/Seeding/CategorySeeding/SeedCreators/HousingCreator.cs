using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class HousingCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Housing.Code,
                    "Housing",
                    "Costs related to where you live and how you keep your home functional, comfortable, and maintained.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Housing.Rent.Code,
                        "Rent",
                        "Monthly or periodic payments for living in a rented home, apartment, room, or other residential space."),

                    new SubCategorySeed(
                        CategoryCodes.Housing.Mortgage.Code,
                        "Mortgage",
                        "Payments toward owning a home through a mortgage or home financing agreement."),

                    new SubCategorySeed(
                        CategoryCodes.Housing.HomeMaintenance.Code,
                        "Home Maintenance",
                        "Repairs, upkeep, and fixing things in the home such as plumbing, electrical work, painting, and small renovations."),

                    new SubCategorySeed(
                        CategoryCodes.Housing.Furniture.Code,
                        "Furniture",
                        "Furniture bought for the home such as beds, sofas, tables, chairs, and storage pieces."),

                    new SubCategorySeed(
                        CategoryCodes.Housing.Appliances.Code,
                        "Appliances",
                        "Home appliances and large household devices such as refrigerators, washing machines, microwaves, and stoves, Including thier maintaince & repair expenses."),

                    new SubCategorySeed(
                        CategoryCodes.Housing.HouseholdSupplies.Code,
                        "Household Supplies",
                        "Consumable household items and daily-use home products such as cleaning supplies, tissue, trash bags, and similar essentials."),

                    new SubCategorySeed(
                        CategoryCodes.Housing.Other.Code,
                        "Other",
                        "Any housing-related expense that does not fit the other housing subcategories.")
                    });
            }
        }
    }
