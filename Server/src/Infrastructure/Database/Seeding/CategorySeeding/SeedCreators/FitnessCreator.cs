using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class FitnessCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Fitness.Code,
                    "Fitness",
                    "Spending related to training, exercise, body maintenance, and gym-oriented self-improvement.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Fitness.GymMembership.Code,
                        "Gym Membership",
                        "Monthly, yearly, or one-time payments to access a gym or fitness center."),

                    new SubCategorySeed(
                        CategoryCodes.Fitness.GymEquipment.Code,
                        "Gym Equipment",
                        "Fitness machines, weights, bands, mats, and workout equipment for exercise."),

                    new SubCategorySeed(
                        CategoryCodes.Fitness.PersonalTrainer.Code,
                        "Personal Trainer",
                        "Paid coaching, training sessions, or personal fitness guidance."),

                    new SubCategorySeed(
                        CategoryCodes.Fitness.Classes.Code,
                        "Classes",
                        "Workout classes, fitness lessons, group training, and similar structured exercise sessions."),

                    new SubCategorySeed(
                        CategoryCodes.Fitness.Supplements.Code,
                        "Supplements",
                        "Nutrition supplements and fitness support products used for training or body goals."),

                    new SubCategorySeed(
                        CategoryCodes.Fitness.FitnessApparel.Code,
                        "Fitness Apparel",
                        "Clothing and wearable items bought mainly for working out or fitness use."),

                    new SubCategorySeed(
                        CategoryCodes.Fitness.FitnessAccessories.Code,
                        "Fitness Accessories",
                        "Small accessories used for training, exercise, or gym routines."),

                    new SubCategorySeed(
                        CategoryCodes.Fitness.Other.Code,
                        "Other",
                        "Any fitness expense that does not fit the listed fitness subcategories.")
                    });
            }
        }
    }
