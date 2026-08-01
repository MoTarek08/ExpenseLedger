namespace Domain.Entities.Notification
{
    public static class NotificationDedupKeyBuilder
    {
        public static string BudgetWentNegative(Guid userId, DateOnly periodStart)
            => $"budget-negative:{userId}:{periodStart:yyyy-MM-dd}";

        public static string BudgetBelowQuarter(Guid userId, DateOnly periodStart)
            => $"budget-below-quarter:{userId}:{periodStart:yyyy-MM-dd}";

        public static string BudgetBelowTenPercent(Guid userId, DateOnly periodStart)
            => $"budget-below-ten-percent:{userId}:{periodStart:yyyy-MM-dd}";

        public static string SpendingOnAvoidPreference(Guid userId, Guid categoryId, DateOnly budgetPeriodStart)
            => $"preference-avoid:{userId}:{categoryId}:{budgetPeriodStart:yyyy-MM-dd}";

        public static string SpendingOnMinimalPreference(Guid userId, Guid categoryId, DateOnly budgetPeriodStart)
            => $"preference-minimal:{userId}:{categoryId}:{budgetPeriodStart:yyyy-MM-dd}";

        public static string GoalAchieved(Guid goalId, DateOnly startDate, DateOnly endDate)
            => $"goal-achieved:{goalId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";

        public static string ScheduledExpenseProcessed(Guid scheduledExpenseId, DateOnly processedAt)
            => $"scheduled-processed:{scheduledExpenseId}:{processedAt:yyyy-MM-dd}";
    }
}
