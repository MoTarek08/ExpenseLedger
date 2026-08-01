namespace Domain.Entities.DomainEnums
{
    public enum NotificationReason
    {
        BudgetWentNegative = 1,
        BudgetWentBelowQuarter = 2,
        BudgetWentBelowTenPercent = 3,
        SpendingOnAvoidPreference = 4,
        SpendingOnMinimalPreference = 5,
        GoalAchieved = 6,
        ScheduledExpenseProcessed = 7
    }
}