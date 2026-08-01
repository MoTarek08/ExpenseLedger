using Application.Models;

namespace Application.UseCases.ExpensesUseCases.GetExpensesByDay.Models
{
    public sealed record GetExpensesByDayRequestModel(DateOnly Day) : PaginationParameters;
}
