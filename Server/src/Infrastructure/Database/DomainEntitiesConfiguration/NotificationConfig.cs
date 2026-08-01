using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.Notification;
using Domain.Entities.ScheduledExpenseNamespace;
using Domain.Entities.SpendingGoalNamespace;
using Domain.Entities.UserNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public sealed class NotificationConfig : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.DedupKey)
                .HasColumnName("dedup_key")
                .HasColumnType("text")
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Reason)
                .HasColumnName("reason")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.ExpenseId)
                .HasColumnName("expense_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(x => x.SpendingGoalId)
                .HasColumnName("spending_goal_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(x => x.ScheduledExpenseId)
                .HasColumnName("scheduled_expense_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(x => x.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(x => x.BudgetPeriodStart)
                .HasColumnName("budget_period_start")
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasColumnType("text")
                .IsRequired()
                .HasMaxLength(BusinessConstants.MaxNotificationTitleLength);

            builder.Property(x => x.Body)
                .HasColumnName("body")
                .HasColumnType("text")
                .IsRequired()
                .HasMaxLength(BusinessConstants.MaxNotificationBodyLength);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(x => x.ReadAt)
                .HasColumnName("read_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            // Relationships
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK_notifications_users_user_id")
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(true);

            builder.HasOne<Expense>()
                .WithMany()
                .HasForeignKey(p => p.ExpenseId)
                .HasConstraintName("FK_notifications_expenses_expense_id")
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne<SpendingGoal>()
                .WithMany()
                .HasForeignKey(p => p.SpendingGoalId)
                .HasConstraintName("FK_notifications_spending_goals_spending_goal_id")
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne<ScheduledExpense>()
                .WithMany()
                .HasForeignKey(p => p.ScheduledExpenseId)
                .HasConstraintName("FK_notifications_scheduled_expenses_scheduled_expense_id")
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne<ExpenseCategory>()
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_notifications_expense_categories_category_id")
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(x => new { x.UserId, x.DedupKey })
                .IsUnique()
                .HasDatabaseName("UQ_notifications_user_id_deduplication_key");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_notifications_user_id");

            builder.HasIndex(x => new { x.UserId, x.ReadAt })
                .HasDatabaseName("IX_notifications_user_id_read_at");

            builder.HasIndex(x => new { x.UserId, x.DeletedAt, x.CreatedAt })
                .HasDatabaseName("IX_notifications_user_id_deleted_at_created_at");

            builder.HasIndex(x => x.ExpenseId)
                .HasDatabaseName("IX_notifications_expense_id");

            builder.HasIndex(x => x.SpendingGoalId)
                .HasDatabaseName("IX_notifications_spending_goal_id");

            builder.HasIndex(x => x.ScheduledExpenseId)
                .HasDatabaseName("IX_notifications_scheduled_expense_id");

            builder.HasIndex(x => x.CategoryId)
                .HasDatabaseName("IX_notifications_category_id");
        }
    }
}
