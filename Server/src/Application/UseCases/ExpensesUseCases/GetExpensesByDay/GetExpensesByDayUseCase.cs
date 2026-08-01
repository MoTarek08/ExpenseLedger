using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.GetExpensesByDay.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ExpensesUseCases.GetExpensesByDay
{
    public class GetExpensesByDayUseCase
    {
        private readonly IExpensesRepository _repository;
        private readonly ILogger<GetExpensesByDayUseCase> _logger;

        public GetExpensesByDayUseCase(
            IExpensesRepository repository,
            ILogger<GetExpensesByDayUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<List<ExpenseDto>>> Execute(Guid userId, GetExpensesByDayRequestModel queryParameters, CancellationToken cancellationToken)
        {
            var query = _repository.GetAllForUserInDayQuery(userId, queryParameters.Day);
            var expenses = await _repository.ToExpenseDtoAsync(
                query
                .Skip(queryParameters.PageNumber -1)
                .Take(queryParameters.PageSize)
                ,cancellationToken);

            _logger.LogInformation("Fetched {ExpensesCount} expenses for user {UserId} on {Day}", userId, queryParameters.Day, expenses.Count);
            return Result<List<ExpenseDto>>.Success(expenses);
        }
    }
}
