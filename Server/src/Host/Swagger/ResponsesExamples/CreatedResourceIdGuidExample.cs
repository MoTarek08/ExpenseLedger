using Host.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples;

public class CreatedResourceIdGuidExample : IExamplesProvider<CreatedResourceId<Guid>>
{
    public CreatedResourceId<Guid> GetExamples() =>
        new CreatedResourceId<Guid>(Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"));
}
