namespace Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace.ModelsNamespace
{
    public sealed record UpdateFinancialProfileRequestModel(
        decimal? MonthlyNetIncome,
        int? ResetDay
    );
}
