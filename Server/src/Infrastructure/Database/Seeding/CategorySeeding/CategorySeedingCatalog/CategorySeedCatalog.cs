using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;
using Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace;
namespace Infrastructure.Database.Seeding.CategorySeeding.CategorySeedingCatalogNamespace
{
    public static class CategorySeedCatalog
    {
        public static readonly IReadOnlyList<CategorySeed> All =
        [
        FoodCreator.Create(),
        HousingCreator.Create(),
        TransportationCreator.Create(),
        VehicleOwnershipCreator.Create(),
        HealthcareCreator.Create(),
        ShoppingCreator.Create(),
        EntertainmentCreator.Create(),
        EducationCreator.Create(),
        BillsCreator.Create(),
        FinancialCreator.Create(),
        FamilyCreator.Create(),
        TravelCreator.Create(),
        WorkCreator.Create(),
        CharityCreator.Create(),
        GiftsCreator.Create(),
        ReligiousObligationsCreator.Create(),
        FitnessCreator.Create(),
        SportsCreator.Create(),
        PersonalCareCreator.Create(),
        UncategorizedCreator.Create()
        ];
    }
}
