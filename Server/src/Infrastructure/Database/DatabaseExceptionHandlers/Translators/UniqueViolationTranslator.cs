using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;
using Infrastructure.Database.ConstraintsConstants;
using Npgsql;

namespace Infrastructure.Database.DatabaseExceptionHandlersNamespace.TranslatorsNamespace
{
    public static class UniqueViolationTranslator
    {
        public static void Translate(PostgresException ex)
        {
            switch (ex.ConstraintName)
            {
                case DatabaseConstraintsConstants.UniqueEmail:
                    throw new EmailAlreadyExists();

                case DatabaseConstraintsConstants.UserCategoryPreferencePrimaryKey:
                    throw new CategoryPreferenceAlreadyExists();

                case DatabaseConstraintsConstants.UniqueSpendingGoalUserCategoryPeriod:
                    throw new SpendingGoalAlreadyExists();

                case DatabaseConstraintsConstants.UniqueActiveRefreshTokenSessionId:
                    throw new SessionAlreadyHaveAnActiveRefreshToken();

                case DatabaseConstraintsConstants.UniqueExpenseSchedluedExpenseIdScheduledGenerationDate:
                    throw new GeneratedExpenseForThatDayAlreadyExists();

                case DatabaseConstraintsConstants.UniqueNotificationUserIdDeduplicationKey:
                    throw new NotificationDeuplicationKeyAlreadyExists();

                // COMMENTED OUT: object storage deletion requests are no longer used
                //case DatabaseConstraintsConstants.UniqueObjectStorageDeletionsRequestsObjectKey:
                //    throw new ObjectStorageDeletionRequestForThatObjecyKeyAlreadyExists();

                default:
                    throw new UniqueViolation();
            }
        }
    }
}
