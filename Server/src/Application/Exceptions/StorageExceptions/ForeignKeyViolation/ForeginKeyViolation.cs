using Application.ErrorNamespace.ErrorCodesNamespace;

namespace Application.Exceptions.StorageExceptions.ForeignKeyViolation
{
    public class ForeginKeyViolation : Exception
    {
        public const string TitleConst = "Conflict";
        public const int StatusConst = 409;

        public virtual string Title => TitleConst;
        public virtual int Status => StatusConst;

        public string ErrorCode { get; protected init; } = StorageErrorCodes.FOREGIN_KEY_VIOLATION;
        public string Detail { get; protected init; } = "Foregin Key Violated";
        public override string Message => Detail;

        public ForeginKeyViolation() { }

        protected ForeginKeyViolation(string detail, string errorCode)
        {
            Detail = detail;
            ErrorCode = errorCode;
        }
    }
}
