using Application.Models;
using Microsoft.IdentityModel.Tokens;

namespace Application.Interfaces.TokensServiceNamespace
{
    public interface ITokensService
    {

        public string GenerateAccessToken(UserClaims userClaims, DateTime timeStamp);
        public string GenerateRefreshToken();
        public Task<TokenValidationResult> ValidateAccessTokenAsync(string token);

    }
}