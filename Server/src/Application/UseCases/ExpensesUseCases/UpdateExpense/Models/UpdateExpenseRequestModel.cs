namespace Application.UseCases.ExpensesUseCases.UpdateExpense.Models
{
    public sealed record UpdateExpenseRequestModel(
        string? Title,
        decimal? Amount,
        Guid? CategoryId,
        Guid? SubCategoryId,
        DateOnly? SpentOn);
}
