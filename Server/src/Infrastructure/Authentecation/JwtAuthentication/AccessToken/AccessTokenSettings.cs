namespace Infrastructure.Authentecation.JwtAuthentication.AccessToken
{
    public sealed record AccessTokenSettings (string KeyId, string Issuer, string Audience, string SigningKey, int LifeTime);
}
