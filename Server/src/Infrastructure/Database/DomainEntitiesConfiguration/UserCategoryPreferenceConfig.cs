using Domain.Entities.UserCategoryPreferenceNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class UserCategoryPreferenceConfig : IEntityTypeConfiguration<UserCategoryPreference>
    {
        public void Configure(EntityTypeBuilder<UserCategoryPreference> builder)
        {
            builder.ToTable("user_category_preferences");
            builder.HasKey(p => new { p.UserId, p.CategoryId });

            // Columns:
            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.PreferenceLevel)
                .HasColumnName("preference_level")
                .HasColumnType("int")
                .IsRequired();

            // Relationships:
            builder.HasOne(p => p.User)
                .WithMany(p => p.CategoryPreferences)
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK_user_category_preferences_users_user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(p => p.Category)
                .WithMany(p => p.UserCategoryPreferences)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_user_category_preferences_expense_categories_category_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            // Constraints:
        }
    }
}
