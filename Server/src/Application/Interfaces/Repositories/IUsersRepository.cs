using Domain.Entities.UserFinancialProfileNamespace;
using Domain.Entities.UserNamespace;

namespace Application.Interfaces.RepositoriesNamespace
{
    public interface IUsersRepository
    {
        public void Add(User user);
        public void AddFinancialProfile(UserFinancialProfile userFinancialProfile);

        public Task<User?> FindAsync(Guid id, CancellationToken cancellationToken);
        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

        public Task<UserFinancialProfile?> GetFinancialProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
