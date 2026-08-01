using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetailsNamespace.ProblemsNamespace
{
    [ProblemDictionary]
    public static class OtherProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [OtherErrorCodes.VALIDATION_ERROR] =
            new(
                "Validation Error(s)",
                "Invalid data input",
                OtherErrorCodes.VALIDATION_ERROR,
                400),

            [OtherErrorCodes.INVALID_JSON_FORMAT] =
            new(
                "Invalid JSON Format",
                "The JSON body provided is malformed",
                OtherErrorCodes.INVALID_JSON_FORMAT,
                400)
        };
    }
}
