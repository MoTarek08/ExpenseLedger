using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Domain.Entities.ExpenseNamespace;

namespace Application.Interfaces.SharedServices
{
    public interface IBuildExpenseService
    {
        public Task<Result<Expense>> BuildExpense(Guid userId, CreateExpenseRequestModel requestModel, CancellationToken cancellationToken = default);
    }
}