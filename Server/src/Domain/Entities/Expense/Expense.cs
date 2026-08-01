using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;
using Domain.Entities.FileObjectNamespace;
using Domain.Entities.NoteNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using Domain.Entities.UserNamespace;
using Domain.ExceptionsNamespace;


namespace Domain.Entities.ExpenseNamespace
{
    public class Expense
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }

        public Guid CategoryId { get; private set; }
        public Guid? SubCategoryId { get; private set; }

        public Guid? ScheduledExpenseId { get; private set; }
        public DateOnly? ScheduledGenerationDate { get; private set; }

        public string? Title { get; private set; }
        public decimal Amount { get; private set; }

        public DateOnly SpentOn { get; private set; }
        
        public User User { get; private set; } = null!; 
        public ExpenseCategory Category { get; private set; } = null!;
        public ExpenseSubCategory? SubCategory { get; private set; }
        public ScheduledExpense? ScheduledExpense { get; private set; }
        public ExpenseFileObject? FileObject { get; private set; }
        
        public DateTimeOffset CreatedAt { get; private set; }


        private readonly List<Note> _notes = [];
        public IReadOnlyList<Note> Notes => _notes.AsReadOnly();


        private Expense() { }

        private Expense(Guid userId,
            Guid categoryId,
            string? title,
            decimal amount,
            DateOnly spentOn,
            Guid? subCategoryId,
            DateTimeOffset createdAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CategoryId = categoryId;
            Title = title;
            Amount = amount;
            SpentOn = spentOn;
            SubCategoryId = subCategoryId;
            CreatedAt = createdAt;
        }

        public static Expense CreateManualExpense(Guid userId, 
            Guid categoryId,
            string? title,
            decimal amount,
            DateOnly spentOn,
            DateTimeOffset createdAt,
            Guid? subCategoryId = null)
        {
            if (userId == Guid.Empty)
                throw new DomainException("User id cannot be empty");

            if(categoryId == Guid.Empty)
                throw new DomainException("Category id cannot be empty");

            if (subCategoryId is not null && subCategoryId == Guid.Empty)
                throw new DomainException("Sub category id cannot be empty when provided");

            if (amount <= 0)
                throw new DomainException("amount cannot be smaller than 0");

            if (title is not null)
            {
                if (string.IsNullOrWhiteSpace(title))
                    throw new DomainException("Title cannot be empty or whitespace.");

                if (title.Length > BusinessConstants.MaxTitleLength)
                    throw new DomainException($"Title must not be more than {BusinessConstants.MaxTitleLength} characters.");

                title = title.Trim();
            }
            
            return new Expense(userId, categoryId, title, amount, spentOn, subCategoryId,createdAt);
           
        }


        public Expense LinkToScheduledExpense(Guid scheduledExpenseId, DateOnly generationDate)
        {
            ScheduledExpenseId = scheduledExpenseId;
            ScheduledGenerationDate = generationDate;
            return this;
        }

        public Expense ChangeMainCategory(Guid categoryId)
        {
            CategoryId = categoryId;
            return this;
        }

        public Expense ChangeSubCategory(Guid? subCategoryId)
        {
            SubCategoryId = subCategoryId;
            return this;
        }

        public Expense ChangeTitle(string? title)
        {
            if (title is not null)
            {
                if (string.IsNullOrWhiteSpace(title))
                    throw new DomainException("Title cannot be empty or whitespace.");

                if (title.Length > BusinessConstants.MaxTitleLength)
                    throw new DomainException($"Title must not be more than {BusinessConstants.MaxTitleLength} characters.");

                Title = title.Trim();
            }
            else
            {
                Title = null;
            }

            return this;
        }

        public Expense ChangeAmount(decimal amount)
        {
            Amount = amount;
            return this;
        }

        public Expense ChangeSpentOn(DateOnly newSpentOn)
        {
            SpentOn = newSpentOn;
            return this;
        }

    }
}
