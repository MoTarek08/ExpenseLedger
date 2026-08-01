using Application.Exceptions.StorageExceptions.CheckViolation;
using Application.Exceptions.StorageExceptions.CheckViolationNamespace;
using Infrastructure.Database.ConstraintsConstants;
using Npgsql;

namespace Infrastructure.Database.DatabaseExceptionHandlers.Translators
{
    public static class CheckViolationTranslator
    {
        public static void Translate(PostgresException ex)
        {
            switch (ex.ConstraintName)
            {
                case DatabaseConstraintsConstants.CkSpendingGoalsBounds:
                    throw new SpendingGoalBoundsViolation();

                default:
                    throw new CheckViolation();
            }
        }
    }
}
