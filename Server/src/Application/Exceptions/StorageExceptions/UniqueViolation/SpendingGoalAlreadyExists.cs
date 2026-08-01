using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolation
{
    public class SpendingGoalAlreadyExists : UniqueViolationNamespace.UniqueViolation
    {
        public SpendingGoalAlreadyExists() : base(
            "A spending goal for the specified period already exists",
            SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS) { }
    }
}
