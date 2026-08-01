using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.AuthorizationExceptions;
using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;

namespace Host.ProblemDetailsNamespace.ProblemsNamespace
{
    [ProblemDictionary]
    public static class AuthProblems
    {
        private static readonly IReadOnlyDictionary<string, ProblemDefinition> All = new Dictionary<string, ProblemDefinition>()
        {
            [AuthErrorCodes.AUTH_INVALID_AUTHORIZATION_HEADER] =
            new(
                "Unauthorized",
                "Invalid authorization",
                AuthErrorCodes.AUTH_INVALID_AUTHORIZATION_HEADER,
                401),

            [AuthErrorCodes.AUTH_REFRESH_TOKEN_MISSING] = 
            new(
                "Refresh token is missing",
                "Invalid authorization",
                AuthErrorCodes.AUTH_REFRESH_TOKEN_MISSING,
                401),

            [AuthErrorCodes.AUTH_INVALID_CREDENTIALS] =
            new(
                "Unauthorized",
                "Invalid email or password.",
                AuthErrorCodes.AUTH_INVALID_CREDENTIALS,
                401),

            [AuthErrorCodes.AUTH_EMAIL_ALREADY_EXISTS] =
            new(
                "Conflict",
                "An account with this email already exists.",
                AuthErrorCodes.AUTH_EMAIL_ALREADY_EXISTS,
                409),

            [AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN] = 
            new(
                InvalidAccessToken.Title,
                "Invalid authorization.",
                AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN,
                InvalidAccessToken.Status),

            [AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST] =
             new(
                 "Unauthorized", 
                 "Invalid authorization.",
                 AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST,
                 401), 
            
            [AuthErrorCodes.AUTH_REFRESH_TOKEN_EXPIRED] =
            new(
                "Unauthorized", 
                "Session timed out",
                AuthErrorCodes.AUTH_REFRESH_TOKEN_EXPIRED,
                401), 

            [AuthErrorCodes.AUTH_REVOKED_REFRESH_TOKEN] = 
            new(
                "Unauthorized", 
                "Invalid authorization",
                AuthErrorCodes.AUTH_REVOKED_REFRESH_TOKEN,
                401),
            
            [AuthErrorCodes.AUTH_TOKENS_PAYLOAD_MISMATCH] =
            new(
                "Unauthorized", 
                "Invalid authorization",
                AuthErrorCodes.AUTH_TOKENS_PAYLOAD_MISMATCH,
                401)
        };
    }
}


