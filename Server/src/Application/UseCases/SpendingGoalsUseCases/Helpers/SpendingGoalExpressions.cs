using System.Linq.Expressions;
using Domain.Entities.ExpenseNamespace;

namespace Application.UseCases.SpendingGoalsUseCases.Helpers
{
    public static class SpendingGoalExpressions
    {
        public static readonly Expression<Func<Expense, Guid, Guid?, DateOnly, DateOnly, bool>> ExpenseAffectsGoal =
            (e, userId, categoryId, startsAt, endsAt) =>
                e.UserId == userId &&
                (categoryId == null || e.CategoryId == categoryId) &&
                e.SpentOn >= startsAt &&
                e.SpentOn <= endsAt;
    }
}
