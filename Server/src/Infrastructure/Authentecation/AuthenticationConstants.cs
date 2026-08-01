using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Infrastructure.Authentecation
{
    public static class AuthenticationConstants
    {
        public const string AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
    }
}
