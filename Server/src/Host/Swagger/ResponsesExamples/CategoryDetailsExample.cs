using Application.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CategoryDetailsExample : IExamplesProvider<CategoryDetails>
    {
        public CategoryDetails GetExamples()
        {
            return new CategoryDetails(
                Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                "FOOD",
                "Food & Dining",
                "Expenses related to food and dining out.",
                new List<SubCategoryDetails>
                {
                    new(Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "FOOD_GROCERIES", "Groceries", "Supermarket and grocery store purchases"),
                    new(Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"), "FOOD_RESTAURANT", "Restaurants", "Dining out at restaurants and cafes"),
                });
        }
    }
}
