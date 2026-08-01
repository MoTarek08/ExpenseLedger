using Domain.Entities.RefreshTokenNamespace;

namespace Application.Models
{
    public sealed record Tokens(string AccessToken, RefreshToken RefreshToken);

}
