using Infrastructure.Database.AppDbContextNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.TestHelpers;

public static class DatabaseAssertions
{
    public static async Task Verify(IntegrationTestFixture fixture, Func<AppDbContext, Task> assertion)
    {
        using var scope = fixture.Factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await assertion(db);
    }
}
