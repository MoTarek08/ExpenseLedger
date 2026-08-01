using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference
{
    public class UpdateUserCategoryPrefereneUseCase
    {
        private readonly IUserCategoryPreferencesRepository _userCategoryPreferencesRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserCategoryPrefereneUseCase> _logger;

        public UpdateUserCategoryPrefereneUseCase(
            IUserCategoryPreferencesRepository userCategoryPreferencesRepository,
            ICategoriesRepository categoriesRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateUserCategoryPrefereneUseCase> logger)
        {
            _userCategoryPreferencesRepository = userCategoryPreferencesRepository;
            _categoriesRepository = categoriesRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<UpdateUserCategoryPrefereneResponseModel>> Execute(
            Guid userId,
            UpdateCategoryPreferenceRequestModel requestModel,
            CancellationToken cancellationToken)
        {
            var category = await _categoriesRepository.FindAsync(requestModel.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Update category preference failed — category not found {CategoryId}", requestModel.CategoryId);
                return Result<UpdateUserCategoryPrefereneResponseModel>.Failure(new Error(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND));
            }

            var preference = await _userCategoryPreferencesRepository.FindAsync(userId, category.Id, cancellationToken);
            if (preference is null)
            {
                _logger.LogWarning("Update category preference failed — preference not found {UserId} {CategoryId}", userId, requestModel.CategoryId);
                return Result<UpdateUserCategoryPrefereneResponseModel>.Failure(new Error(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND));
            }

            var oldPreferenceLevel = preference.PreferenceLevel;
            preference.ChangePreferenceLevel(requestModel.PreferenceLevel);

            if (oldPreferenceLevel != preference.PreferenceLevel)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category preference updated {UserId} {CategoryId} {OldLevel} {NewLevel}",
                userId, requestModel.CategoryId, oldPreferenceLevel, preference.PreferenceLevel);

            return Result<UpdateUserCategoryPrefereneResponseModel>.Success(
                new UpdateUserCategoryPrefereneResponseModel(
                    userId,
                    requestModel.CategoryId,
                    oldPreferenceLevel,
                    preference.PreferenceLevel));
        }
    }
}
