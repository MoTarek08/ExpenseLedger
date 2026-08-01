using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class BillsCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Bills.Code,
                    "Bills & Subsriptions",
                    "Life essential utilities, recurring service payments and essential digital services that are paid regularly rather than bought once.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Bills.Utilities.Code,
                        "Utilities",
                        "Electricity, water, gas, and similar basic home utility bills."),

                    new SubCategorySeed(
                        CategoryCodes.Bills.Internet.Code,
                        "Internet",
                        "Home internet and broadband services."),

                    new SubCategorySeed(
                        CategoryCodes.Bills.Phone.Code,
                        "Phone",
                        "Mobile plans, line rental, and phone service payments."),

                    new SubCategorySeed(
                        CategoryCodes.Bills.Software.Code,
                        "Software",
                        "Recurring digital services or software subscriptions that are paid regularly and are not one-time purchases"),

                    new SubCategorySeed(
                        CategoryCodes.Bills.DigitalServices.Code,
                        "Digital Services",
                        "Other recurring online or digital services required for maintaining quality of life, such as smart assistants, cloud-based services, or paid connected services."),

                    new SubCategorySeed(
                        CategoryCodes.Bills.Other.Code,
                        "Other",
                        "Any recurring bill that does not fit the listed bill subcategories.")
                    });
            }
        }
    }
