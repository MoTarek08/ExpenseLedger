using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class FoodCreator
    {
        public static CategorySeed Create()
        {
            return new CategorySeed(
                CategoryCodes.Food.Code,
                "Food & Dining",
                "Money spent on everyday food and drink, whether you are buying groceries for home, eating out, grabbing coffee, ordering delivery, or picking up small food items and snacks.",
                new List<SubCategorySeed>
                {
                    new SubCategorySeed(
                        CategoryCodes.Food.Groceries.Code,
                        "Groceries",
                        "Food and household edible items bought to prepare or consume at home, including supermarket and market purchases."),

                    new SubCategorySeed(
                        CategoryCodes.Food.Restaurants.Code,
                        "Restaurants",
                        "Meals, drinks, and dining experiences purchased in full-service restaurants or casual dine-in places."),

                    new SubCategorySeed(
                        CategoryCodes.Food.FastFood.Code,
                        "Fast Food",
                        "Quick-service meals from burger shops, fried chicken places, shawarma shops, and similar grab-and-go food outlets."),

                    new SubCategorySeed(
                        CategoryCodes.Food.CafesCoffee.Code,
                        "Cafés & Coffee",
                        "Coffee, tea, pastries, and light food bought from cafés, coffee shops, or similar hangout spots."),

                    new SubCategorySeed(
                        CategoryCodes.Food.FoodDelivery.Code,
                        "Food Delivery",
                        "Delivered meals, delivery fees, and app-based food orders brought to your home or office."),

                    new SubCategorySeed(
                        CategoryCodes.Food.Snacks.Code,
                        "Snacks",
                        "Small food purchases such as chips, chocolate, sweets, fruits, and other in-between meal items."),

                    new SubCategorySeed(
                        CategoryCodes.Food.Other.Code,
                        "Other",
                        "Any food or drink expense that does not fit cleanly into the other food subcategories.")
                });
        }
    }
    }
