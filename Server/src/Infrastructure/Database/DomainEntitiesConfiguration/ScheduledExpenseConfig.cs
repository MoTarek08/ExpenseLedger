using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class ScheduledExpenseConfig : IEntityTypeConfiguration<ScheduledExpense>
    {
        public void Configure(EntityTypeBuilder<ScheduledExpense> builder)
        {
            builder.ToTable("scheduled_expenses");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.Title)
                .HasColumnName("title")
                .HasColumnType("text")
                .HasMaxLength(BusinessConstants.MaxTitleLength)
                .IsRequired(false);

            builder.Property(p => p.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.SubCategoryId)
                .HasColumnName("sub_category_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(p => p.Cadence)
                .HasColumnName("cadence")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(p => p.FirstDueOn)
                .HasColumnName("first_due_on")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(p => p.NextDueOn)
                .HasColumnName("next_due_on")
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(p => p.LastProcessedAt)
                .HasColumnName("last_processed_at")
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

            builder.Property(p => p.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean")
                .IsRequired();

            // Relationships:
            builder.HasOne(p => p.User)
                .WithMany(p => p.ScheduledExpenses)
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK_scheduled_expenses_users_user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(p => p.Category)
                .WithMany(p => p.ScheduledExpenses)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_scheduled_expenses_expense_categories_category_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(p => p.SubCategory)
                .WithMany(p => p.ScheduledExpenses)
                .HasForeignKey(p => p.SubCategoryId)
                .HasConstraintName("FK_scheduled_expenses_expense_sub_categories_sub_category_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);


            // Indexes:
            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_scheduled_expenses_user_id");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_scheduled_expenses_category_id");

            builder.HasIndex(p => p.Cadence)
                .HasDatabaseName("IX_scheduled_expenses_cadence");

            // Constraints:
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_scheduled_expenses_amount",
                "amount > 0"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_scheduled_expenses_next_due_on",
                "next_due_on IS NULL OR next_due_on >= first_due_on"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_scheduled_expenses_last_processed_at",
                "last_processed_at IS NULL OR last_processed_at >= first_due_on"));

            builder.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_scheduled_expenses_active_next_due_on",
                    "is_active = TRUE OR next_due_on IS NULL"));
        }
    }
}
