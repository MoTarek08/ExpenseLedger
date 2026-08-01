using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetails.Problems
{
    [ProblemDictionary]
    public static class CategoryPreferenceProblems
    {
        public static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND] =
                new(
                    "Category Not Found",
                    "The selected category does not exist.",
                    CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND,
                    404),

            [CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND] =
                new(
                    "Preference Not Found",
                    "No preference was found for this category.",
                    CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND,
                    404),

            [CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_ALREADY_EXISTS] =
                new(
                    "Conflict",
                    "A category preference already exists for this category.",
                    CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_ALREADY_EXISTS,
                    409),
        };
    }
}