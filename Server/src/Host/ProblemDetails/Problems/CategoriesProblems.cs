using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetails.Problems
{
    [ProblemDictionary]
    public static class CategoriesProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [CategoriesErrorCodes.CATEGORY_NOT_FOUND] =
            new(
                "Category not found",
                "The specified category code does not exist.",
                CategoriesErrorCodes.CATEGORY_NOT_FOUND,
                404),

            [CategoriesErrorCodes.SUB_CATEGORY_NOT_FOUND] =
            new(
                "Sub-category not found",
                "The specified sub-category code does not exist.",
                CategoriesErrorCodes.SUB_CATEGORY_NOT_FOUND,
                404),
        };
    }
}
