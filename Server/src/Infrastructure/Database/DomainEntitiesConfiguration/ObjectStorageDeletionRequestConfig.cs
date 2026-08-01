using Domain.Entities.ObjectStorageDeletionRequestNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class ObjectStorageDeletionRequestConfig : IEntityTypeConfiguration<ObjectStorageDeletionRequest>
    {
        public void Configure(EntityTypeBuilder<ObjectStorageDeletionRequest> builder)
        {
            builder.ToTable("object_storage_deletion_requests");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.ObjectKey)
                .HasColumnName("object_key")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(p => p.StorageProvider)
                .HasColumnName("storage_provider")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(p => p.ProcessedAt)
                .HasColumnName("processed_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.HasIndex(p => p.ProcessedAt)
                .HasDatabaseName("IX_object_storage_deletion_requests_processed_at");

            builder.HasIndex(p => p.ObjectKey)
                .IsUnique()
                .HasDatabaseName("UQ_object_storage_deletion_requests_object_key");
        }
    }
}
