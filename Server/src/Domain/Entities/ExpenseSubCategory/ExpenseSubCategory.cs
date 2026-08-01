using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.ExpenseSubCategoryNamespace
{
    public class ExpenseSubCategory
    {
        public Guid Id { get; private set; }
        public Guid CategoryId { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public ExpenseCategory Category { get; private set; } = null!;

        private readonly List<Expense> _expenses = [];
        public IReadOnlyList<Expense> Expenses => _expenses.AsReadOnly();

        private readonly List<ScheduledExpense> _scheduledExpenses = [];
        public IReadOnlyList<ScheduledExpense> ScheduledExpenses => _scheduledExpenses.AsReadOnly();

        private ExpenseSubCategory() { }

        private ExpenseSubCategory(Guid categoryId, string code, string name, string description)
        {
            Id = Guid.NewGuid();
            CategoryId = categoryId;
            Code = code;
            Name = name;
            Description = description;
        }

        public static ExpenseSubCategory Create(Guid categoryId, string code, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Code is required");

            if (code.Trim().Length > BusinessConstants.MaxCategoryNameLength)
                throw new DomainException($"Category code cannot be more than {BusinessConstants.MaxCategoryNameLength} characters.");

            if(description.Length > BusinessConstants.MaxDescriptionLength)
                throw new DomainException($"Description cannot be more than {BusinessConstants.MaxDescriptionLength} characters");

            ValidateName(name);

            return new ExpenseSubCategory(categoryId, code.Trim(), name.Trim(),description.Trim());
        }


        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Subcategory is required");

            if (name.Trim().Length > BusinessConstants.MaxCategoryNameLength)
                throw new DomainException($"Name must not be more than {BusinessConstants.MaxCategoryNameLength} characters.");


        }
    }
}
