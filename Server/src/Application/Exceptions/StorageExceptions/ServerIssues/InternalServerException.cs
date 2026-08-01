using Application.ErrorNamespace.ErrorCodesNamespace;

namespace Application.Exceptions.StorageExceptions.ServerIssuesNamespace
{
    public class InternalServerException : Exception
    {
        public const string Title = "Internal server error";
        public const int Status = 500;

        public string ErrorCode { get; } = StorageErrorCodes.INTERNAL_SERVER_ERROR;
        public string Detail { get; } = "Internal error";
        public override string Message => Detail;

        public InternalServerException() { }
        protected InternalServerException(string detail, string errorCode)
        {
            Detail = detail;
            ErrorCode = errorCode;
        }
    }

}
