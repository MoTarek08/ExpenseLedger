using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class SportsCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Sports.Code,
                    "Sports",
                    "Spending related to participating in sports, joining clubs or teams, and taking part in competitions or organized sport activity.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Sports.ClubMembership.Code,
                        "Club Membership",
                        "Membership fees for sports clubs, training clubs, or organized sport facilities."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.TeamFees.Code,
                        "Team Fees",
                        "Fees paid to join or support a sports team or group."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.Coaching.Code,
                        "Coaching",
                        "Paid sports coaching, lessons, or personal skill development in a sport."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.TournamentFees.Code,
                        "Tournament Fees",
                        "Entry fees or participation costs for tournaments, competitions, or matches."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.LeagueFees.Code,
                        "League Fees",
                        "Payments related to league participation, registration, or competition structure."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.CourtFieldRental.Code,
                        "Court & Field Rental",
                        "Payments for using courts, fields, pitches, arenas, or sports spaces."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.SportsEquipment.Code,
                        "Sports Equipment",
                        "Equipment specifically used to play a sport, such as balls, rackets, guards, or training gear."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.Uniforms.Code,
                        "Uniforms",
                        "Sport-specific clothes or uniforms worn for team play or competitive activity."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.SportsTravel.Code,
                        "Sports Travel",
                        "Travel and transport costs directly related to sports participation or competitions."),

                    new SubCategorySeed(
                        CategoryCodes.Sports.Other.Code,
                        "Other",
                        "Any sports expense that does not fit the listed sports subcategories.")
                    });
            }
        }
    }
