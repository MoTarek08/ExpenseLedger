using Domain.Entities.RefreshTokenNamespace;

namespace Application.Interfaces.RepositoriesNamespace
{
    public interface IRefreshTokensRepository
    {       
        public void Add(RefreshToken refreshToken);

        public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

        public Task RevokeAllTokensForUsers(List<Guid> usersIds, DateTimeOffset revokingTime, CancellationToken cancellationToken);

    }
}
