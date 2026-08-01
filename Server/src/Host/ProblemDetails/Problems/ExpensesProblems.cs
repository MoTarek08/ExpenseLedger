using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetails.Problems
{
    [ProblemDictionary]
    public static class ExpensesProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [ExpensesErrorCodes.EXPENSE_CATEGORY_NOT_FOUND] =
            new(
                "Category does not exist",
                "Category does not exist",
                ExpensesErrorCodes.EXPENSE_CATEGORY_NOT_FOUND,
                400),


            [ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER] =
            new(
                "Categories do not belong to each other",
                "Categories do not belong to each other,",
                ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER,
                400),


            [ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND] =
            new(
                "Scheduled expense not found",
                "The scheduled expense was not found.",
                ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND,
                404),


            [ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_ACTIVE] =
            new(
                "Scheduled expense is not active",
                "The scheduled expense is not active and cannot be updated.",
                ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_ACTIVE,
                400),


            [ExpensesErrorCodes.SCHEDULED_EXPENSE_PROCESSED_BEFORE_AND_CANNOT_CHANGE_FIRST_DUE] =
            new(
                "Scheduled expense has already been processed",
                "Cannot change first due date for already processed & active expenses",
                ExpensesErrorCodes.SCHEDULED_EXPENSE_PROCESSED_BEFORE_AND_CANNOT_CHANGE_FIRST_DUE,
                400),


            [ExpensesErrorCodes.EXPENSE_NOT_FOUND] =
            new(
                "Expense not found",
                "The expense was not found.",
                ExpensesErrorCodes.EXPENSE_NOT_FOUND,
                404),

            [ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND] =
            new(
                "File not found",
                "The file was not found.",
                ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND,
                404),

            [ExpensesErrorCodes.EXPENSE_FILE_WAS_DELETED] =
            new(
                "File not found",
                "The file was not found.",
                ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND,
                409),

            [ExpensesErrorCodes.EXPENSE_FILE_ALREADY_LINKED_TO_OTHER_EXPENSE] =
            new(
                "File already linked",
                "This file is already linked to another expense.",
                ExpensesErrorCodes.EXPENSE_FILE_ALREADY_LINKED_TO_OTHER_EXPENSE,
                409),

            [ExpensesErrorCodes.EXPENSE_INVALID_FILE_STATE] =
            new(
                "Invalid file state",
                "The file is not in a valid state to be confirmed.",
                ExpensesErrorCodes.EXPENSE_INVALID_FILE_STATE,
                409),

            [ExpensesErrorCodes.EXPENSE_FILE_NOT_UPLOADED_YET] =
            new(
                "File not uploaded",
                "The file has not been uploaded to storage yet.",
                ExpensesErrorCodes.EXPENSE_FILE_NOT_UPLOADED_YET,
                422),

            [UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND] =
            new(
                "Financial profile not found",
                "Please create a financial profile to start tracking expenses.",
                UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND,
                404),
        };
    }
}
