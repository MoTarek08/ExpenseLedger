using Domain.Entities.FileObjectNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class FileObjectConfig : IEntityTypeConfiguration<ExpenseFileObject>
    {
        public void Configure(EntityTypeBuilder<ExpenseFileObject> builder)
        {
            builder.ToTable("expenses_file_objects");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.ExpenseId)
                .HasColumnName("expense_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(p => p.ObjectKey)
                .HasColumnName("object_key")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(p => p.StorageProvider)
                .HasColumnName("storage_provider")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(p => p.ContentType)
                .HasColumnName("content_type")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(p => p.FileSizeInBytes)
                .HasColumnName("file_size_in_bytes")
                .HasColumnType("bigint")
                .IsRequired();


            builder.Property(p => p.OriginalFileName)
                .HasColumnName("original_file_name")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(p => p.Status)
                .HasColumnName("status")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(p => p.StartedProcessingAt)
                .HasColumnName("started_processing_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(p => p.UploadUrlExpiresAt)
                .HasColumnName("upload_url_expires_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(p => p.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);


            // Relationships:
            builder.HasOne(p => p.User)
                .WithMany(p => p.FileObjects)
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK_expenses_file_objects_users_user_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(p => p.Expense)
                .WithOne(p => p.FileObject)
                .HasForeignKey<ExpenseFileObject>(p => p.ExpenseId)
                .HasConstraintName("FK_expenses_file_objects_expenses_expense_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // Indexes:
            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_expenses_file_objects_user_id");

            builder.HasIndex(p => p.ObjectKey)
                .IsUnique()
                .HasDatabaseName("UQ_expenses_file_objects_object_key");

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_expenses_file_objects_status");
        }
    }
}
