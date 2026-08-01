using Application.ErrorNamespace.ErrorCodesNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolationNamespace
{
    public class UniqueViolation : Exception
    {
        public const string Title = "Conflict";
        public const int Status = 409;

        public string ErrorCode { get; protected init; } = StorageErrorCodes.UNIQUE_VIOLATION;
        public string Detail { get; protected init; } = "Resource already exists";
        public override string Message => Detail;

        public UniqueViolation() { }

        protected UniqueViolation(string detail, string errorCode)
        {
            Detail = detail;
            ErrorCode = errorCode;
        }
    }
}
