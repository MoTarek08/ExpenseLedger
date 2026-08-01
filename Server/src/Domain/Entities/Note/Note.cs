using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.NoteNamespace
{
    public class Note
    {
        public Guid Id { get; private set; }
        public Guid ExpenseId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; private set; }

        public Expense Expense { get; private set; } = null!;

        private Note() { }

        private Note(Guid expenseId, string content, DateTimeOffset createdAt)
        {
            Id = Guid.NewGuid();
            ExpenseId = expenseId;
            Content = content;
            CreatedAt = createdAt;
        }

        public static Note Create(Guid expenseId, string content, DateTimeOffset createdAt)
        {
            if (expenseId == Guid.Empty)
                throw new DomainException("Expense id is required");

            ValidateContent(content);

            return new Note(expenseId, content.Trim(), createdAt);
        }

        public Note UpdateContent(string content)
        {
            ValidateContent(content);
            Content = content.Trim();
            return this;
        }

        private static void ValidateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException("Note content is required");

            if (content.Trim().Length > BusinessConstants.MaxNoteContentLength)
                throw new DomainException($"Note content must not be more than {BusinessConstants.MaxNoteContentLength} characters");
        }
    }
}
