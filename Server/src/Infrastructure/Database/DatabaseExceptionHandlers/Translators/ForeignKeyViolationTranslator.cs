using Application.Exceptions.StorageExceptions.ForeignKeyViolation;
using Npgsql;

namespace Infrastructure.Database.DatabaseExceptionHandlers.Translators
{
    public static class ForeignKeyViolationTranslator
    {
        public static void Translate(PostgresException ex)
        {
            switch (ex.ConstraintName)
            {
                case "FK_expenses_expense_categories_category_id":
                case "FK_expense_sub_categories_expense_categories_category_id":
                case "FK_scheduled_expenses_expense_categories_category_id":
                case "FK_spending_goals_expense_categories_category_id":
                case "FK_user_category_preferences_expense_categories_category_id":
                case "FK_notifications_expense_categories_category_id":
                    throw new ReferencedEntityNotFound("Category");

                case "FK_expenses_expense_sub_categories_sub_category_id":
                case "FK_scheduled_expenses_expense_sub_categories_sub_category_id":
                    throw new ReferencedEntityNotFound("Sub-category");

                case "FK_expenses_file_objects_expenses_expense_id":
                case "FK_notes_expenses_expense_id":
                case "FK_notifications_expenses_expense_id":
                    throw new ReferencedEntityNotFound("Expense");

                case "FK_expenses_scheduled_expenses_scheduled_expense_id":
                case "FK_notifications_scheduled_expenses_scheduled_expense_id":
                    throw new ReferencedEntityNotFound("Scheduled expense");

                case "FK_notifications_spending_goals_spending_goal_id":
                    throw new ReferencedEntityNotFound("Spending goal");

                case "FK_expenses_users_user_id":
                case "FK_expenses_file_objects_users_user_id":
                case "FK_notifications_users_user_id":
                case "FK_refresh_tokens_users_user_id":
                case "FK_scheduled_expenses_users_user_id":
                case "FK_spending_goals_users_user_id":
                case "FK_user_category_preferences_users_user_id":
                case "FK_users_financial_profiles_users_user_id":
                    throw new ReferencedEntityNotFound("User");

                default:
                    throw new ForeginKeyViolation();
            }
        }
    }
}
