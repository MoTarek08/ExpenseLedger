using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences.Models;
using Microsoft.Extensions.Logging;
using static Application.ApplicationConstantsNamesapce.ApplicationConstants;

namespace Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences
{
    public class SearchUserCategoryPreferencesUseCase
    {
        private readonly IUserCategoryPreferencesRepository _repository;
        private readonly ILogger<SearchUserCategoryPreferencesUseCase> _logger;

        public SearchUserCategoryPreferencesUseCase(
            IUserCategoryPreferencesRepository repository,
            ILogger<SearchUserCategoryPreferencesUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<List<UserCategoryPreferenceDto>>> Execute(
            Guid userId,
            SearchUserCategoryPreferencesQueryParameters queryParameters,
            CancellationToken cancellationToken)
        {
            var query = _repository.GetAllForUserQuery(userId);

            if (queryParameters.PreferenceLevel.HasValue)
                query = query.Where(p => p.PreferenceLevel == queryParameters.PreferenceLevel.Value);

            query = queryParameters.SortOrder.ToUpperInvariant() == SortOrders.Ascending
                ? query.OrderByDescending(p => p.PreferenceLevel).ThenBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.PreferenceLevel).ThenByDescending(p => p.CreatedAt);

            var data = await _repository.ToPreferenceDtoListAsync(
                query
                    .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                    .Take(queryParameters.PageSize),
                cancellationToken);

            _logger.LogInformation("Category preferences search for user {UserId} returned {Count} results", userId, data.Count);

            return Result<List<UserCategoryPreferenceDto>>.Success(data);
        }
    }
}
