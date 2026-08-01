using Infrastructure.Database.DatabaseExceptionHandlers.Translators;
using Infrastructure.Database.DatabaseExceptionHandlersNamespace.TranslatorsNamespace;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Database.DatabaseExceptionHandlersNamespace
{
    public static class HandlingDatabaseExceptionsService
    {
        public static void Handle(DbUpdateException ex)
        {
            if(ex.InnerException is PostgresException pgEx)
            {
                switch (pgEx.SqlState) 
                {
                    case PostgresErrorCodes.UniqueViolation:
                        UniqueViolationTranslator.Translate(pgEx);
                        break;

                    case PostgresErrorCodes.ForeignKeyViolation:
                        ForeignKeyViolationTranslator.Translate(pgEx);
                        break;

                    case PostgresErrorCodes.CheckViolation:
                        CheckViolationTranslator.Translate(pgEx);
                        break;
                }
                throw pgEx;
            }
            throw ex;
        }
    }
}
