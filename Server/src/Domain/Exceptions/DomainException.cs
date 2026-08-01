namespace Domain.ExceptionsNamespace
{
    public class DomainException : Exception
    {
        public string ErrorCode { get; } = "DOMAIN_ERROR";

        public DomainException(string? message) : base(message)
        {
        }

    }
}
