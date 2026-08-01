using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using Domain.Entities.SpendingGoalNamespace;
using Domain.Entities.UserCategoryPreferenceNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.ExpenseCategoryNamespace
{
    public class ExpenseCategory
    {
        public Guid Id { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        private readonly List<ExpenseSubCategory> _subCategories = [];
        public IReadOnlyList<ExpenseSubCategory> SubCategories => _subCategories.AsReadOnly();

        private readonly List<Expense> _expenses = [];
        public IReadOnlyList<Expense> Expenses => _expenses.AsReadOnly();

        private readonly List<ScheduledExpense> _scheduledExpenses = [];
        public IReadOnlyList<ScheduledExpense> ScheduledExpenses => _scheduledExpenses.AsReadOnly();

        private readonly List<SpendingGoal> _spendingGoals = [];
        public IReadOnlyList<SpendingGoal> SpendingGoals => _spendingGoals.AsReadOnly();

        private readonly List<UserCategoryPreference> _userCategoryPreferences = [];
        public IReadOnlyList<UserCategoryPreference> UserCategoryPreferences => _userCategoryPreferences.AsReadOnly();

        private ExpenseCategory() { }

        private ExpenseCategory(string code, string name, string description)
        {
            Id = Guid.NewGuid();
            Code = code;
            Name = name;
            Description = description;
        }

        public static ExpenseCategory Create(string code, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Code is required");

            if (code.Trim().Length > BusinessConstants.MaxCategoryNameLength)
                throw new DomainException($"Category code cannot be more than {BusinessConstants.MaxCategoryNameLength} characters.");

            if(description.Length > BusinessConstants.MaxDescriptionLength)
                throw new DomainException($"Description cannot be more than {BusinessConstants.MaxDescriptionLength} characters");

            ValidateName(name);

            return new ExpenseCategory(code.Trim(), name.Trim(),description.Trim());
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Category name is required");

            if (name.Trim().Length > BusinessConstants.MaxCategoryNameLength)
                throw new DomainException($"Category name must not be more than {BusinessConstants.MaxCategoryNameLength} characters");
        }
    }
}
