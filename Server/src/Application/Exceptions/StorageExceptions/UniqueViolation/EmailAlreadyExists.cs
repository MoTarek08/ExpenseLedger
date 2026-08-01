using Application.ErrorNamespace.ErrorCodesNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolationNamespace
{
    public class EmailAlreadyExists : UniqueViolation
    {
        public EmailAlreadyExists() : base("Invalid account setup",AuthErrorCodes.AUTH_EMAIL_ALREADY_EXISTS) { }
    }
}
