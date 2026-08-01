using Domain.Entities.DomainEnums;

namespace Domain.Entities.Notification
{
    public sealed class Notification
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }

        public string DedupKey { get; private set; } = null!;

        public NotificationReason Reason { get; private set; }
        public NotificationType Type  { get; private set; }

        public Guid? ExpenseId { get; private set; }
        public Guid? SpendingGoalId { get; private set; }
        public Guid? ScheduledExpenseId { get; private set; }
        public Guid? CategoryId { get; private set; }

        public DateOnly? BudgetPeriodStart { get; private set; }

        public string Title { get; private set; } = null!;
        public string Body { get; private set; } = null!;

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? ReadAt { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }

        public static Notification BudgetWentNegative(
            Guid userId,
            Guid expenseId,
            decimal remaining,
            DateOnly budgetPeriodStart,
            DateTimeOffset createdAt)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpenseId = expenseId,
                DedupKey = NotificationDedupKeyBuilder.BudgetWentNegative(userId, budgetPeriodStart),
                Reason = NotificationReason.BudgetWentNegative,
                Type = NotificationType.CriticalIssue,
                Title = "Budget exceeded",
                Body = $"Your budget went negative by {Math.Abs(remaining):0.##}.",
                CreatedAt = createdAt
            };
        }


        public static Notification BudgentWentBelowQuarter(
            Guid userId,
            Guid expenseId,
            DateOnly budgetPeriodStart,
            DateTimeOffset createdAt)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpenseId = expenseId,
                DedupKey = NotificationDedupKeyBuilder.BudgetBelowQuarter(userId, budgetPeriodStart),
                Reason = NotificationReason.BudgetWentBelowQuarter,
                Type = NotificationType.Warning,
                Title = "Budget is low",
                Body = $"Your budget went below quarter",
                CreatedAt = createdAt
            };
        }

        public static Notification BudgentWentBelowTenPercent(
            Guid userId,
            Guid expenseId,
            DateOnly budgetPeriodStart,
            DateTimeOffset createdAt)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpenseId = expenseId,
                DedupKey = NotificationDedupKeyBuilder.BudgetBelowTenPercent(userId, budgetPeriodStart),
                Reason = NotificationReason.BudgetWentBelowTenPercent,
                Type = NotificationType.Warning,
                Title = "Budget is very low",
                Body = $"Your budget went below 10%",
                CreatedAt = createdAt
            };
        }

        public static Notification SpendingOnAvoidPreference(
            Guid userId,
            Guid expenseId,
            Guid categoryId,
            string categoryName,
            DateOnly budgetPeriodStart,
            DateTimeOffset createdAt)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpenseId = expenseId,
                CategoryId = categoryId,
                DedupKey = NotificationDedupKeyBuilder.SpendingOnAvoidPreference(
                    userId,
                    categoryId,
                    budgetPeriodStart),
                Reason = NotificationReason.SpendingOnAvoidPreference,
                Type = NotificationType.Warning,
                Title = "Avoided category spent on",
                Body = $"You spent on {categoryName}, which you marked as avoid.",
                BudgetPeriodStart = budgetPeriodStart,
                CreatedAt = createdAt
            };
        }

        public static Notification GoalAchieved(
            Guid spendingGoalId,
            Guid userId,
            Guid? categoryId,
            DateOnly goalStartDate,
            DateOnly goalEndDate,
            DateTimeOffset createdAt)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SpendingGoalId = spendingGoalId,
                DedupKey = NotificationDedupKeyBuilder.GoalAchieved(spendingGoalId, goalStartDate, goalEndDate),
                Reason = NotificationReason.GoalAchieved,
                Type = NotificationType.Achievement,
                Title = "Spending goal achieved!",
                Body = "Your spending goal's current state is achieved! maintain your spending until the goal period ends to achieve it officially",
                CategoryId = categoryId,
                CreatedAt = createdAt
            };
        }

        public static Notification ScheduledExpenseProcessed(
            Guid userId,
            Guid expenseId,
            Guid scheduledExpenseId,
            string? expenseTitle,
            DateOnly generatedOn,
            DateTimeOffset createdAt)
        {

            var title = expenseTitle is not null ? $"Scheduled expense \"{expenseTitle}\" has been generated." :
                "Scheduled expense has been generated.";

            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpenseId = expenseId,
                ScheduledExpenseId = scheduledExpenseId,
                DedupKey = NotificationDedupKeyBuilder.ScheduledExpenseProcessed(scheduledExpenseId, generatedOn),
                Reason = NotificationReason.ScheduledExpenseProcessed,
                Type = NotificationType.Information,
                Title = "Scheduled expense generated",
                Body = $"Scheduled expense {title} has been generated.",
                CreatedAt = createdAt
            };
        }

        public static Notification SpendingOnMinimalPreference(
            Guid userId,
            Guid expenseId,
            Guid categoryId,
            string categoryName,
            decimal spentThisPeriod,
            DateOnly budgetPeriodStart,
            DateTimeOffset createdAt)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpenseId = expenseId,
                CategoryId = categoryId,
                DedupKey = NotificationDedupKeyBuilder.SpendingOnMinimalPreference(
                    userId,
                    categoryId,
                    budgetPeriodStart),
                Reason = NotificationReason.SpendingOnMinimalPreference,
                Type = NotificationType.Warning,
                Title = "Category spending warning",
                Body = $"You have spent {spentThisPeriod} on {categoryName} this month, which you want to minimalize spending on",
                BudgetPeriodStart = budgetPeriodStart,
                CreatedAt = createdAt
            };
        }

        public Notification MarkAsRead(DateTimeOffset readAt)
        {
            if (ReadAt is null)
                ReadAt = readAt;

            return this;
        }

        public Notification MarkAsDeleted(DateTimeOffset deletedAt)
        {
            if (DeletedAt is null)
                DeletedAt = deletedAt;

            return this;
        }

        public Notification Undelete()
        {
            DeletedAt = null;
            return this;
        }
    }
}
