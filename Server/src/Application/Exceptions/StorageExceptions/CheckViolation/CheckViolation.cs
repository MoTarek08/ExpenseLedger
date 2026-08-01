using Application.ErrorNamespace.ErrorCodesNamespace;

namespace Application.Exceptions.StorageExceptions.CheckViolationNamespace
{
    public class CheckViolation : Exception
    {
        public const string Title = "Bad Request";
        public const int Status = 400;

        public string ErrorCode { get; protected init; } = StorageErrorCodes.CHECK_VIOLATION;
        public string Detail { get; protected init; } = "A check constraint was violated";
        public override string Message => Detail;

        public CheckViolation() { }

        protected CheckViolation(string detail, string errorCode)
        {
            Detail = detail;
            ErrorCode = errorCode;
        }
    }
}
