using Application.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CategoryDetailsListExample : IExamplesProvider<List<CategoryDetails>>
    {
        public List<CategoryDetails> GetExamples()
        {
            return new List<CategoryDetails>
            {
                new(
                    Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    "FOOD",
                    "Food & Dining",
                    "Expenses related to food and dining out.",
                    new List<SubCategoryDetails>
                    {
                        new(Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "FOOD_GROCERIES", "Groceries", "Supermarket and grocery store purchases"),
                        new(Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"), "FOOD_RESTAURANT", "Restaurants", "Dining out at restaurants and cafes"),
                    }),
                new(
                    Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123"),
                    "TRANSPORT",
                    "Transportation",
                    "Expenses related to travel and commuting.",
                    new List<SubCategoryDetails>
                    {
                        new(Guid.Parse("e5f6a7b8-c9d0-1234-efab-345678901234"), "TRANSPORT_FUEL", "Fuel", "Gasoline and charging costs"),
                        new(Guid.Parse("f6a7b8c9-d0e1-2345-fabc-456789012345"), "TRANSPORT_PUBLIC", "Public Transit", "Bus, train, and subway fares"),
                    }),
            };
        }
    }
}
