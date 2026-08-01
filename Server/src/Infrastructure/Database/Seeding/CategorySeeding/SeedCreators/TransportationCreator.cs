using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class TransportationCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Transportation.Code,
                    "Transportation",
                    "Money spent on moving around using public transport or paid rides",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Transportation.PublicTransport.Code,
                        "Public Transport",
                        "Buses, metro, trains, minibuses, and similar shared transport used to get from one place to another."),

                    new SubCategorySeed(
                        CategoryCodes.Transportation.TaxiRideshare.Code,
                        "Taxi & Rideshare",
                        "Taxis, Uber-style rides, private hired cars, and similar point-to-point transport."),

                    new SubCategorySeed(
                        CategoryCodes.Transportation.Tolls.Code,
                        "Tolls",
                        "Road, bridge, or highway toll payments made while traveling."),

                    new SubCategorySeed(
                        CategoryCodes.Transportation.Other.Code,
                        "Other",
                        "Any transport expense that does not fit the listed transportation subcategories.")
                    });
            }
        }
    }
