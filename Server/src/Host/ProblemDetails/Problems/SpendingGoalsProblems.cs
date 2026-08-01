using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.CheckViolationNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetails.Problems
{
    [ProblemDictionary]
    public static class SpendingGoalsProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND] =
                new(
                "Not Found",
                "The requested spending goal could not be found.",
                SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND,
                404),

            [SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS] =
                new(
                UniqueViolation.Title,
                "A spending goal for the specified period already exists",
                SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS,
                UniqueViolation.Status),


            [SpendingGoalsErrorCodes.SPENDING_GOAL_LONG_PERIOD_GAP] = 
                new(
                "Bad Request",
                "Gap between start date and end date cannot be more than a year",
                SpendingGoalsErrorCodes.SPENDING_GOAL_LONG_PERIOD_GAP,
                400
                ),

            [SpendingGoalsErrorCodes.SPENDING_GOAL_COMPLETED] =
                new(
                "Bad Request",
                "Completed spending goals cannot be modified",
                SpendingGoalsErrorCodes.SPENDING_GOAL_COMPLETED,
                400
                ),

            [SpendingGoalsErrorCodes.SPENDING_GOAL_BOUNDS_VIOLATION] =
                new(
                CheckViolation.Title,
                "Spending goal bounds constraint violated",
                SpendingGoalsErrorCodes.SPENDING_GOAL_BOUNDS_VIOLATION,
                CheckViolation.Status
                ),

        };
    }
}
