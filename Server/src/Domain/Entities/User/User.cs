using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.FileObjectNamespace;
using Domain.Entities.RefreshTokenNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using Domain.Entities.SpendingGoalNamespace;
using Domain.Entities.UserCategoryPreferenceNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.UserNamespace
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string DisplayName { get; private set; } = null!;
        public Role Role { get; private set; }


        public DateTimeOffset RegisteredAt { get; private set; }
        public DateTimeOffset? EmailVerifiedAt { get; private set; }
        public DateTimeOffset? LastLoginAt { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }


        public UserFinancialProfile? FinancialProfile { get; private set; }

        private readonly List<Expense> _expenses = [];
        public IReadOnlyList<Expense> Expenses => _expenses.AsReadOnly();

        private readonly List<ScheduledExpense> _scheduledExpenses = [];
        public IReadOnlyList<ScheduledExpense> ScheduledExpenses => _scheduledExpenses.AsReadOnly();

        private readonly List<SpendingGoal> _spendingGoals = [];
        public IReadOnlyList<SpendingGoal> SpendingGoals => _spendingGoals.AsReadOnly();

        private readonly List<UserCategoryPreference> _categoryPreferences = [];
        public IReadOnlyList<UserCategoryPreference> CategoryPreferences => _categoryPreferences.AsReadOnly();

        //private readonly List<ExpenseImport> _expenseImports = [];
        //public IReadOnlyList<ExpenseImport> ExpenseImports => _expenseImports.AsReadOnly();

        private readonly List<ExpenseFileObject> _fileObjects = [];
        public IReadOnlyList<ExpenseFileObject> FileObjects => _fileObjects.AsReadOnly();

        private readonly List<RefreshToken> _refreshTokens = [];
        public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private User() { }

        private User(string email, string passwordHash, string displayName, Role role, DateTimeOffset registerdAt)
        {
            Id = Guid.NewGuid();
            Email = email;
            PasswordHash = passwordHash;
            DisplayName = displayName;
            RegisteredAt = registerdAt;
            Role = role;
        }

        public static User Register(string email, string passwordHash, string displayName, Role role, DateTimeOffset registeredAt)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email is required.");

            if (email.Trim().Length > BusinessConstants.MaxEmailLength)
                throw new DomainException($"Email must not be more than {BusinessConstants.MaxEmailLength} characters");

            if (string.IsNullOrWhiteSpace(displayName))
                throw new DomainException("Display name is required.");

            if (displayName.Trim().Length > BusinessConstants.MaxDisplayNameLength)
                throw new DomainException($"Display name must not be more than {BusinessConstants.MaxDisplayNameLength} characters");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("Password hash is required.");

            return new User(email, passwordHash, displayName, role, registeredAt);
        }

        public User MarkAsLoggedIn(DateTimeOffset loggedInAt)
        {
            if (DeletedAt is not null)
                throw new DomainException("User is deleted and cannot login");

            LastLoginAt = loggedInAt;
            return this;
        }

        public User MarkAsDeleted(DateTimeOffset deletedAt)
        {
            DeletedAt = deletedAt;
            return this;
        }

        public User UpdateDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new DomainException("Display name is required.");

            if (displayName.Trim().Length > BusinessConstants.MaxDisplayNameLength)
                throw new DomainException($"Display name must not be more than {BusinessConstants.MaxDisplayNameLength} characters.");

            DisplayName = displayName.Trim();
            return this;
        }
    }
}
