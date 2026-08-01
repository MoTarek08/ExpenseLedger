using Application.Interfaces.RepositoriesNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using Domain.Entities.UserNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly AppDbContext _dbContext;

        public UsersRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        }

        public void Add(User user)
        {
            _dbContext.Users.Add(user);
        }

        public void AddFinancialProfile(UserFinancialProfile userFinancialProfile)
        {
            _dbContext.UserFinancialProfiles.Add(userFinancialProfile);
        }

        public async Task<UserFinancialProfile?> GetFinancialProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserFinancialProfiles.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.ToListAsync(cancellationToken);
        }

        public async Task<User?> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
