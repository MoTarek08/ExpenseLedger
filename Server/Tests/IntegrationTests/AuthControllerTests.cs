using Application.UseCases.AuthUseCases.Login.Models;
using Application.UseCases.AuthUseCases.Register.Models;
using Domain.Entities.DomainEnums;
using Host.Models;
using IntegrationTests.CustomWebApplicationFactoryNamespace;
using IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public class AuthControllerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public AuthControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Factory.CreateClient();
        }

        public async ValueTask InitializeAsync() => await _fixture.ResetAsync();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task Register_Success_ShouldReturn201AndCreateUser()
        {
            var email = $"test-{Guid.NewGuid()}@example.com";
            var request = new RegisterRequestModel(email, "Test User", "Password123!", "Password123!");

            var response = await _client.PostAsync("/api/v1/auth/register", JsonHelper.Serialize(request), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<CreatedResourceId<Guid>>(JsonHelper.Options, TestContext.Current.CancellationToken);
            Assert.NotEqual(Guid.Empty, body!.Id);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
                Assert.NotNull(user);
                Assert.Equal(email.ToLowerInvariant(), user.Email);
                Assert.NotEqual("Password123!", user.PasswordHash);
                Assert.Equal(Role.User, user.Role);
                Assert.Null(user.LastLoginAt);
            });
        }

        [Fact]
        public async Task Register_DuplicateEmail_ShouldReturn409()
        {
            var email = $"dup-{Guid.NewGuid()}@example.com";
            await AuthenticationScenarioBuilder.Create(_fixture)
                .WithEmail(email)
                .WithPassword("Password123!")
                .BuildAsync();

            var request = new RegisterRequestModel(email, "Another", "OtherPass1!", "OtherPass1!");

            var response = await _client.PostAsync("/api/v1/auth/register", JsonHelper.Serialize(request), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                var count = await db.Users.CountAsync(u => u.Email == email.ToLowerInvariant());
                Assert.Equal(1, count);
            });
        }

        [Fact]
        public async Task Register_InvalidInput_ShouldReturn400()
        {
            var request = new RegisterRequestModel("invalid", "", "weak", "mismatch");

            var response = await _client.PostAsync("/api/v1/auth/register", JsonHelper.Serialize(request), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                var count = await db.Users.CountAsync();
                Assert.Equal(0, count);
            });
        }

        [Fact]
        public async Task Login_Success_ShouldReturn200AndSetRefreshCookie()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithPassword("Password123!")
                .BuildAsync();

            var request = new LoginRequestModel(auth.Email, "Password123!");

            var response = await _client.PostAsync("/api/v1/auth/login", JsonHelper.Serialize(request), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<GeneratedAccessToken>(JsonHelper.Options, TestContext.Current.CancellationToken);
            Assert.NotNull(body!.AccessToken);
            Assert.NotEmpty(body.AccessToken);

            Assert.True(response.Headers.Contains("Set-Cookie"));
            var setCookie = response.Headers.GetValues("Set-Cookie").First();
            Assert.StartsWith("refreshToken=", setCookie);
            Assert.Contains("httponly", setCookie);
            Assert.Contains("secure", setCookie);
            Assert.Contains("samesite=strict", setCookie);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                var token = Assert.Single(await db.RefreshTokens.ToListAsync());
                Assert.Equal(auth.UserId, token.UserId);
                Assert.Null(token.RevokedAt);
                Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);

                var user = await db.Users.FirstAsync(u => u.Id == auth.UserId);
                Assert.NotNull(user.LastLoginAt);
            });
        }

        [Fact]
        public async Task Login_WrongPassword_ShouldReturn401()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithPassword("Password123!")
                .BuildAsync();

            var request = new LoginRequestModel(auth.Email, "WrongPassword1!");

            var response = await _client.PostAsync("/api/v1/auth/login", JsonHelper.Serialize(request), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                Assert.Empty(await db.RefreshTokens.ToListAsync());
            });
        }

        [Fact]
        public async Task Login_NonExistentUser_ShouldReturn401()
        {
            var request = new LoginRequestModel("nonexistent@example.com", "Password123!");

            var response = await _client.PostAsync("/api/v1/auth/login", JsonHelper.Serialize(request), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                Assert.Empty(await db.RefreshTokens.ToListAsync());
            });
        }

        [Fact]
        public async Task Refresh_Success_ShouldRotateTokens()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithRefreshToken()
                .BuildAsync();

            var oldRefreshTokenValue = auth.RefreshTokenValue!;
            var oldTokenId = auth.RefreshTokenId!.Value;

            var response = await _client.SendAsync(
                HttpRequestFactory.Post("/api/v1/auth/refresh")
                    .WithCookie("refreshToken", oldRefreshTokenValue)
                    .WithBearerToken(auth.AccessToken!), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<GeneratedAccessToken>(JsonHelper.Options, TestContext.Current.CancellationToken);
            Assert.NotNull(body!.AccessToken);
            Assert.NotEmpty(body.AccessToken);

            var newRefreshTokenValue = HttpRequestFactory.ExtractRefreshToken(response);
            Assert.NotEqual(oldRefreshTokenValue, newRefreshTokenValue);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                var oldToken = await db.RefreshTokens.FindAsync(oldTokenId);
                Assert.NotNull(oldToken!.RevokedAt);

                var tokens = await db.RefreshTokens.ToListAsync();
                Assert.Equal(2, tokens.Count);
                var activeToken = tokens.First(t => t.RevokedAt == null);
                Assert.Equal(auth.UserId, activeToken.UserId);
            });
        }

        [Fact]
        public async Task Refresh_MissingCookie_ShouldReturn401()
        {
            var response = await _client.SendAsync(
                HttpRequestFactory.Post("/api/v1/auth/refresh")
                    .WithBearerToken("some-token"), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task Refresh_MissingAuthHeader_ShouldReturn401()
        {
            var response = await _client.SendAsync(
                HttpRequestFactory.Post("/api/v1/auth/refresh")
                    .WithCookie("refreshToken", "some-token"), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Refresh_RevokedToken_ShouldReturn401()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithRevokedRefreshToken()
                .BuildAsync();

            var response = await _client.SendAsync(
                HttpRequestFactory.Post("/api/v1/auth/refresh")
                    .WithCookie("refreshToken", auth.RefreshTokenValue!)
                    .WithBearerToken(auth.AccessToken!), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                Assert.Single(await db.RefreshTokens.ToListAsync());
            });
        }

        [Fact]
        public async Task Refresh_TokenMismatch_ShouldReturn401()
        {
            var authA = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithRefreshToken()
                .BuildAsync();

            var authB = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithRefreshToken()
                .BuildAsync();

            var response = await _client.SendAsync(
                HttpRequestFactory.Post("/api/v1/auth/refresh")
                    .WithCookie("refreshToken", authA.RefreshTokenValue!)
                    .WithBearerToken(authB.AccessToken!), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                var revokedTokens = await db.RefreshTokens.Where(t => t.RevokedAt != null).ToListAsync();
                Assert.Equal(2, revokedTokens.Count);
            });
        }

        [Fact]
        public async Task Logout_Success_ShouldRevokeToken()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithRefreshToken()
                .BuildAsync();

            var response = await _client.SendAsync(
                HttpRequestFactory.Post("/api/v1/auth/logout")
                    .WithCookie("refreshToken", auth.RefreshTokenValue!)
                    .WithBearerToken(auth.AccessToken!), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            await DatabaseAssertions.Verify(_fixture, async db =>
            {
                var token = await db.RefreshTokens.FirstAsync();
                Assert.NotNull(token.RevokedAt);
            });
        }

        [Fact]
        public async Task Logout_Unauthenticated_ShouldReturn401()
        {
            var response = await _client.PostAsync("/api/v1/auth/logout", null, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_AlreadyRevokedToken_ShouldReturn204()
        {
            var auth = await AuthenticationScenarioBuilder.Create(_fixture)
                .WithRevokedRefreshToken()
                .BuildAsync();

            var response = await _client.SendAsync(
                HttpRequestFactory.Post("/api/v1/auth/logout")
                    .WithCookie("refreshToken", auth.RefreshTokenValue!)
                    .WithBearerToken(auth.AccessToken!), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
