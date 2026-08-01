using Domain.Entities.RefreshTokenNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid");

            builder.Property(p => p.SessionId)
                .HasColumnName("session_id")
                .HasColumnType("uuid");

            builder.Property(p => p.Token)
                .HasColumnName("token")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(p => p.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(p => p.RevokedAt)
                .HasColumnName("revoked_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            // Relationships:
            builder.HasOne(p => p.User)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK_refresh_tokens_users_user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Indexes:

            builder.HasIndex(p => p.Token)
                .HasDatabaseName("IX_refresh_tokens_token");

            builder.HasIndex(p => p.RevokedAt)
                .HasDatabaseName("IX_refresh_tokens_revoked_at");

            builder.HasIndex(p => p.SessionId)
                .IsUnique()
                .HasFilter("revoked_at IS NULL")
                .HasDatabaseName("UQ_active_refresh_token_session_id");
                

            // Constraints:
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_refresh_tokens_timestamps",
                $"expires_at >= created_at"));
        }
    }
}
