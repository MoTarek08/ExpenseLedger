namespace Infrastructure.Database.ConstraintsConstants
{
    public static class DatabaseConstraintsConstants
    {
        public const string UniqueEmail = "UQ_user_email";

        // Enforces a unique {user_id, category_id} compination 
        public const string UserCategoryPreferencePrimaryKey = "PK_user_category_preference";

        public const string UniqueSpendingGoalUserCategoryPeriod = "UQ_spending_goal_user_category_period";

        public const string UniqueActiveRefreshTokenSessionId = "UQ_active_refresh_token_session_id";

        public const string UniqueExpenseSchedluedExpenseIdScheduledGenerationDate = "UQ_expenses_scheduled_expense_id_scheduled_generation_date";

        public const string UniqueNotificationUserIdDeduplicationKey = "UQ_notifications_user_id_deduplication_key";

        // COMMENTED OUT: object storage deletion requests are no longer used
        //public const string UniqueObjectStorageDeletionsRequestsObjectKey = "UQ_object_storage_deletion_requests_object_key";

        public const string CkSpendingGoalsBounds = "CK_spending_goals_bounds";
    }
}
