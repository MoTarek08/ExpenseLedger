using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.UseCases.AuthUseCases.Login;
using Application.UseCases.AuthUseCases.Login.Models;
using Application.UseCases.AuthUseCases.Logout;
using Application.UseCases.AuthUseCases.RefreshTokensNamespace;
using Application.UseCases.AuthUseCases.Register;
using Application.UseCases.AuthUseCases.Register.Models;
using Asp.Versioning;
using Host.Attributes;
using Host.Controllers.AuthController.Helpers;
using Host.Controllers.ControllersExtenstions;
using Host.Models;
using Host.ProblemDetails.Problems;
using Host.RateLimiters;
using Host.Swagger.ResponsesExamples;
using Host.Validation.ValidatorsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.AuthNamespace
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RefreshTokensUseCase _refreshTokensUseCase;
        private readonly RegisterUseCase _registerUseCase;
        private readonly LoginUserUseCase _loginUseCase;
        private readonly LogoutUseCase _logoutUseCase;

        public AuthController(
            RefreshTokensUseCase refreshTokensUseCase,
            RegisterUseCase registerUseCase,
            LoginUserUseCase loginUseCase,
            LogoutUseCase logoutUseCase)
        {
            _refreshTokensUseCase = refreshTokensUseCase;
            _registerUseCase = registerUseCase;
            _loginUseCase = loginUseCase;
            _logoutUseCase = logoutUseCase;
        }


        ///<summary>
        /// Register a new user account
        ///</summary>
        ///<remarks>
        /// Creates a new user record with the provided email, display name, and password.
        ///</remarks>
        [HttpPost]
        [Route("register")]
        [EnableRateLimiting(RateLimitingPolicies.ConcurrentRegister)]
        [SwaggerRequestExample(typeof(RegisterRequestModel), typeof(RegisterRequestModelExample))]
        [SwaggerResponse(201, Type = typeof(CreatedResourceId<Guid>))]
        [SwaggerResponseExample(201, typeof(CreatedResourceIdGuidExample))]
        [ProducesError(AuthErrorCodes.AUTH_EMAIL_ALREADY_EXISTS)]
        public async Task<ActionResult<CreatedResourceId<Guid>>> Register(
            RegisterRequestModel registerRequestModel,
            [FromServices] RegisterRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(registerRequestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var result = await _registerUseCase.Execute(registerRequestModel, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Created((Uri?)null, new CreatedResourceId<Guid>(result.Data));
        }

        ///<summary>
        /// Authenticate user and generate tokens
        ///</summary>
        ///<remarks>
        /// Validates the user's credentials and generates a JWT access token and a refresh token.
        ///</remarks>
        [HttpPost]
        [Route("login")]
        [EnableRateLimiting(RateLimitingPolicies.Login)]
        [SwaggerRequestExample(typeof(LoginRequestModel), typeof(LoginRequestModelExample))]
        [SwaggerResponse(200, Type = typeof(GeneratedAccessToken))]
        [SwaggerResponseExample(200, typeof(GeneratedAccessTokenExample))]
        [ProducesError(AuthErrorCodes.AUTH_INVALID_CREDENTIALS)]
        public async Task<ActionResult<GeneratedAccessToken>> Login(
            LoginRequestModel loginRequestModel,
            [FromServices] LoginRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(loginRequestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var result = await _loginUseCase.Execute(loginRequestModel, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            Response.Cookies.Append(
                "refreshToken",
                result.Data!.RefreshToken.Token,
                new CookieOptions()
                {
                    SameSite = SameSiteMode.Strict,
                    Secure = true,
                    HttpOnly = true,
                    Expires = result.Data.RefreshToken.ExpiresAt
                });

            return Ok(new GeneratedAccessToken(result.Data.AccessToken));
        }



        ///<summary>
        /// Refresh authentication tokens
        ///</summary>
        ///<remarks>
        /// Validates the refresh token from the HttpOnly cookie and the access token from the Authorization header,
        /// then issues a new token pair. The refresh token is rotated — the old one is revoked and a new one is
        /// attached as an HttpOnly secure cookie. This endpoint is unauthenticated because the access token may
        /// be expired; validation is performed manually.
        ///</remarks>
        [HttpPost("refresh")]
        [EnableRateLimiting(RateLimitingPolicies.RefreshTokens)]
        [SwaggerResponse(200,Type = typeof(GeneratedAccessToken))]
        [SwaggerResponseExample(200,typeof(GeneratedAccessTokenExample))]
        [ProducesError(AuthErrorCodes.AUTH_REFRESH_TOKEN_MISSING)]
        [ProducesError(AuthErrorCodes.AUTH_INVALID_AUTHORIZATION_HEADER)]
        [ProducesError(AuthErrorCodes.AUTH_INVALID_ACCESS_TOKEN)]
        [ProducesError(AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST)]
        [ProducesError(AuthErrorCodes.AUTH_REFRESH_TOKEN_EXPIRED)]
        [ProducesError(AuthErrorCodes.AUTH_REVOKED_REFRESH_TOKEN)]
        [ProducesError(AuthErrorCodes.AUTH_TOKENS_PAYLOAD_MISMATCH)]
        public async Task<ActionResult<GeneratedAccessToken>> Refresh(CancellationToken cancellationToken)
        {
            var isRefreshToken = Request.Cookies.TryGetValue("refreshToken", out var refreshTokenFromCookie);
            if (!isRefreshToken || string.IsNullOrWhiteSpace(refreshTokenFromCookie))
                return this.FromProblem(AllProblems.Get(AuthErrorCodes.AUTH_REFRESH_TOKEN_MISSING));

            var authHeaderValidationResult = AuthHeaderValidator.Validate(Request);
            if (authHeaderValidationResult.IsFailure)
                return this.FromProblem(AllProblems.Get(authHeaderValidationResult.Error!.Code));

            var accessToken = authHeaderValidationResult.Data!;
            var result = await _refreshTokensUseCase.Execute(accessToken, refreshTokenFromCookie, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            Response.Cookies.Append(
                "refreshToken",
                result.Data!.RefreshToken.Token,
                new CookieOptions
                { 
                    SameSite = SameSiteMode.Strict,
                    Secure = true, HttpOnly = true, 
                    Expires = result.Data.RefreshToken.ExpiresAt 
                }
            );
            return Ok(new GeneratedAccessToken(result.Data.AccessToken));
        }
        /// <summary>
        /// Logout the authenticated user 
        /// </summary>
        /// <remarks>
        /// Idempotent. Revokes only the current user's session while other sessions are not affected
        /// </remarks>
        [HttpPost("logout")]
        [Authorize]
        [SwaggerResponse(204)]
        [ProducesError(AuthErrorCodes.AUTH_REFRESH_TOKEN_MISSING)]
        [ProducesError(AuthErrorCodes.AUTH_REFRESH_TOKEN_DOES_NOT_EXIST)]

        public async Task<ActionResult> Logout(CancellationToken cancellationToken)
        {
            var isRefreshToken = Request.Cookies.TryGetValue("refreshToken", out var refreshTokenFromCookie);
            if (!isRefreshToken || string.IsNullOrWhiteSpace(refreshTokenFromCookie))
                return this.FromProblem(AllProblems.Get(AuthErrorCodes.AUTH_REFRESH_TOKEN_MISSING));

            var userId = this.GetUserIdFromClaims();
            var result = await _logoutUseCase.Execute(userId, refreshTokenFromCookie, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                SameSite = SameSiteMode.Strict,
                Secure = true,
                HttpOnly = true
            });

            return NoContent();
        }

    }
}

            
            
            
