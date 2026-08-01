using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Globalization;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class UserFinancialProfileConfig : IEntityTypeConfiguration<UserFinancialProfile>
    {
        public void Configure(EntityTypeBuilder<UserFinancialProfile> builder)
        {
            builder.ToTable("users_financial_profiles");
            builder.HasKey(p => p.Id);

            // Columns:

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.MonthlyNetIncome)
                .HasColumnName("monthly_net_income")
                .HasColumnType("numeric")
                .HasPrecision(18,2)
                .IsRequired();

            builder.Property(p => p.ResetDay)
                .HasColumnName("reset_day")
                .HasColumnType("int")
                .IsRequired();

            // Relationships:

            builder.HasOne(p => p.User)
                .WithOne(p => p.FinancialProfile)
                .HasForeignKey<UserFinancialProfile>(p => p.UserId)
                .HasConstraintName("FK_users_financial_profiles_users_user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Indexes:
            builder.HasIndex(p => p.UserId)
                .IsUnique()
                .HasDatabaseName("IX_users_financial_profiles_user_id");

            builder.HasIndex(x => x.ResetDay)
                .HasDatabaseName("IX_users_financial_profiles_reset_day");


            // Constraints:
            decimal minMonthlyNetIncome = BusinessConstants.MinMonthlyNetIncome;
            string netIncomeCultured = minMonthlyNetIncome.ToString(CultureInfo.InvariantCulture);

            builder.ToTable(t => t.HasCheckConstraint("CK_users_financial_profiles_monthly_net_income",
                $"monthly_net_income >= {netIncomeCultured}"));
        }
    }
}

