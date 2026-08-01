using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class SpendingGoalDtoExample : IExamplesProvider<SpendingGoalDto>
    {
        public SpendingGoalDto GetExamples()
        {
            return new SpendingGoalDto(
                Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                "Keep restaurant spending in check",
                Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                700m, 500m, 320m,
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 31),
                new DateTimeOffset(2026, 7, 15, 10, 30, 0, TimeSpan.Zero),
                SpendingGoalStatus.InProgress);
        }
    }
}
