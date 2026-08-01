using Application.ApplicationConstantsNamesapce;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.HashingService;
using Application.Interfaces.RefreshTokenSettings;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.TokensServiceNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Application.UseCases.AuthUseCases.Login;
using Application.UseCases.AuthUseCases.Login.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.RefreshTokenNamespace;
using Domain.Entities.UserNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.AuthUseCases.Login
{
    public class LoginUserUseCaseTests
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashingService _hashingService;
        private readonly ITokensService _tokensService;
        private readonly IDateProvider _dateTimeProvider;
        private readonly IRefreshTokenSettings _refreshTokenSettings;
        private readonly ILogger<LoginUserUseCase> _logger;

        private readonly LoginUserUseCase _sut;

        private readonly LoginRequestModel _request;

        public LoginUserUseCaseTests()
        {
            _usersRepository = A.Fake<IUsersRepository>();
            _refreshTokensRepository = A.Fake<IRefreshTokensRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _hashingService = A.Fake<IHashingService>();
            _tokensService = A.Fake<ITokensService>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _refreshTokenSettings = A.Fake<IRefreshTokenSettings>();
            _logger = A.Fake<ILogger<LoginUserUseCase>>();

            _sut = new LoginUserUseCase(
                _usersRepository,
                _refreshTokensRepository,
                _unitOfWork,
                _hashingService,
                _tokensService,
                _dateTimeProvider,
                _refreshTokenSettings,
                _logger);

            _request = new LoginRequestModel("user@test.com", "Password123!");
        }

        [Fact]
        public async Task Execute_WhenUserDoesNotExist_ShouldReturnInvalidCredentials()
        {
            A.CallTo(() => _usersRepository.FindByEmailAsync(_request.Email, A<CancellationToken>._))
                .Returns((User?)null);

            var result = await _sut.Execute(_request, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_INVALID_CREDENTIALS, result.Error!.Code);
            A.CallTo(() => _hashingService.AreEqual(
                ApplicationConstants.PlainTextDummyPassword,
                ApplicationConstants.HashedDummyPassword))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _tokensService.GenerateAccessToken(A<UserClaims>._, A<DateTime>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenPasswordIsIncorrect_ShouldReturnInvalidCredentials()
        {
            var user = User.Register(
                _request.Email,
                "HashedPassword",
                "Test",
                Role.User,
                DateTimeOffset.UtcNow);

            A.CallTo(() => _usersRepository.FindByEmailAsync(_request.Email, A<CancellationToken>._))
                .Returns(user);
            A.CallTo(() => _hashingService.AreEqual(_request.Password, user.PasswordHash))
                .Returns(false);

            var result = await _sut.Execute(_request, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_INVALID_CREDENTIALS, result.Error!.Code);
            A.CallTo(() => _hashingService.AreEqual(
                ApplicationConstants.PlainTextDummyPassword,
                ApplicationConstants.HashedDummyPassword))
                .MustNotHaveHappened();
            A.CallTo(() => _tokensService.GenerateAccessToken(A<UserClaims>._, A<DateTime>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenCredentialsAreValid_ShouldReturnTokensAndPersistRefreshToken()
        {
            var now = DateTimeOffset.UtcNow;
            const int lifetimeInDays = 7;
            const string accessToken = "access-token";
            const string refreshTokenValue = "refresh-token-value";

            var user = User.Register(
                _request.Email,
                "HashedPassword",
                "Test",
                Role.User,
                now.AddDays(-10));

            A.CallTo(() => _usersRepository.FindByEmailAsync(_request.Email, A<CancellationToken>._))
                .Returns(user);
            A.CallTo(() => _hashingService.AreEqual(_request.Password, user.PasswordHash))
                .Returns(true);
            A.CallTo(() => _dateTimeProvider.Now)
                .Returns(now);
            A.CallTo(() => _refreshTokenSettings.LifeTimeInDays)
                .Returns(lifetimeInDays);
            A.CallTo(() => _tokensService.GenerateAccessToken(A<UserClaims>._, now.UtcDateTime))
                .Returns(accessToken);
            A.CallTo(() => _tokensService.GenerateRefreshToken())
                .Returns(refreshTokenValue);

            RefreshToken? capturedRefreshToken = null;
            A.CallTo(() => _refreshTokensRepository.Add(A<RefreshToken>._))
                .Invokes(call => capturedRefreshToken = call.GetArgument<RefreshToken>(0));

            var result = await _sut.Execute(_request, default);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(accessToken, result.Data!.AccessToken);
            Assert.Equal(refreshTokenValue, result.Data.RefreshToken.Token);

            Assert.NotNull(capturedRefreshToken);
            Assert.Equal(user.Id, capturedRefreshToken!.UserId);
            Assert.Equal(refreshTokenValue, capturedRefreshToken.Token);
            Assert.Equal(now, capturedRefreshToken.CreatedAt);
            Assert.Equal(now.AddDays(lifetimeInDays), capturedRefreshToken.ExpiresAt);
            Assert.Null(capturedRefreshToken.RevokedAt);

            Assert.Equal(now, user.LastLoginAt);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenUserIsDeleted_ShouldReturnInvalidCredentials()
        {
            var user = User.Register(
                _request.Email,
                "HashedPassword",
                "Test",
                Role.User,
                DateTimeOffset.UtcNow);

            user.MarkAsDeleted(DateTimeOffset.UtcNow);

            A.CallTo(() => _usersRepository.FindByEmailAsync(_request.Email, A<CancellationToken>._))
                .Returns(user);

            var result = await _sut.Execute(_request, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_INVALID_CREDENTIALS, result.Error!.Code);
            A.CallTo(() => _hashingService.AreEqual(
                ApplicationConstants.PlainTextDummyPassword,
                ApplicationConstants.HashedDummyPassword))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _tokensService.GenerateAccessToken(A<UserClaims>._, A<DateTime>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}
