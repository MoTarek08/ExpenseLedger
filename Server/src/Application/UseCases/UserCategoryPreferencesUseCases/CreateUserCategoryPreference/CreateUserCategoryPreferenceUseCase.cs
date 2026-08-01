using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference.Models;
using Domain.Entities.UserCategoryPreferenceNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference
{
    public class CreateUserCategoryPreferenceUseCase
    {
        private IUserCategoryPreferencesRepository _userCategoryPreferencesRepository;
        private ICategoriesRepository _categoriesRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateUserCategoryPreferenceUseCase> _logger;

        public CreateUserCategoryPreferenceUseCase(
            IUserCategoryPreferencesRepository userCategoryPreferencesRepository,
            ICategoriesRepository categoriesRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateProvider,
            ILogger<CreateUserCategoryPreferenceUseCase> logger)
        {
            _userCategoryPreferencesRepository = userCategoryPreferencesRepository;
            _categoriesRepository = categoriesRepository;
            _unitOfWork = unitOfWork;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId,
            CreateCategoryPreferenceRequestModel requestModel,
            CancellationToken cancellationToken)
        {
            var category = await _categoriesRepository.FindAsync(requestModel.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Create category preference failed — category not found {CategoryId}", requestModel.CategoryId);
                return Result.Failure(new Error(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND));
            }

            var existing = await _userCategoryPreferencesRepository.FindAsync(userId, requestModel.CategoryId, cancellationToken);
            if (existing is not null)
            {
                _logger.LogWarning("Create category preference failed — already exists {UserId} {CategoryId}", userId, requestModel.CategoryId);
                return Result.Failure(new Error(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_ALREADY_EXISTS));
            }

            var preference = UserCategoryPreference.Create(userId, category.Id, requestModel.PreferenceLevel, _dateProvider.Now);
            _userCategoryPreferencesRepository.Add(preference);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category preference created {UserId} {CategoryId}", userId, requestModel.CategoryId);

            return Result.Success();
        }
    }
}
