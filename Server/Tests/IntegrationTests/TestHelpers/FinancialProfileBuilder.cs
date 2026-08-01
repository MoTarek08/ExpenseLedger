using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.UserFinancialProfileNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.TestHelpers;

public class FinancialProfileBuilder
{
    private readonly IntegrationTestFixture _fixture;
    private readonly Guid _userId;
    private decimal _monthlyNetIncome = 5000m;
    private int _resetDay = 15;

    private FinancialProfileBuilder(IntegrationTestFixture fixture, Guid userId)
    {
        _fixture = fixture;
        _userId = userId;
    }

    public static FinancialProfileBuilder Create(IntegrationTestFixture fixture, Guid userId)
        => new(fixture, userId);

    public FinancialProfileBuilder WithMonthlyIncome(decimal income)
    { _monthlyNetIncome = income; return this; }

    public FinancialProfileBuilder WithResetDay(int day)
    { _resetDay = day; return this; }

    public async Task BuildAsync()
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var usersRepo = sp.GetRequiredService<IUsersRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var profile = UserFinancialProfile.Create(_userId, _monthlyNetIncome, _resetDay, DateTimeOffset.UtcNow);
        usersRepo.AddFinancialProfile(profile);
        await unitOfWork.SaveChangesAsync();
    }
}
