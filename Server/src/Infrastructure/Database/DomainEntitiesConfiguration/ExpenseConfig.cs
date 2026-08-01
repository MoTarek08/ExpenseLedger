using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class ExpenseConfig : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("expenses");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.SubCategoryId)
                .HasColumnName("sub_category_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(p => p.ScheduledExpenseId)
                .HasColumnName("scheduled_expense_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(p => p.ScheduledGenerationDate)
                .HasColumnName("scheduled_generation_date")
                .HasColumnType("date")
                .IsRequired(false);

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

            builder.Property(p => p.SpentOn)
                .HasColumnName("spent_on")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Relationships:
            builder.HasOne(p => p.User)
                .WithMany(p => p.Expenses)
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK_expenses_users_user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(p => p.Category)
                .WithMany(p => p.Expenses)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_expenses_expense_categories_category_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(p => p.SubCategory)
                .WithMany(p => p.Expenses)
                .HasForeignKey(p => p.SubCategoryId)
                .HasConstraintName("FK_expenses_expense_sub_categories_sub_category_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(p => p.ScheduledExpense)
                .WithMany(p => p.GeneratedExpenses)
                .HasForeignKey(p => p.ScheduledExpenseId)
                .HasConstraintName("FK_expenses_scheduled_expenses_scheduled_expense_id")
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes:

            builder.HasIndex(p => new { p.UserId, p.SpentOn })
                .HasDatabaseName("IX_expenses_user_id_spent_on");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_expenses_category_id");

            builder.HasIndex(p => p.SubCategoryId)
                .HasDatabaseName("IX_expenses_sub_category_id");

            builder.HasIndex(p => new { p.ScheduledExpenseId, p.ScheduledGenerationDate })
                .IsUnique()
                .HasFilter("scheduled_expense_id IS NOT NULL")
                .HasDatabaseName("UQ_expenses_scheduled_expense_id_scheduled_generation_date");


            // Constraints:
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_expenses_amount",
                "amount > 0"));

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_expenses_scheduled_generation_date_required",
                    "scheduled_expense_id IS NULL OR scheduled_generation_date IS NOT NULL");
            });

        }
    }
}
