using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CreateSpendingGoalRequestModelExample : IExamplesProvider<CreateSpendingGoalRequestModel>
    {
        public CreateSpendingGoalRequestModel GetExamples()
        {
            return new CreateSpendingGoalRequestModel(
                "Keep restaurant spending in check",
                700m,
                500m,
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 31),
                Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"));
        }
    }
}
