using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class FinancialProfileDtoExample : IExamplesProvider<FinancialProfileDto>
    {
        public FinancialProfileDto GetExamples()
        {
            return new FinancialProfileDto(
                Id: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                MonthlyNetIncome: 5000m,
                ResetDay: 1,
                CreatedAt: new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.Zero));
        }
    }
}
