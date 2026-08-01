namespace Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords
{
    public sealed record CategorySeed(string Code, string Name, string Desctiption,List<SubCategorySeed> SubCategories);
    public sealed record SubCategorySeed(string Code, string Name, string Description);

}
