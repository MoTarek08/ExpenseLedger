using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.AuthUseCases.Logout;
using Domain.Entities.RefreshTokenNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.AuthUseCases.Logout
{
    public class LogoutUseCaseTests
    {
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<LogoutUseCase> _logger;

        private readonly LogoutUseCase _sut;

        private const string RefreshTokenFromCookie = "refresh-token-value";

        public LogoutUseCaseTests()
        {
            _refreshTokensRepository = A.Fake<IRefreshTokensRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<LogoutUseCase>>();

            _sut = new LogoutUseCase(
                _refreshTokensRepository,
                _unitOfWork,
                _dateProvider,
                _logger);
        }

        [Fact]
        public async Task Execute_WhenRefreshTokenDoesNotExist_ShouldReturnDoesNotExistFailure()
        {
            var userId = Guid.NewGuid();

            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns((RefreshToken?)null);

            var result = await _sut.Execute(userId, RefreshTokenFromCookie, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST, result.Error!.Code);

            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenRefreshTokenIsAlreadyRevoked_ShouldReturnSuccess()
        {
            var now = DateTimeOffset.UtcNow;
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var revokedToken = RefreshToken.Create(
                userId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-3), now.AddDays(3));
            revokedToken.Revoke(now.AddMinutes(-1));

            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(revokedToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(userId, RefreshTokenFromCookie, default);

            Assert.True(result.IsSuccess);

            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenRefreshTokenIsExpired_ShouldReturnSuccess()
        {
            var now = DateTimeOffset.UtcNow;
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var expiredToken = RefreshToken.Create(
                userId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-10), now.AddSeconds(-1));

            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(expiredToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(userId, RefreshTokenFromCookie, default);

            Assert.True(result.IsSuccess);

            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenUserIdMismatchesTokenOwner_ShouldRevokeAllAndReturnSuccess()
        {
            var now = DateTimeOffset.UtcNow;
            var accessTokenUserId = Guid.NewGuid();
            var refreshTokenOwnerUserId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var refreshToken = RefreshToken.Create(
                refreshTokenOwnerUserId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-3), now.AddDays(3));

            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(refreshToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(accessTokenUserId, RefreshTokenFromCookie, default);

            Assert.True(result.IsSuccess);

            A.CallTo(() => _refreshTokensRepository.RevokeAllTokensForUsers(
                A<List<Guid>>.That.Matches(list => list.Contains(refreshTokenOwnerUserId) && list.Contains(accessTokenUserId)),
                now, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenValid_ShouldRevokeAndReturnSuccess()
        {
            var now = DateTimeOffset.UtcNow;
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var refreshToken = RefreshToken.Create(
                userId, sessionId, RefreshTokenFromCookie,
                now.AddDays(-3), now.AddDays(3));

            A.CallTo(() => _refreshTokensRepository.GetByTokenAsync(RefreshTokenFromCookie, A<CancellationToken>._))
                .Returns(refreshToken);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(userId, RefreshTokenFromCookie, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(now, refreshToken.RevokedAt);

            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}
