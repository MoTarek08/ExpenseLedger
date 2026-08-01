namespace Application.UseCases.ExpensesUseCases.Models
{
    public sealed record ExpenseDto(
        Guid Id,
        Guid UserId,
        DateOnly SpentOn,
        string? Title,
        decimal Amount,
        string CategoryCode,
        string? SubCategoryCode,
        Guid? ScheduledExpenseId,
        int NotesCount);
}
