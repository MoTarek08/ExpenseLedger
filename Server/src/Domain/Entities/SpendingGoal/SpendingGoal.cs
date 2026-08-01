using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.UserNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.SpendingGoalNamespace
{
    public class SpendingGoal
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }
        public Guid? CategoryId { get; private set; }

        public string? Description { get; private set; }
        public decimal MaximumTargetAmount { get; private set; }
        public decimal MinimumTargetAmount { get; private set; }

        public DateOnly StartsAt { get; private set; }
        public DateOnly EndsAt { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public User User { get; private set; } = null!;
        public ExpenseCategory? Category { get; private set; }

        private SpendingGoal() { }

        private SpendingGoal(
            Guid userId,
            string? description,
            Guid? categoryId,
            decimal maximumTaretAmount,
            decimal minimumTargetAmount,
            DateOnly startsAt,
            DateOnly endsAt,
            DateTimeOffset createdAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Description = description;
            CategoryId = categoryId;
            MaximumTargetAmount = maximumTaretAmount;
            MinimumTargetAmount = minimumTargetAmount;
            StartsAt = startsAt;
            EndsAt = endsAt;
            CreatedAt = createdAt;  
        }

        public static SpendingGoal Create(
            Guid userId,
            string? description,
            Guid? categoryId,
            decimal maximumTaretAmount,
            decimal minimumTargetAmount,
            DateOnly startsAt,
            DateOnly endsAt,
            DateTimeOffset createdAt)

        {
            if (userId == Guid.Empty)
                throw new DomainException("User id is required");

            if (description is not null)
            {
                if (string.IsNullOrWhiteSpace(description))
                    throw new DomainException("Description cannot be empty or whitespace when provided");

                if (description.Length > BusinessConstants.MaxDescriptionLength)
                    throw new DomainException($"Description cannot be more than {BusinessConstants.MaxDescriptionLength} characters");
            }

            if (categoryId is not null && categoryId == Guid.Empty)
                throw new DomainException("Category id must be a valid Guid when provided");

            if (maximumTaretAmount <= 0)
                throw new DomainException("Maximum target amount must be greater than zero");

            if (minimumTargetAmount <= 0)
                throw new DomainException("Minimum target amount must be greater than zero");

            if (maximumTaretAmount < minimumTargetAmount)
                throw new DomainException("Maximum target amount cannot be less than minimum target amount");

            if (endsAt < startsAt)
                throw new DomainException("Goal end date cannot be earlier than start date");

            if (endsAt > startsAt.AddYears(1))
                throw new DomainException("A spending goal cannot span more than 365 days.");

            return new SpendingGoal(
                userId,
                description?.Trim(),
                categoryId,
                maximumTaretAmount,
                minimumTargetAmount,
                startsAt,
                endsAt,
                createdAt);
        }

        public SpendingGoal UpdateDescription(string? description)
        {
            if (description is not null)
            {
                if (string.IsNullOrWhiteSpace(description))
                    throw new DomainException("Description cannot be empty or whitespace when provided");

                if (description.Length > BusinessConstants.MaxDescriptionLength)
                    throw new DomainException($"Description cannot be more than {BusinessConstants.MaxDescriptionLength} characters");
            }

            Description = description?.Trim();

            return this;
        }

        public SpendingGoal UpdateTargets(decimal minimumTargetAmount, decimal maximumTaretAmount)
        {
            if (minimumTargetAmount <= 0)
                throw new DomainException("Minimum target amount must be greater than zero");

            if (maximumTaretAmount <= 0)
                throw new DomainException("Maximum target amount must be greater than zero");

            if (maximumTaretAmount < minimumTargetAmount)
                throw new DomainException("Maximum target amount cannot be less than minimum target amount");

            MinimumTargetAmount = minimumTargetAmount;
            MaximumTargetAmount = maximumTaretAmount;

            return this;
        }

        public SpendingGoal AssignCategory(Guid? categoryId)
        {
            if (categoryId is not null && categoryId == Guid.Empty)
                throw new DomainException("Category id must be a valid Guid when provided");

            CategoryId = categoryId;

            return this;
        }

        public SpendingGoal Reschedule(DateOnly startsAt, DateOnly endsAt)
        {
            if (endsAt < startsAt)
                throw new DomainException("Goal end date cannot be earlier than start date");

            if (startsAt.AddYears(1) < endsAt)
                throw new DomainException("Gap between start date and end date cannot be more than a year");

            StartsAt = startsAt;
            EndsAt = endsAt;

            return this;
        }

        public SpendingGoal UpdateStartDate(DateOnly startsAt)
        {
            if (EndsAt < startsAt)
                throw new DomainException("Goal end date cannot be earlier than start date");

            if (startsAt.AddYears(1) < EndsAt)
                throw new DomainException("Gap between start date and end date cannot be more than a year");

            StartsAt = startsAt;
            return this;
        }

        public SpendingGoal UpdateEndDate(DateOnly endsAt)
        {
            if (endsAt < StartsAt)
                throw new DomainException("Goal end date cannot be earlier than start date");

            if (StartsAt.AddYears(1) < endsAt)
                throw new DomainException("Gap between start date and end date cannot be more than a year");

            EndsAt = endsAt;
            return this;
        }

        public GoalLifecycle GetLifecycle(DateOnly today)
        {
            if (today < StartsAt)
                return GoalLifecycle.Pending;

            if (today <= EndsAt)
                return GoalLifecycle.Active;

            return GoalLifecycle.Completed;
        }

        public GoalOutcome? Evaluate(
            decimal spendingAmount,
            DateOnly now)
        {
            if (GetLifecycle(now) != GoalLifecycle.Completed)
                return null;

            var succeeded =
                spendingAmount >= MinimumTargetAmount &&
                spendingAmount <= MaximumTargetAmount;

            return succeeded
                ? GoalOutcome.Succeeded
                : GoalOutcome.Failed;
        }
    }
}
