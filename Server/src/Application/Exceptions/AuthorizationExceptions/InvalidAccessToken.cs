using Application.ErrorNamespace.ErrorCodesNamespace;

namespace Application.Exceptions.AuthorizationExceptions
{
    public class InvalidAccessToken : Exception
    {
        public const string Title = "Unauthorized";
        public const int Status = 401;

        public string ErrorCode { get; } = AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN;
        public string Detail { get; } = "Invalid authorization.";
        public override string Message => Detail;

        public InvalidAccessToken() { }
        protected InvalidAccessToken(string detail, string errorCode)
        {
            Detail = detail;
            ErrorCode = errorCode;
        }
    }
}
