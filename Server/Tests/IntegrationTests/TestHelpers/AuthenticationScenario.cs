namespace IntegrationTests.TestHelpers;

public sealed record AuthenticationScenario(
    Guid UserId,
    string Email,
    string Password,
    string? AccessToken,
    string? RefreshTokenValue,
    Guid? RefreshTokenId);
