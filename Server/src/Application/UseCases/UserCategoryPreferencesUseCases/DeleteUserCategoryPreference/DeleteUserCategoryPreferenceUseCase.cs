using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UserCategoryPreferencesUseCases.DeleteUserCategoryPreference
{
    public class DeleteUserCategoryPreferenceUseCase
    {
        private readonly IUserCategoryPreferencesRepository _categoryPreferencesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteUserCategoryPreferenceUseCase> _logger;

        public DeleteUserCategoryPreferenceUseCase(
            IUserCategoryPreferencesRepository categoryPreferencesRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteUserCategoryPreferenceUseCase> logger)
        {
            _categoryPreferencesRepository = categoryPreferencesRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, Guid categoryId, CancellationToken cancellationToken)
        {
            var preference = await _categoryPreferencesRepository.FindAsync(userId, categoryId, cancellationToken);
            if (preference is null)
                return Result.Success();

            _categoryPreferencesRepository.Remove(preference);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User {UserId} deleted a category preference {CategoryId}", userId, categoryId);
            return Result.Success();
        }
    }
}
