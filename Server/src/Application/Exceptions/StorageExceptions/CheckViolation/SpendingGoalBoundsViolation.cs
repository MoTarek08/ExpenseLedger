using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.CheckViolationNamespace;

namespace Application.Exceptions.StorageExceptions.CheckViolation
{
    public class SpendingGoalBoundsViolation : CheckViolationNamespace.CheckViolation
    {
        public SpendingGoalBoundsViolation() : base(
            "Spending goal bounds constraint violated",
            SpendingGoalsErrorCodes.SPENDING_GOAL_BOUNDS_VIOLATION) { }
    }
}
