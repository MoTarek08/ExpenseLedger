using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.SpendingGoalNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class SpendingGoalConfig : IEntityTypeConfiguration<SpendingGoal>
    {
        public void Configure(EntityTypeBuilder<SpendingGoal> builder)
        {
            builder.ToTable("spending_goals");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasColumnType("text")
                .HasMaxLength(BusinessConstants.MaxDescriptionLength)
                .IsRequired(false);

            builder.Property(p => p.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(p => p.MaximumTargetAmount)
                .HasColumnName("maximum_target_amount")
                .HasColumnType("numeric")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.MinimumTargetAmount)
                .HasColumnName("minimum_target_amount")
                .HasColumnType("numeric")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.StartsAt)
                .HasColumnName("starts_at")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(p => p.EndsAt)
                .HasColumnName("ends_at")
                .HasColumnType("date")
                .IsRequired();

            // Relationships:
            builder.HasOne(p => p.User)
                .WithMany(p => p.SpendingGoals)
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK_spending_goals_users_user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(p => p.Category)
                .WithMany(p => p.SpendingGoals)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_spending_goals_expense_categories_category_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes:
            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_spending_goals_user_id");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_spending_goals_category_id");

            builder
                .HasIndex(x => new
                {
                    x.UserId,
                    x.CategoryId,
                    x.StartsAt,
                    x.EndsAt
                })
                .IsUnique()
                .HasDatabaseName("UQ_spending_goals_user_category_period");

            // Constraints:
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_spending_goals_bounds",
                "minimum_target_amount > 0 AND maximum_target_amount >= minimum_target_amount"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_spending_goals_ends_at",
                "ends_at >= starts_at"));
        }
    }
}
