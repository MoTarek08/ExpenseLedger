namespace Host.ProblemDetailsNamespace.ProblemDefinitionNamespace
{
    public sealed record ProblemDefinition(
        string Title,
        string Detail,
        string ErrorCode,
        int StatusCode);
}
