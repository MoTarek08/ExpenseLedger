using Application.ApplicationConstantsNamesapce;
using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.HashingService;
using Application.Interfaces.RefreshTokenSettings;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.TokensServiceNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Application.Models.Result;
using Application.UseCases.AuthUseCases.Login.Models;
using Domain.Entities.RefreshTokenNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.AuthUseCases.Login
{
    public class LoginUserUseCase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashingService _hashingService;
        private readonly ITokensService _tokenService;
        private readonly IDateProvider _dateProvider;
        private readonly IRefreshTokenSettings _refreshTokenSettings;
        private readonly ILogger<LoginUserUseCase> _logger;

        public LoginUserUseCase(
            IUsersRepository usersRepository,
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork,
            IHashingService hashingService,
            ITokensService tokenService,
            IDateProvider dateProvider,
            IRefreshTokenSettings refreshTokenSettings,
            ILogger<LoginUserUseCase> logger)
        {
            _usersRepository = usersRepository;
            _refreshTokensRepository = refreshTokensRepository;
            _unitOfWork = unitOfWork;
            _hashingService = hashingService;
            _tokenService = tokenService;
            _dateProvider = dateProvider;
            _refreshTokenSettings = refreshTokenSettings;
            _logger = logger;
        }

        public async Task<Result<Tokens>> Execute(LoginRequestModel loginRequestModel, CancellationToken cancellationToken)
        {
            var lowerCasedEmail = loginRequestModel.Email.Trim().ToLowerInvariant();
            var user = await _usersRepository.FindByEmailAsync(lowerCasedEmail, cancellationToken);
            if (user is null || user.DeletedAt is not null)
            {
                _hashingService.AreEqual(
                    ApplicationConstants.PlainTextDummyPassword,
                    ApplicationConstants.HashedDummyPassword);

                _logger.LogWarning(
                    "Login failed - invalid credentials {Email}",
                    lowerCasedEmail);

                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_INVALID_CREDENTIALS));
            }

            var passwordMatch = _hashingService.AreEqual(loginRequestModel.Password.Trim(), user.PasswordHash);
            if (!passwordMatch)
            {
                _logger.LogWarning(
                    "Login failed - invalid credentials {Email}",
                    lowerCasedEmail);

                return Result<Tokens>.Failure(new Error(AuthErrorCodes.AUTH_INVALID_CREDENTIALS));
            }

            var userClaims = new UserClaims(user.Id, user.Role);

            var now = _dateProvider.Now;

            var accessToken = _tokenService.GenerateAccessToken(userClaims,now.UtcDateTime);

            var refreshToken = RefreshToken.Create(
                user.Id,
                Guid.NewGuid(),
                _tokenService.GenerateRefreshToken(),
                now,
                now.AddDays(_refreshTokenSettings.LifeTimeInDays));

            _refreshTokensRepository.Add(refreshToken);

            user.MarkAsLoggedIn(now);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Login succeeded {UserId} {Email}",
                user.Id,
                lowerCasedEmail);

            return Result<Tokens>.Success(new Tokens(accessToken, refreshToken));
        }
    }
}
