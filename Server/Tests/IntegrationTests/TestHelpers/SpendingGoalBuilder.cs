using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.SpendingGoalNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.TestHelpers;

public class SpendingGoalBuilder
{
    private readonly IntegrationTestFixture _fixture;
    private readonly Guid _userId;
    private Guid? _categoryId;
    private decimal _minimumTargetAmount = 500m;
    private decimal _maximumTargetAmount = 1000m;
    private string? _description;
    private DateOnly _startsAt = DateOnly.FromDateTime(DateTime.UtcNow);
    private DateOnly _endsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    private SpendingGoalBuilder(IntegrationTestFixture fixture, Guid userId)
    {
        _fixture = fixture;
        _userId = userId;
    }

    public static SpendingGoalBuilder Create(IntegrationTestFixture fixture, Guid userId)
        => new(fixture, userId);

    public SpendingGoalBuilder WithCategory(Guid categoryId)
    { _categoryId = categoryId; return this; }

    public SpendingGoalBuilder WithTargets(decimal min, decimal max)
    { _minimumTargetAmount = min; _maximumTargetAmount = max; return this; }

    public SpendingGoalBuilder WithPeriod(DateOnly start, DateOnly end)
    { _startsAt = start; _endsAt = end; return this; }

    public SpendingGoalBuilder WithDescription(string? description)
    { _description = description; return this; }

    public async Task<Guid> BuildAsync()
    {
        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;
        var repo = sp.GetRequiredService<ISpendingGoalsRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var goal = SpendingGoal.Create(
            _userId,
            _description,
            _categoryId,
            _maximumTargetAmount,
            _minimumTargetAmount,
            _startsAt,
            _endsAt,
            DateTimeOffset.UtcNow);

        repo.Add(goal);
        await unitOfWork.SaveChangesAsync();
        return goal.Id;
    }
}
