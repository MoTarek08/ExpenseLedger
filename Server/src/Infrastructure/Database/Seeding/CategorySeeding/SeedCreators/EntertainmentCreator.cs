using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class EntertainmentCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Entertainment.Code,
                    "Entertainment",
                    "Leisure spending for fun, social activity, enjoyment, and non-essential free-time experiences.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Entertainment.Cinema.Code,
                        "Cinema",
                        "Movie tickets, theater visits, and paid screenings."),

                    new SubCategorySeed(
                        CategoryCodes.Entertainment.Gaming.Code,
                        "Gaming",
                        "game purchases, in-game purchases and other gaming-related spending."),

                    new SubCategorySeed(
                        CategoryCodes.Entertainment.Parties.Code,
                        "Parties",
                        "Social gatherings, party entry, celebration spending, and event-related fun outside the normal dining context."),

                    new SubCategorySeed(
                        CategoryCodes.Entertainment.Outing.Code,
                        "Outing",
                        "Leisure visits such as museums, amusement parks, arcades, attractions, and general hangout activities."),

                    new SubCategorySeed(
                        CategoryCodes.Entertainment.Events.Code,
                        "Events",
                        "Concerts, shows, performances, festivals, and similar entertainment events."),

                    new SubCategorySeed(
                        CategoryCodes.Entertainment.Hobbies.Code,
                        "Hobbies",
                        "Money spent on personal fun activities, creative hobbies, or recreational interests."),

                    new SubCategorySeed(
                        CategoryCodes.Entertainment.Other.Code,
                        "Other",
                        "Any entertainment expense that does not fit the listed entertainment subcategories.")
                    });
            }
        }
    }
