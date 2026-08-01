using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;
using Domain.Entities.UserNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.ScheduledExpenseNamespace
{
    public class ScheduledExpense
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }

        public string? Title { get; private set; }
        public decimal Amount { get; private set; }

        public Guid CategoryId { get; private set; }
        public Guid? SubCategoryId { get; private set; }

        public CadenceInterval Cadence { get; private set; }

        public DateOnly FirstDueOn { get; private set; }
        public DateOnly? NextDueOn { get; private set; }
        public DateOnly? LastProcessedAt { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; } 
        public bool IsActive { get; private set; } = true;

        public User User { get; private set; } = null!;
        public ExpenseCategory Category { get; private set; } = null!;
        public ExpenseSubCategory? SubCategory { get; private set; }

        private readonly List<Expense> _generatedExpenses = [];
        public IReadOnlyList<Expense> GeneratedExpenses => _generatedExpenses.AsReadOnly();

        private ScheduledExpense() { }

        private ScheduledExpense(
            Guid userId,
            string? title,
            decimal amount,
            Guid categoryId,
            Guid? subCategoryId,
            CadenceInterval cadence,
            DateOnly firstDueOn,
            DateTimeOffset createdAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            Amount = amount;
            CategoryId = categoryId;
            SubCategoryId = subCategoryId;
            Cadence = cadence;
            FirstDueOn = firstDueOn;
            NextDueOn = firstDueOn;
            CreatedAt = createdAt;
        }

        public static ScheduledExpense Create(
            Guid userId,
            string? title,
            decimal amount,
            Guid categoryId,
            Guid? subCategoryId,
            CadenceInterval cadence,
            DateOnly firstDueOn,
            DateTimeOffset createdAt)
        {
            if (userId == Guid.Empty)
                throw new DomainException("User id is required");

            if (title is not null)
            {
                if (string.IsNullOrWhiteSpace(title))
                    throw new DomainException("Title cannot be empty or whitespace.");

                if (title.Length > BusinessConstants.MaxTitleLength)
                    throw new DomainException($"Title must not be more than {BusinessConstants.MaxTitleLength} characters");

                title = title.Trim();
            }

            if (amount <= 0)
                throw new DomainException("Amount must be greater than zero");

            if (categoryId == Guid.Empty)
                throw new DomainException("Category id must be a positive value");



            return new ScheduledExpense(
                userId,
                title,
                amount,
                categoryId,
                subCategoryId,
                cadence,
                firstDueOn,
                createdAt);
        }

        public ScheduledExpense UpdateTitle(string? title)
        {
            if (!IsActive)
                throw new DomainException("Scheduled expense is not active and cannot be modefied");

            if (title is not null)
            {
                if (string.IsNullOrWhiteSpace(title))
                    throw new DomainException("Title cannot be empty or whitespace.");

                if (title.Length > BusinessConstants.MaxTitleLength)
                    throw new DomainException($"Title cannot be more than {BusinessConstants.MaxTitleLength} characters");

                Title = title.Trim();
            }
            else
            {
                Title = null;
            }

            return this;
        }

        public ScheduledExpense UpdateAmount(decimal amount)
        { 

            if (!IsActive)
                throw new DomainException("Scheduled expense is not active and cannot be modefied");

            if (amount <= 0)
                throw new DomainException("Amount must be greater than zero");

            Amount = amount;
            return this;
        }

        public ScheduledExpense ChangeCadence(CadenceInterval cadence)
        {

            if (!IsActive)
                throw new DomainException("Scheduled expense is not active and cannot be modefied");

            if (NextDueOn is null)
                throw new DomainException("cannot change cadence for scheduled expense with no next due date");

            Cadence = cadence;

            if (cadence == CadenceInterval.Once)
                return this;

            if (LastProcessedAt is not null)
            {
                NextDueOn = CalculateNextDueOn(LastProcessedAt.Value);
                return this;
            }

            NextDueOn = CalculateNextDueOn(NextDueOn.Value);
            return this;
        }

        public void MarkAsProcessed(DateOnly processedAt)
        {

            if (!IsActive)
                throw new DomainException("Scheduled expense is not active and cannot be modefied");

            if (processedAt < FirstDueOn)
                throw new DomainException("Processed date cannot be earlier than the first due date");

            LastProcessedAt = processedAt;

            if (Cadence == CadenceInterval.Once)
            {
                NextDueOn = null;
                IsActive = false;
                return;
            }

            NextDueOn = CalculateNextDueOn(NextDueOn ?? FirstDueOn);
        }

        public void Cancel()
        {
            IsActive = false;
            NextDueOn = null;
        }

        public ScheduledExpense ChangeFirstDue(DateOnly newFirstDueDate)
        {

            if (!IsActive)
                throw new DomainException("Scheduled expense is not active and cannot be modefied");

            if (LastProcessedAt is not null)
                throw new DomainException("Cannot change first due date for alrady processed & active expenses");


            FirstDueOn = newFirstDueDate;
            NextDueOn = newFirstDueDate;
            return this;
        }

        private DateOnly? CalculateNextDueOn(DateOnly currentDueOn)
        {
            return Cadence switch
            {
                CadenceInterval.Once => null,
                CadenceInterval.Daily => currentDueOn.AddDays(1),
                CadenceInterval.Weekly => currentDueOn.AddDays(7),
                CadenceInterval.Monthly => currentDueOn.AddMonths(1),
                CadenceInterval.Quarterly => currentDueOn.AddMonths(3),
                CadenceInterval.Yearly => currentDueOn.AddYears(1),
                _ => throw new DomainException("Unsupported cadence interval")
            };
        }

    }
}
