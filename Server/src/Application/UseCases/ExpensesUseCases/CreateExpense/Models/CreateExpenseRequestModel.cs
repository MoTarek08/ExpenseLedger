namespace Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models
{
    public sealed record CreateExpenseRequestModel(
        Guid CategoryId,
        string? Title,
        decimal Amount,
        DateOnly SpentOn,
        Guid? SubCategoryId); 
}
