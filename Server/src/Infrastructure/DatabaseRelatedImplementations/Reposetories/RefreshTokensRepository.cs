using Application.Interfaces.RepositoriesNamespace;
using Domain.Entities.RefreshTokenNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class RefreshTokensRepository : IRefreshTokensRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<RefreshTokensRepository> _logger;

        public RefreshTokensRepository(AppDbContext dbContext, ILogger<RefreshTokensRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public void Add(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Add(refreshToken);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
        }

        public async Task RevokeAllTokensForUsers(List<Guid> usersIds, DateTimeOffset revokingTime, CancellationToken cancellationToken)
        {
            await using var trans = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _dbContext.RefreshTokens
                    .Where(x => usersIds.Contains(x.UserId))
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(x => x.RevokedAt, revokingTime),
                        cancellationToken);

                await trans.CommitAsync(cancellationToken);
            }

            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Revoking refresh tokens for multiple users transaction failed");
            }
        }
    }
}
