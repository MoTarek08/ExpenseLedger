using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.TokensServiceNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Application.Models.Result;
using Domain.Entities.DomainEnums;
using Domain.Entities.RefreshTokenNamespace;
using Application.Interfaces.RefreshTokenSettings;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
namespace Application.UseCases.AuthUseCases.RefreshTokensNamespace
{
    public class RefreshTokensUseCase
    {
        private readonly ITokensService _tokenService;
        private readonly IRefreshTokenSettings _refreshTokenSettings;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefreshTokensUseCase> _logger;
        private readonly IDateProvider _dateProvider;

        public RefreshTokensUseCase(
            ITokensService tokenService,
            IRefreshTokenSettings refreshTokenSettings,
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork,
            ILogger<RefreshTokensUseCase> logger,
            IDateProvider dateProvider)
        {
            _tokenService = tokenService;
            _refreshTokenSettings = refreshTokenSettings;
            _refreshTokensRepository = refreshTokensRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateProvider = dateProvider;
        }

        public async Task<Result<Tokens>> Execute(string accessToken, string refreshTokenFromCookie, CancellationToken cancellationToken)
        {
            var validateTokenResult = await _tokenService.ValidateAccessTokenAsync(accessToken);
            if (!validateTokenResult.IsValid)
            {
                _logger.LogWarning("Refresh failed - invalid access token");
                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN));
            }

            var refreshTokenRecord = await _refreshTokensRepository.GetByTokenAsync(refreshTokenFromCookie, cancellationToken);
            if (refreshTokenRecord is null)
            {
                _logger.LogWarning("Refresh failed - refresh token not found in DB");
                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST));
            }

            if (refreshTokenRecord.RevokedAt is not null)
            {
                _logger.LogWarning("Refresh failed - revoked token {RefreshTokenId}", refreshTokenRecord.Id);
                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_REVOKED_REFRESH_TOKEN));
            }

            var now = _dateProvider.Now;
            if (refreshTokenRecord.IsExpiredIn(now))
            {
                refreshTokenRecord.Revoke(now);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Refresh failed - expired token {RefreshTokenId}", refreshTokenRecord.Id);
                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_REFRESH_TOKEN_EXPIRED));
            }

            var userId = Guid.Parse(validateTokenResult.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (refreshTokenRecord.UserId != userId)
            {
                await _refreshTokensRepository.RevokeAllTokensForUsers(
                    new List<Guid> { refreshTokenRecord.UserId, userId }, now, cancellationToken);

                _logger.LogWarning("Refresh failed - access token user {AccessTokenUserId} does not match refresh token owner {RefreshTokenUserId}", userId, refreshTokenRecord.UserId);
                _logger.LogWarning("Revoking all refresh tokens for user {RTUserId} and {ATUserId} due to mismatch", refreshTokenRecord.UserId,userId);
                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_TOKENS_PAYLOAD_MISMATCH));
            }

            var roleString = validateTokenResult.ClaimsIdentity.FindFirst(ClaimTypes.Role)!.Value;

            if (!Enum.TryParse<Role>(roleString, out var role))
            {
                _logger.LogWarning("Refresh failed - invalid role claim {RoleClaim}", roleString);
                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN));
            }

            var userClaims = new UserClaims(userId, role);
            var newAccessToken = _tokenService.GenerateAccessToken(userClaims, now.UtcDateTime);

            var newRefreshToken = RefreshToken.Create(
                userId,
                refreshTokenRecord.SessionId,
                _tokenService.GenerateRefreshToken(),
                now,
                now.AddDays(_refreshTokenSettings.LifeTimeInDays));

            refreshTokenRecord.Revoke(now);

            _refreshTokensRepository.Add(newRefreshToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tokens refreshed {UserId} {RefreshTokenId}", userId, refreshTokenRecord.Id);

            return Result<Tokens>.Success(new Tokens(newAccessToken, newRefreshToken));
        }
    }
}
