using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetailsNamespace.ProblemsNamespace
{
    [ProblemDictionary]
    public static class UsersProblems
    {
        public static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [UsersErrorCodes.FINANCIAL_PROFILE_ALREADY_EXISTS] =
                new(
                    "Conflict",
                    "A financial profile already exists for this account.",
                    UsersErrorCodes.FINANCIAL_PROFILE_ALREADY_EXISTS,
                    409),

            [UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND] =
                new(
                    "Financial profile not found",
                    "The financial profile was not found.",
                    UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND,
                    404),

            [UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND] =
                new(
                    "User not found",
                    "The current session user was not found.",
                    UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND,
                    500)
        };
    }
}
