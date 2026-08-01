using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseCategoryNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class ExpenseCategoryConfig : IEntityTypeConfiguration<ExpenseCategory>
    {
        public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
        {
            builder.ToTable("expense_categories");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

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

            // Indexes:
            builder.HasIndex(p => p.Code)
                .IsUnique()
                .HasDatabaseName("UQ_expense_categories_code");

            // Constraints:
        }
    }
}
