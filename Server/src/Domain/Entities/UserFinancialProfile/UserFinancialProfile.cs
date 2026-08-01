using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.UserNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.UserFinancialProfileNamespace
{
    public class UserFinancialProfile
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public decimal MonthlyNetIncome { get; private set; }
        public int ResetDay { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public User User { get; private set; } = null!;

        private UserFinancialProfile() { }

        private UserFinancialProfile(
            Guid userId,
            decimal monthlyNetIncome,
            int resetDay,
            DateTimeOffset createdAt)
      {
            Id = Guid.NewGuid();
            UserId = userId;
            MonthlyNetIncome = monthlyNetIncome;
            ResetDay = resetDay;
            CreatedAt = createdAt;
        }

        public static UserFinancialProfile Create(
            Guid userId,
            decimal monthlyNetIncome,
            int resetDay,
            DateTimeOffset createdAt)
        {

            if (userId == Guid.Empty)
                throw new DomainException("User id is required");

            if (monthlyNetIncome < BusinessConstants.MinMonthlyNetIncome)
                throw new DomainException($"Monthly net income cannot be less than {BusinessConstants.MinMonthlyNetIncome}");

            if (resetDay <= 0 || resetDay > 31)
                throw new DomainException("Invalid pay day");

            return new UserFinancialProfile(userId, monthlyNetIncome, resetDay,createdAt);
        }

        public UserFinancialProfile UpdateMonthlyNetIncome(decimal monthlyNetIncome)
        {
            if (monthlyNetIncome < BusinessConstants.MinMonthlyNetIncome)
                throw new DomainException($"Monthly net income cannot be less than {BusinessConstants.MinMonthlyNetIncome}");

            MonthlyNetIncome = monthlyNetIncome;
            return this;
        }

        public UserFinancialProfile UpdateResetDay(int resetDay)
        {
            if (resetDay <= 0 || resetDay > 31)
                throw new DomainException("Invalid reset day.");

            ResetDay = resetDay;
            return this;
        }
    }
}
