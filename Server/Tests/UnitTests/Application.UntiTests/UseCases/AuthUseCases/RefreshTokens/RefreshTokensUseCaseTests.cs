using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RefreshTokenSettings;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.TokensServiceNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Application.UseCases.AuthUseCases.RefreshTokensNamespace;
using Domain.Entities.DomainEnums;
using Domain.Entities.RefreshTokenNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace UnitTests.Application.UntiTests.UseCases.AuthUseCases.RefreshTokens
{
    public class RefreshTokensUseCaseTests
    {
        private readonly ITokensService _tokensService;
        private readonly IRefreshTokenSettings _refreshTokenSettings;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefreshTokensUseCase> _logger;
        private readonly IDateProvider _dateProvider;

        private readonly RefreshTokensUseCase _sut;

        private const string AccessToken = "access-token";
        private const string RefreshTokenFromCookie = "refresh-token-cookie";

        public RefreshTokensUseCaseTests()
        {
            _tokensService = A.Fake<ITokensService>();
            _refreshTokenSettings = A.Fake<IRefreshTokenSettings>();
            _refreshTokensRepository = A.Fake<IRefreshTokensRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<RefreshTokensUseCase>>();
            _dateProvider = A.Fake<IDateProvider>();

            _sut = new RefreshTokensUseCase(
                _tokensService,
                _refreshTokenSettings,
                _refreshTokensRepository,
                _unitOfWork,
                _logger,
                _dateProvider);
        }

        [Fact]
        public async Task Execute_WhenAccessTokenIsInvalid_ShouldReturnInvalidAccessTokenFailure()
        {
            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .Returns(new TokenValidationResult { IsValid = false });

            var result = await _sut.Execute(AccessToken, RefreshTokenFromCookie, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN, result.Error!.Code);

            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenRefreshTokenDoesNotExist_ShouldReturnDoesNotExistFailure()
        {
            var userId = Guid.NewGuid();

            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .Returns(ValidTokenValidationResult(userId, Role.User));
            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns((RefreshToken?)null);

            var result = await _sut.Execute(AccessToken, RefreshTokenFromCookie, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST, result.Error!.Code);

            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenRefreshTokenIsExpired_ShouldRevokeAndReturnExpiredFailure()
        {
            var now = DateTimeOffset.UtcNow;
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var expiredRefreshToken = RefreshToken.Create(
                userId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-10), now.AddSeconds(-1));

            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .Returns(ValidTokenValidationResult(userId, Role.User));
            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(expiredRefreshToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(AccessToken, RefreshTokenFromCookie, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_REFRESH_TOKEN_EXPIRED, result.Error!.Code);
            Assert.Equal(now, expiredRefreshToken.RevokedAt);

            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenRefreshTokenIsRevoked_ShouldReturnRevokedFailure()
        {
            var now = DateTimeOffset.UtcNow;
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var revokedRefreshToken = RefreshToken.Create(
                userId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-3), now.AddDays(3));

            revokedRefreshToken.Revoke(now.AddMinutes(-1));

            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .Returns(ValidTokenValidationResult(userId, Role.User));
            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(revokedRefreshToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(AccessToken, RefreshTokenFromCookie, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_REVOKED_REFRESH_TOKEN, result.Error!.Code);

            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenAccessTokenUserMismatchesRefreshTokenOwner_ShouldRevokeAllAndReturnMismatchFailure()
        {
            var now = DateTimeOffset.UtcNow;
            var accessTokenUserId = Guid.NewGuid();
            var refreshTokenOwnerUserId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var refreshToken = RefreshToken.Create(
                refreshTokenOwnerUserId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-3), now.AddDays(3));

            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .Returns(ValidTokenValidationResult(accessTokenUserId, Role.User));
            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(refreshToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(AccessToken, RefreshTokenFromCookie, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_TOKENS_PAYLOAD_MISMATCH, result.Error!.Code);

            A.CallTo(() => _refreshTokensRepository.RevokeAllTokensForUsers(
                A<List<Guid>>.That.Matches(list => list.Contains(refreshTokenOwnerUserId) && list.Contains(accessTokenUserId)),
                now, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenRoleClaimIsInvalid_ShouldReturnInvalidAccessTokenFailure()
        {
            var now = DateTimeOffset.UtcNow;
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var refreshToken = RefreshToken.Create(
                userId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-1), now.AddDays(1));

            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .Returns(ValidTokenValidationResult(userId, "NotARole"));
            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(refreshToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(AccessToken, RefreshTokenFromCookie, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN, result.Error!.Code);

            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _refreshTokensRepository.RevokeAllTokensForUsers(A<List<Guid>>._, A<DateTimeOffset>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenRefreshIsValid_ShouldRotateTokenAndReturnSuccess()
        {
            var now = DateTimeOffset.UtcNow;
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            const int lifetimeInDays = 7;
            const string newAccessToken = "new-access-token";
            const string newRefreshTokenValue = "new-refresh-token";

            var existingRefreshToken = RefreshToken.Create(
                userId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-3), now.AddDays(3));

            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .Returns(ValidTokenValidationResult(userId, Role.User));
            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(existingRefreshToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);
            A.CallTo(() => _refreshTokenSettings.LifeTimeInDays)
                .Returns(lifetimeInDays);

            UserClaims? capturedClaims = null;
            A.CallTo(() => _tokensService.GenerateAccessToken(A<UserClaims>._, now.UtcDateTime))
                .Invokes(call => capturedClaims = call.GetArgument<UserClaims>(0))
                .Returns(newAccessToken);

            A.CallTo(() => _tokensService.GenerateRefreshToken())
                .Returns(newRefreshTokenValue);

            RefreshToken? addedRefreshToken = null;
            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .Invokes(call => addedRefreshToken = call.GetArgument<RefreshToken>(0));

            var result = await _sut.Execute(AccessToken, RefreshTokenFromCookie, default);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(newAccessToken, result.Data!.AccessToken);

            Assert.NotNull(capturedClaims);
            Assert.Equal(userId, capturedClaims!.Id);
            Assert.Equal(Role.User, capturedClaims.Role);

            Assert.NotNull(addedRefreshToken);
            Assert.Equal(userId, addedRefreshToken!.UserId);
            Assert.Equal(sessionId, addedRefreshToken.SessionId);
            Assert.Equal(newRefreshTokenValue, addedRefreshToken.Token);
            Assert.Equal(now, addedRefreshToken.CreatedAt);
            Assert.Equal(now.AddDays(lifetimeInDays), addedRefreshToken.ExpiresAt);
            Assert.Null(addedRefreshToken.RevokedAt);

            Assert.Equal(now, existingRefreshToken.RevokedAt);

            A.CallTo(() => _tokensService.ValidateAccessTokenAsync(AccessToken))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _tokensService.GenerateAccessToken(A<UserClaims>._, now.UtcDateTime))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _tokensService.GenerateRefreshToken())
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        private static TokenValidationResult ValidTokenValidationResult(Guid userId, Role role)
        {
            return new TokenValidationResult
            {
                IsValid = true,
                ClaimsIdentity = new ClaimsIdentity(
                    new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, userId.ToString()),
                        new(ClaimTypes.Role, role.ToString())
                    })
            };
        }

        private static TokenValidationResult ValidTokenValidationResult(Guid userId, string role)
        {
            return new TokenValidationResult
            {
                IsValid = true,
                ClaimsIdentity = new ClaimsIdentity(
                    new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, userId.ToString()),
                        new(ClaimTypes.Role, role)
                    })
            };
        }
    }
}
