using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class FamilyCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Family.Code,
                    "Family",
                    "Spending related to family life, dependents, children, parents, and household members.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Family.Childcare.Code,
                        "Childcare",
                        "Nursery, babysitting, daycare, and other child supervision costs."),

                    new SubCategorySeed(
                        CategoryCodes.Family.BabySupplies.Code,
                        "Baby Supplies",
                        "Diapers, formula, baby clothes, and similar items for infants."),

                    new SubCategorySeed(
                        CategoryCodes.Family.Parents.Code,
                        "Parents",
                        "Expenses related to supporting parents or elder family members."),

                    new SubCategorySeed(
                        CategoryCodes.Family.Allowance.Code,
                        "Allowance",
                        "Regular money given to children or dependents as allowance."),

                    new SubCategorySeed(
                        CategoryCodes.Family.Pets.Code,
                        "Pets",
                        "Pet food, pet care, grooming, vet basics, and animal-related family spending."),

                    new SubCategorySeed(
                        CategoryCodes.Family.Other.Code,
                        "Other",
                        "Any family expense that does not fit the listed family subcategories.")
                    });
            }
        }
    }
