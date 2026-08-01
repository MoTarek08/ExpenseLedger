namespace Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace
{
    public sealed record FinancialProfileDto(
        Guid Id,
        decimal MonthlyNetIncome,
        int ResetDay,
        DateTimeOffset CreatedAt
    );
}
