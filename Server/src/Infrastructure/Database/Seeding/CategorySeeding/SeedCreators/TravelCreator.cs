using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class TravelCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Travel.Code,
                    "Travel",
                    "Travel-related spending away from normal daily routine, including trips, stays, and trip necessities.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Travel.Flights.Code,
                        "Flights",
                        "Airplane tickets and flight-related travel costs."),

                    new SubCategorySeed(
                        CategoryCodes.Travel.Hotels.Code,
                        "Hotels",
                        "Hotel stays, lodging, guest houses, and overnight accommodations."),

                    new SubCategorySeed(
                        CategoryCodes.Travel.Visa.Code,
                        "Visa",
                        "Visa applications, travel permits, and entry-related travel documents."),

                    new SubCategorySeed(
                        CategoryCodes.Travel.Luggage.Code,
                        "Luggage",
                        "Bags, suitcases, and travel storage equipment."),

                    new SubCategorySeed(
                        CategoryCodes.Travel.Activities.Code,
                        "Activities",
                        "Tourist activities, excursions, attractions, and trip experiences."),

                    new SubCategorySeed(
                        CategoryCodes.Travel.TravelMeals.Code,
                        "Travel Meals",
                        "Food and drink purchased while travelling and not part of regular home or daily routine."),

                    new SubCategorySeed(
                        CategoryCodes.Travel.Other.Code,
                        "Other",
                        "Any travel expense that does not fit the listed travel subcategories.")
                    });
            }
        }
    }
