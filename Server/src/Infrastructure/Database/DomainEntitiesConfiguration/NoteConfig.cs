using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.NoteNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.DomainEntitiesConfigurationNamespace
{
    public class NoteConfig : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            builder.ToTable("notes");
            builder.HasKey(p => p.Id);

            // Columns:
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(p => p.ExpenseId)
                .HasColumnName("expense_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(p => p.Content)
                .HasColumnName("content")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Relationships:
            builder.HasOne(p => p.Expense)
                .WithMany(p => p.Notes)
                .HasForeignKey(p => p.ExpenseId)
                .HasConstraintName("FK_notes_expenses_expense_id")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Indexes:

            builder.HasIndex(p => p.ExpenseId)
                .HasDatabaseName("IX_notes_expenses_expense_id");

            // Constraints:
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_notes_content_length",
                $"length(content) >= {BusinessConstants.MinNoteContentLength} AND length(content) <= {BusinessConstants.MaxNoteContentLength}"));
        }
    }
}
