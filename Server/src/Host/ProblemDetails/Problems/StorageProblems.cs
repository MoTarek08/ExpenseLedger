using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.ForeignKeyViolation;
using Application.Exceptions.StorageExceptions.ServerIssuesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetailsNamespace.ProblemsNamespace
{
    [ProblemDictionary]
    public static class StorageProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [StorageErrorCodes.INTERNAL_SERVER_ERROR] =
            new(
                InternalServerException.Title,
                "Internal error",
                StorageErrorCodes.INTERNAL_SERVER_ERROR,
                InternalServerException.Status),

            [StorageErrorCodes.BAD_DB_CONNECTION] =
            new(
                "Bad connection",
                "Bad connection",
                StorageErrorCodes.BAD_DB_CONNECTION,
                503),

            [StorageErrorCodes.REFERENCED_ENTITY_NOT_FOUND] =
            new(
                ReferencedEntityNotFound.TitleConst,
                "A referenced entity does not exist.",
                StorageErrorCodes.REFERENCED_ENTITY_NOT_FOUND,
                404),

        };
    }
}


