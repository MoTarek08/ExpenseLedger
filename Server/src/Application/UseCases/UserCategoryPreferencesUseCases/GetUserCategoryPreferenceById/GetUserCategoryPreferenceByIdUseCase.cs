using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UserCategoryPreferencesUseCases.GetUserCategoryPreferenceById
{
    public class GetUserCategoryPreferenceByIdUseCase
    {
        private readonly IUserCategoryPreferencesRepository _repository;
        private readonly ILogger<GetUserCategoryPreferenceByIdUseCase> _logger;

        public GetUserCategoryPreferenceByIdUseCase(
            IUserCategoryPreferencesRepository repository,
            ILogger<GetUserCategoryPreferenceByIdUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<UserCategoryPreferenceDto>> Execute(Guid userId, Guid categoryId, CancellationToken cancellationToken)
        {
            var preference = await _repository.FindIncludingCategoryAsync(userId, categoryId, cancellationToken);

            if (preference is null)
            {
                _logger.LogWarning("Category preference not found for user {UserId}, category {CategoryId}", userId, categoryId);
                return Result<UserCategoryPreferenceDto>.Failure(new Error(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND));
            }

            var dto = new UserCategoryPreferenceDto(
                preference.Category.Code,
                preference.Category.Name,
                preference.PreferenceLevel,
                preference.CreatedAt);

            return Result<UserCategoryPreferenceDto>.Success(dto);
        }
    }
}
