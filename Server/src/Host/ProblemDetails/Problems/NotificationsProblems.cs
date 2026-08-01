using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetails.Problems
{
    [ProblemDictionary]
    public static class NotificationsProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [NotificationsErrorCodes.NOTIFICATION_NOT_FOUND] =
            new(
                "Not Found",
                "The requested notification could not be found.",
                NotificationsErrorCodes.NOTIFICATION_NOT_FOUND,
                404)

        };

    }
}
