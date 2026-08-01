using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Domain.Entities.UserCategoryPreferenceNamespace;

namespace Application.Interfaces.RepositoriesNamespace
{
    public interface IUserCategoryPreferencesRepository
    {
        public void Add(UserCategoryPreference userCategoryPreference);
        public void Remove(UserCategoryPreference userCategoryPreference);

        public Task<UserCategoryPreference?> FindAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken);
        public Task<UserCategoryPreference?> FindIncludingCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);

        public IQueryable<UserCategoryPreference> GetAllForUserQuery(Guid userId);
        public Task<List<UserCategoryPreferenceDto>> ToPreferenceDtoListAsync(
            IQueryable<UserCategoryPreference> query, CancellationToken cancellationToken);
    }
}
