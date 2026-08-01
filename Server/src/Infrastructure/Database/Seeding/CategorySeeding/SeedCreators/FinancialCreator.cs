using Domain.BusinessInvariants.CategoryCodesNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeedRecords;

namespace Infrastructure.Database.Seeding.CategorySeeding.SeedCreatorsNamespace
{
    public static class FinancialCreator
        {
            public static CategorySeed Create()
            {
                return new CategorySeed(
                    CategoryCodes.Financial.Code,
                    "Financial",
                    "Money movement and financial obligations such as debt repayment, bank charges, and savings transfers.",
                    new List<SubCategorySeed>
                    {
                    new SubCategorySeed(
                        CategoryCodes.Financial.LoanPayment.Code,
                        "Loan Payment",
                        "Payments made toward personal or consumer loans."),

                    new SubCategorySeed(
                        CategoryCodes.Financial.CreditCardPayment.Code,
                        "Credit Card Payment",
                        "Payments made to settle or reduce credit card balances."),

                    new SubCategorySeed(
                        CategoryCodes.Financial.BankFees.Code,
                        "Bank Fees",
                        "Charges, service fees, transfer fees, and other bank-related costs."),

                    new SubCategorySeed(
                        CategoryCodes.Financial.SavingsTransfer.Code,
                        "Savings Transfer",
                        "Money moved into savings, reserves, or other planned financial holding accounts."),

                    new SubCategorySeed(
                        CategoryCodes.Financial.Other.Code,
                        "Other",
                        "Any financial expense that does not fit the listed financial subcategories.")
                    });
            }
        }
    }
