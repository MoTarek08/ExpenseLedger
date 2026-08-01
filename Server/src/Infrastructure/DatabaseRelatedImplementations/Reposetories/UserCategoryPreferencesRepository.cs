using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Domain.Entities.UserCategoryPreferenceNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class UserCategoryPreferencesRepository : IUserCategoryPreferencesRepository
    {
        private readonly AppDbContext _dbContext;

        public UserCategoryPreferencesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(UserCategoryPreference userCategoryPreference)
        {
            _dbContext.UserCategoryPreferences.Add(userCategoryPreference);
        }

        public void Remove(UserCategoryPreference userCategoryPreference)
        {
            _dbContext.UserCategoryPreferences.Remove(userCategoryPreference);
        }

        public async Task<UserCategoryPreference?> FindAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken)
        {
            return await _dbContext.UserCategoryPreferences.SingleOrDefaultAsync(x => x.UserId == userId && x.CategoryId == categoryId, cancellationToken);
        }

        public async Task<UserCategoryPreference?> FindIncludingCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserCategoryPreferences.Include(p => p.Category).SingleOrDefaultAsync(x => x.UserId == userId && x.CategoryId == categoryId, cancellationToken);
        }

        public IQueryable<UserCategoryPreference> GetAllForUserQuery(Guid userId)
        {
            return _dbContext.UserCategoryPreferences.Where(p => p.UserId == userId);
        }


        public async Task<List<UserCategoryPreferenceDto>> ToPreferenceDtoListAsync(
            IQueryable<UserCategoryPreference> query, CancellationToken cancellationToken)
        {
            return await query
                .Select(p => new UserCategoryPreferenceDto(
                    p.Category.Code,
                    p.Category.Name,
                    p.PreferenceLevel,
                    p.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}
