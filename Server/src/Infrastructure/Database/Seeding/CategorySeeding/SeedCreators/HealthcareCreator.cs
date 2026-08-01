using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class HealthcareCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Healthcare.Code,
                    "Healthcare",
                    "Expenses related to physical or mental health, medical care, treatment, and health-related necessities.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Healthcare.Doctor.Code,
                        "Doctor",
                        "Consultations, visits, and appointments with general or specialist doctors."),

                    new SubCategorySeed(
                        CategoryCodes.Healthcare.Dentist.Code,
                        "Dentist",
                        "Dental visits, treatments, cleanings, procedures, and oral care services."),

                    new SubCategorySeed(
                        CategoryCodes.Healthcare.Pharmacy.Code,
                        "Pharmacy",
                        "Medicines, prescriptions, medical creams, and other pharmacy purchases."),

                    new SubCategorySeed(
                        CategoryCodes.Healthcare.MedicalTests.Code,
                        "Medical Tests",
                        "Lab work, scans, diagnostics, imaging, and any other medical testing."),

                    new SubCategorySeed(
                        CategoryCodes.Healthcare.Therapy.Code,
                        "Therapy",
                        "Physical therapy, counseling, psychological sessions, rehabilitation, and similar treatment."),

                    new SubCategorySeed(
                        CategoryCodes.Healthcare.Insurance.Code,
                        "Insurance",
                        "Health insurance premiums, renewals, and medical coverage payments."),

                    new SubCategorySeed(
                        CategoryCodes.Healthcare.Other.Code,
                        "Other",
                        "Any healthcare expense that does not fit the other healthcare subcategories.")
                    });
            }
        }
    }
