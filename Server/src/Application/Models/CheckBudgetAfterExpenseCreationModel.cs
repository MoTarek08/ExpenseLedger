namespace Application.Models
{
    public sealed record CheckBudgetAfterExpenseCreationModel(
        DateOnly ExpenseSpentOn,
        Guid UserId,
        int ResetDay,
        decimal MonthlyNetIncome)
    {
    }
}
