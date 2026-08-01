using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolation
{
    public class SessionAlreadyHaveAnActiveRefreshToken : UniqueViolationNamespace.UniqueViolation
    {
        public SessionAlreadyHaveAnActiveRefreshToken() : base
            ("You are already authorized",
            AuthErrorCodes.AUTH_REFRESH_TOKEN_ACTIVE_SESSION_ID_ALREADY_EXISTS) { }
    }
}
