using Application.ApplicationConstantsNamesapce;
using Application.Interfaces.HashingService;
using Application.Interfaces.RefreshTokenSettings;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.TokensServiceNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.RefreshTokenNamespace;
using Domain.Entities.UserNamespace;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.TestHelpers;

public class AuthenticationScenarioBuilder
{
    private readonly IntegrationTestFixture _fixture;

    private string _email = $"auth-{Guid.NewGuid()}@test.com";
    private string _password = "Password123!";
    private string _displayName = "Test User";
    private bool _isAdmin;
    private bool _withRefreshToken;
    private bool _revokeRefreshToken;

    private AuthenticationScenarioBuilder(IntegrationTestFixture fixture) => _fixture = fixture;

    public static AuthenticationScenarioBuilder Create(IntegrationTestFixture fixture) => new(fixture);

    public AuthenticationScenarioBuilder WithEmail(string email) { _email = email; return this; }
    public AuthenticationScenarioBuilder WithPassword(string password) { _password = password; return this; }
    public AuthenticationScenarioBuilder WithRefreshToken() { _withRefreshToken = true; return this; }
    public AuthenticationScenarioBuilder WithRevokedRefreshToken() { _withRefreshToken = true; _revokeRefreshToken = true; return this; }
    public AuthenticationScenarioBuilder AsAdmin() { _isAdmin = true; return this; }

    public async Task<AuthenticationScenario> BuildAsync()
    {
        var now = DateTimeOffset.UtcNow;

        using var scope = _fixture.Factory.CreateScope();
        var sp = scope.ServiceProvider;

        var hashingService = sp.GetRequiredService<IHashingService>();
        var usersRepo = sp.GetRequiredService<IUsersRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var hashedPassword = hashingService.Hash(_password, ApplicationConstants.HashingWorkFactor);
        var role = _isAdmin ? Role.Admin : Role.User;
        var user = User.Register(_email, hashedPassword, _displayName, role, now);
        usersRepo.Add(user);

        string? accessToken = null;
        string? refreshTokenValue = null;
        Guid? refreshTokenId = null;

        if (_withRefreshToken || _revokeRefreshToken)
        {
            var tokenService = sp.GetRequiredService<ITokensService>();
            var refreshTokensRepo = sp.GetRequiredService<IRefreshTokensRepository>();
            var refreshTokenSettings = sp.GetRequiredService<IRefreshTokenSettings>();

            var sessionId = Guid.NewGuid();
            accessToken = tokenService.GenerateAccessToken(
                new UserClaims(user.Id, role), now.UtcDateTime);
            refreshTokenValue = tokenService.GenerateRefreshToken();

            var refreshToken = RefreshToken.Create(
                user.Id,
                sessionId,
                refreshTokenValue,
                now,
                now.AddDays(refreshTokenSettings.LifeTimeInDays));

            if (_revokeRefreshToken)
                refreshToken.Revoke(now);

            refreshTokensRepo.Add(refreshToken);
            refreshTokenId = refreshToken.Id;
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        return new AuthenticationScenario(user.Id, _email, _password, accessToken, refreshTokenValue, refreshTokenId);
    }
}
