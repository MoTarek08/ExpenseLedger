using Application.ErrorNamespace.ErrorCodesNamespace;

namespace Application.Exceptions.StorageExceptions.ForeignKeyViolation
{
    public class ReferencedEntityNotFound : ForeginKeyViolation
    {
        public new const string TitleConst = "NotFound";
        public new const int StatusConst = 404;
        public override int Status => StatusConst;
        public override string Title => "Not Found";

        public ReferencedEntityNotFound(string entityName)
            : base($"The referenced {entityName} was not found.", StorageErrorCodes.REFERENCED_ENTITY_NOT_FOUND)
        { }
    }
}
