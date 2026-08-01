using Application.Interfaces.RefreshTokenSettings;

namespace Infrastructure.Authentecation.JwtAuthentication.RefreshToken
{
    public sealed record RefreshTokenSettings(int LifeTimeInDays) : IRefreshTokenSettings;
}
