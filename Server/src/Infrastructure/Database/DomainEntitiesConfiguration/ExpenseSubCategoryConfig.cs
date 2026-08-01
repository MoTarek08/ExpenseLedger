using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class ExpenseSubCategoryConfig : IEntityTypeConfiguration<ExpenseSubCategory>
    {
        public void Configure(EntityTypeBuilder<ExpenseSubCategory> builder)
        {
            builder.ToTable("expense_sub_categories");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.Code)
                .HasColumnName("code")
                .HasColumnType("text")
                .HasMaxLength(BusinessConstants.MaxCategoryNameLength)
                .IsRequired();

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasColumnType("text")
                .HasMaxLength(BusinessConstants.MaxCategoryNameLength)
                .IsRequired();

            builder.Property(p => p.Description)
               .HasColumnName("description")
               .HasColumnType("text")
               .HasMaxLength(BusinessConstants.MaxDescriptionLength)
               .IsRequired();


            // Relationships:
            builder.HasOne(p => p.Category)
                .WithMany(p => p.SubCategories)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_expense_sub_categories_expense_categories_category_id")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Indexes:

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_expense_sub_categories_category_id");

            builder.HasIndex(p => p.Code)
                .IsUnique()
                .HasDatabaseName("UQ_expense_sub_categories_code");

            // Constraints:
        }
    }
}
