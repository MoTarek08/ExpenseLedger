using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetails.Problems
{
    [ProblemDictionary]
    public static class NotesProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [NotesErrorCodes.NOTE_EXPENSE_NOT_FOUND] =
                new(
                    "Not Found",
                    "The specefied expense does not exist",
                    NotesErrorCodes.NOTE_EXPENSE_NOT_FOUND,
                    404),

            [NotesErrorCodes.NOTE_NOT_FOUND] =
            new(
                "Not Found",
                "The requested note could not be found.",
                NotesErrorCodes.NOTE_NOT_FOUND,
                404),

        };
    }
}
