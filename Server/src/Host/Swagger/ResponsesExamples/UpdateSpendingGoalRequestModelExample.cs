using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateSpendingGoalRequestModelExample : IExamplesProvider<UpdateSpendingGoalRequestModel>
    {
        public UpdateSpendingGoalRequestModel GetExamples()
        {
            return new UpdateSpendingGoalRequestModel(
                "Updated description for restaurant goal",
                600m,
                800m,
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 9, 1));
        }
    }
}
