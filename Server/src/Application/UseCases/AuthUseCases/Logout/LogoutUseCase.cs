using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.AuthUseCases.Logout
{
    public class LogoutUseCase
    {
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly ILogger<LogoutUseCase> _logger;
        private IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;

        public LogoutUseCase(
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateProvider,
            ILogger<LogoutUseCase> logger)
        {
            _refreshTokensRepository = refreshTokensRepository;
            _unitOfWork = unitOfWork;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, string refreshTokenFromCookie, CancellationToken cancellationToken)
        {
            var refreshTokenRecord = await _refreshTokensRepository.GetByTokenAsync(refreshTokenFromCookie, cancellationToken);
            if (refreshTokenRecord is null)
                return Result.Failure(new Error(AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST));

            var now = _dateProvider.Now;

            if (refreshTokenRecord.RevokedAt.HasValue || refreshTokenRecord.IsExpiredIn(now))
            {
                _logger.LogInformation("Logout succeeded and refresh token {RefreshTokenId} is revoked (Refresh token was already revoked or expired)", refreshTokenRecord.Id);
                return Result.Success();
            }

            if (refreshTokenRecord.UserId != userId)
            {
                await _refreshTokensRepository.RevokeAllTokensForUsers(
                    new List<Guid> { refreshTokenRecord.UserId, userId }, now, cancellationToken);

                _logger.LogWarning("Logout succeeded BUT - access token user {AccessTokenUserId} does not match refresh token owner {RefreshTokenUserId}", userId, refreshTokenRecord.UserId);
                _logger.LogWarning("Revoking all refresh tokens for user {RTUserId} and {ATUserId} due to mismatch", refreshTokenRecord.UserId, userId);
                return Result.Success();
            }

            refreshTokenRecord.Revoke(now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Logout succeeded and refresh token {RefreshTokenId} is revoked for user {UserId}", refreshTokenRecord.Id,userId);
            return Result.Success();
        }
    }
}
