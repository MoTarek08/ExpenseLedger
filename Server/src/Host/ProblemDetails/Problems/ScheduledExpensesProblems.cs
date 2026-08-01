using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetails.Problems
{
    [ProblemDictionary]
    public static class ScheduledExpensesProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND] =
            new(
                "Not Found",
                "The requested scheduled expense could not be found.",
                ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND,
                404),

            [UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND] =
            new(
                "Financial profile not found",
                "Please create a financial profile to start tracking scheduled expenses.",
                UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND,
                404),
        };
    }
}