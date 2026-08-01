using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Models.Result;
using Infrastructure.Authentecation;
using System.Net.Http.Headers;

namespace Host.Controllers.AuthController.Helpers
{
    public static class AuthHeaderValidator
    {
        public static Result<string> Validate(HttpRequest request)
        {
            var invalidAuthHeaderResult = Result<string>.Failure(new Error(AuthErrorCodes.AUTH_INVALID_AUTHORIZATION_HEADER));
            if (!AuthenticationHeaderValue.TryParse(request.Headers.Authorization, out var authHeader))
                return invalidAuthHeaderResult;

            if (!string.Equals(authHeader.Scheme,AuthenticationConstants.AuthenticationScheme,StringComparison.OrdinalIgnoreCase))
                return invalidAuthHeaderResult;

            if (string.IsNullOrWhiteSpace(authHeader.Parameter))
                return invalidAuthHeaderResult;

            return Result<string>.Success(authHeader.Parameter);
        }
    }
}
