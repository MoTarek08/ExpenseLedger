using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.UserNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.UserCategoryPreferenceNamespace
{
    public class UserCategoryPreference
    {
        public Guid UserId { get; private set; }
        public Guid CategoryId { get; private set; }

        public CategoryPreferenceLevel PreferenceLevel { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public User User { get; private set; } = null!;
        public ExpenseCategory Category { get; private set; } = null!;

        private UserCategoryPreference() { }

        private UserCategoryPreference(
            Guid userId,
            Guid categoryId,
            CategoryPreferenceLevel preferenceLevel,
            DateTimeOffset createdAt)

        {
            UserId = userId;
            CategoryId = categoryId;
            PreferenceLevel = preferenceLevel;
            CreatedAt = createdAt;
        }

        public static UserCategoryPreference Create(
            Guid userId,
            Guid categoryId,
            CategoryPreferenceLevel preferenceLevel,
            DateTimeOffset createdAt)

        {

            if (userId == Guid.Empty)
                throw new DomainException("User id is required");

            if (categoryId == Guid.Empty)
                throw new DomainException("Category id is required");

            return new UserCategoryPreference(userId, categoryId, preferenceLevel,createdAt);
        }


        public UserCategoryPreference ChangePreferenceLevel(CategoryPreferenceLevel preferenceLevel)
        {
            PreferenceLevel = preferenceLevel;
            return this;
        }
    }
}
