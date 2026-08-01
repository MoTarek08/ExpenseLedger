using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.TokensServiceNamespace;
using Application.Models;
using Infrastructure.Authentecation.JwtAuthentication.AccessToken;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Authentecation.JwtAuthentication
{
    public class TokensService : ITokensService
    {
        private readonly AccessTokenSettings _settings;
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly JsonWebTokenHandler _jwtHandler;


        public TokensService(
            JsonWebTokenHandler jwtHandler,
            AccessTokenSettings settings,
            TokenValidationParameters tokenValidationParams,
            IDateProvider dateTimeProvider)
        {
            _jwtHandler = jwtHandler;
            _settings = settings;
            _tokenValidationParams = tokenValidationParams;
        }


        public string GenerateAccessToken(UserClaims userClaims, DateTime timeStamp)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
            signingKey.KeyId = _settings.KeyId;

            var claimsIdentity = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.NameIdentifier,userClaims.Id.ToString()),
                    new(ClaimTypes.Role,userClaims.Role.ToString()),
                });

            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = claimsIdentity,
                SigningCredentials = signingCredentials,
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                IssuedAt = timeStamp,
                NotBefore = timeStamp,
                Expires = timeStamp.AddMinutes(_settings.LifeTime),
            };

            return _jwtHandler.CreateToken(tokenDescriptor);
        }



        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }


        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token)
        {
            return await _jwtHandler.ValidateTokenAsync(token, _tokenValidationParams);
        }
    }
}
