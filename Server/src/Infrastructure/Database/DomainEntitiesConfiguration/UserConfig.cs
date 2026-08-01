using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.UserNamespace;
using Infrastructure.Database.ConstraintsConstants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(p => p.Id);

            // Columns:

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.Email)
                .HasColumnName("email")
                .HasColumnType("text")
                .HasMaxLength(BusinessConstants.MaxEmailLength)
                .IsRequired();

            builder.Property(p => p.PasswordHash)
                .HasColumnName("password_hash")
                .HasColumnType("text").
                HasMaxLength(BusinessConstants.MaxPasswordHashLength)
                .IsRequired();

            builder.Property(p => p.DisplayName)
                .HasColumnName("display_name")
                .HasColumnType("text")
                .HasMaxLength(BusinessConstants.MaxDisplayNameLength)
                .IsRequired();

            builder.Property(p => p.Role)
                .HasColumnName("role")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(p => p.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(p => p.RegisteredAt)
                .HasColumnName("registered_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(p => p.EmailVerifiedAt)
                .HasColumnName("email_verified_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(p => p.LastLoginAt)
                .HasColumnName("last_login_at")
                .HasColumnType("timestamp with time zone")
              .IsRequired(false);


            // Relationships:


            // Indexes:
            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName(DatabaseConstraintsConstants.UniqueEmail);

            builder.HasIndex(x => x.Role)
                .HasDatabaseName("IX_users_role");

            // Constraints:
        }
    }
}
