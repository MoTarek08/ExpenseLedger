using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class VehicleOwnershipCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.VehicleOwnership.Code,
                    "Vehicle Ownership",
                    "Costs linked to owning, maintaining, insuring, registering, and keeping a personal vehicle running.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.VehicleOwnership.Fuel.Code,
                        "Fuel",
                        "Petrol, diesel, gas, charging, or any other energy used to operate a personal vehicle."),

                    new SubCategorySeed(
                        CategoryCodes.VehicleOwnership.Maintenance.Code,
                        "Maintenance",
                        "Regular servicing and upkeep such as oil changes, inspections, tire rotations, and routine mechanical care."),

                    new SubCategorySeed(
                        CategoryCodes.VehicleOwnership.Insurance.Code,
                        "Insurance",
                        "Payments for vehicle insurance policies, renewals, and related coverage costs."),

                    new SubCategorySeed(
                        CategoryCodes.VehicleOwnership.Registration.Code,
                        "Registration",
                        "Vehicle licensing, registration renewal, plates, and official ownership-related fees."),

                    new SubCategorySeed(
                        CategoryCodes.VehicleOwnership.Repairs.Code,
                        "Repairs",
                        "Unplanned fixes and repair work for the vehicle, including parts and labor."),

                    new SubCategorySeed(
                        CategoryCodes.VehicleOwnership.CarWash.Code,
                        "Car Wash",
                        "Cleaning and detailing costs for a personal vehicle."),

                    new SubCategorySeed(
                        CategoryCodes.VehicleOwnership.Other.Code,
                        "Other",
                        "Any vehicle ownership cost that does not fit the other subcategories.")
                    });
            }
        }
    }
