using Application.UseCases.ExpensesUseCases.Models;
using Domain.BusinessInvariants.CategoryCodesNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class ExpenseDtoListExample : IExamplesProvider<List<ExpenseDto>>
    {
        public List<ExpenseDto> GetExamples() 
        {
            return new List<ExpenseDto>() 
            {
                new ExpenseDto(
                Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a"),
                Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930"),
                new DateOnly(2026, 3, 21),
                "mother's day present for my mother",
                1500,
                CategoryCodes.Gifts.Code,
                null,
                null,
                2),

            new ExpenseDto(
                Guid.Parse("7e32359e-c0ec-4f10-bec1-f161d0ae2569"),
                Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3"),
                new DateOnly(2026, 2, 1),
                "Gym membership",
                1000,
                CategoryCodes.Fitness.Code,
                CategoryCodes.Fitness.GymMembership.Code,
                Guid.Parse("cb5137d4-e03e-4f7a-ac8e-2ee2b53da43f"),
                0)
            };
        }
    }
}
