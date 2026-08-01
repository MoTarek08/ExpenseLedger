using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.SearchExpenses.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Domain.Entities.ExpenseNamespace;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using static Application.ApplicationConstantsNamesapce.ApplicationConstants;

namespace Application.UseCases.ExpensesUseCases.SearchExpenses
{
    public class SearchExpensesUseCase
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<SearchExpensesUseCase> _logger;

        public SearchExpensesUseCase(
            IExpensesRepository expensesRepository,
            IUsersRepository usersRepository,
            ILogger<SearchExpensesUseCase> logger)
        {
            _expensesRepository = expensesRepository;
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<Result<List<ExpenseDto>>> Execute(Guid userId, SearchExpensesQueryParameters queryParameters, CancellationToken cancellationToken)
        {
            var userFinancialProfile = await _usersRepository.GetFinancialProfileByUserIdAsync(userId, cancellationToken);
            if (userFinancialProfile is null)
            {
                _logger.LogInformation("User {UserId} has no financial profile, returning empty expense list", userId);
                return Result<List<ExpenseDto>>.Success(new List<ExpenseDto>());
            }

            var query = _expensesRepository.GetAllForUserQuery(userId);

            if (!string.IsNullOrWhiteSpace(queryParameters.Title))
            {
                query = query.Where(e => e.Title!.Contains(queryParameters.Title));
            }
            if (queryParameters.From.HasValue)
                query = query.Where(e => e.SpentOn >= queryParameters.From.Value);

            if (queryParameters.To.HasValue)
                query = query.Where(e => e.SpentOn <= queryParameters.To.Value);

            if (queryParameters.MinAmount.HasValue)
                query = query.Where(e => e.Amount >= queryParameters.MinAmount.Value);

            if (queryParameters.MaxAmount.HasValue)
                query = query.Where(e => e.Amount <= queryParameters.MaxAmount.Value);

            if (queryParameters.CategoryIds is { Count: > 0 } || queryParameters.SubCategoryIds is { Count: > 0 })
            {
                query = query.Where(e =>
                    (queryParameters.CategoryIds != null && queryParameters.CategoryIds!.Contains(e.CategoryId))
                    ||
                    (queryParameters.SubCategoryIds != null && e.SubCategoryId != null && queryParameters.SubCategoryIds!.Contains(e.SubCategoryId.Value))
                );
            }

            query = queryParameters.SortOrder.ToUpperInvariant() == SortOrders.Descending
                ? query.OrderByDescending(GetSortExpression(queryParameters.SortBy))
                : query.OrderBy(GetSortExpression(queryParameters.SortBy));


            var data = await _expensesRepository.GetExpenseDtoAsync(
                query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize),
                cancellationToken);


            return Result<List<ExpenseDto>>.Success(data);
        }


        private static Expression<Func<Expense, object>> GetSortExpression(string sortBy) =>
            sortBy.ToUpperInvariant() switch
            {
                "SPENTON" => e => e.SpentOn,
                "AMOUNT" => e => e.Amount,
                "CATEGORY" => e => e.Category.Code,
                _ => e => e.SpentOn
            };
        
    }
    
}
