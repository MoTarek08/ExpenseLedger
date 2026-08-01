using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class ShoppingCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Shopping.Code,
                    "Shopping",
                    "Purchases of physical goods and non-food items that are not primarily housing, health, or work expenses.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Shopping.Clothing.Code,
                        "Clothing",
                        "Shirts, pants, jackets, dresses, and other regular clothing purchases."),

                    new SubCategorySeed(
                        CategoryCodes.Shopping.Shoes.Code,
                        "Shoes",
                        "Footwear purchases including casual shoes, sports shoes, formal shoes, and sandals."),

                    new SubCategorySeed(
                        CategoryCodes.Shopping.Electronics.Code,
                        "Electronics",
                        "Gadgets and electronic devices such as phones, headphones, accessories, and small devices."),

                    new SubCategorySeed(
                        CategoryCodes.Shopping.Accessories.Code,
                        "Accessories",
                        "Wearable or personal add-ons such as bags, wallets, watches, belts, and similar items."),

                    new SubCategorySeed(
                        CategoryCodes.Shopping.Gifts.Code,
                        "Gifts",
                        "Physical items bought to give to someone else for social occasions or personal giving."),

                    new SubCategorySeed(
                        CategoryCodes.Shopping.Other.Code,
                        "Other",
                        "Any shopping expense that does not fit the listed shopping subcategories.")
                    });
            }
        }
    }
