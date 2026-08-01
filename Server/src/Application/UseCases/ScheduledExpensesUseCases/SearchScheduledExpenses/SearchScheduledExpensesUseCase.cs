using Application.Interfaces.Repositories;
using Application.Models.Result;
using Application.UseCases.ScheduledExpensesUseCases.Models;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses.Models;
using Domain.Entities.ScheduledExpenseNamespace;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using static Application.ApplicationConstantsNamesapce.ApplicationConstants;

namespace Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses
{
    public class SearchScheduledExpensesUseCase
    {
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly ILogger<SearchScheduledExpensesUseCase> _logger;

        public SearchScheduledExpensesUseCase(
            IScheduledExpensesRepository scheduledExpensesRepository,
            ILogger<SearchScheduledExpensesUseCase> logger)
        {
            _scheduledExpensesRepository = scheduledExpensesRepository;
            _logger = logger;
        }

        public async Task<Result<List<ScheduledExpenseDto>>> Execute(Guid userId, SearchScheduledExpensesQueryParameters queryParameters, CancellationToken cancellationToken)
        {
            var query = _scheduledExpensesRepository.GetAllForUserQuery(userId);

            if (queryParameters.ActiveOnly == true)
                query = query.Where(se => se.IsActive);

            query = queryParameters.SortOrder.ToUpperInvariant() == SortOrders.Descending
                ? query.OrderByDescending(GetSortExpression(queryParameters.SortBy))
                : query.OrderBy(GetSortExpression(queryParameters.SortBy));

            var data = await _scheduledExpensesRepository.GetScheduledExpenseDtoAsync(
                query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize),
                cancellationToken);

            _logger.LogInformation("Scheduled expenses search {UserId} returned {Count} results", userId, data.Count);
            return Result<List<ScheduledExpenseDto>>.Success(data);
        }

        private static Expression<Func<ScheduledExpense, object>> GetSortExpression(string sortBy) =>
            sortBy.ToUpperInvariant() switch
            {
                "FIRSTDUEON" => se => se.FirstDueOn,
                "NEXTDUEON" => se => se.NextDueOn!,
                "LASTPROCESSEDAT" => se => se.LastProcessedAt!,
                "CADENCE" => se => se.Cadence,
                _ => se => se.CreatedAt
            };
    }
}
