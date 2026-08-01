using Host.RateLimiters.Factories;
using Host.RateLimiters.RateLimitersSettings;
using Serilog;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Host.RateLimiters
{
    public static class RateLimitersRegistrationExtensions
    {
        public static IServiceCollection AddRateLimitersExtenstion(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var rateLimitOptions = configuration.GetSection("RateLimiting").Get<RateLimitingOptions>();
            if (rateLimitOptions is null)
            {
                Log.Logger.Error("Failed to load rate limiting settings");
                return services;
            }

            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                    var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? null;
                    Log.Logger.Warning("Limit was hit for IP {IP} in path {Path} (UserId: {UserId})", ip, context.HttpContext.Request.Path, userId);
                    await context.HttpContext.Response.CompleteAsync();
                };

                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    {
                        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (userId is not null)
                            return RateLimitPartition.GetTokenBucketLimiter(userId,
                                _ => RateLimitersFactory.Create(rateLimitOptions.AuthenticatedUser));

                        var ip = context.Connection.RemoteIpAddress?.ToString();
                        if (ip is not null)
                            return RateLimitPartition.GetTokenBucketLimiter(ip,
                                _ => RateLimitersFactory.Create(rateLimitOptions.Ip));

                        return RateLimitPartition.GetTokenBucketLimiter("unknown-ip",
                            _ => RateLimitersFactory.Create(rateLimitOptions.UnknownIp));
                    }),
                    PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    {
                        var path = context.Request.Path.Value;
                        if (path is not null)
                        {
                            if (path.EndsWith("/login", StringComparison.OrdinalIgnoreCase))
                                return RateLimitPartition.GetConcurrencyLimiter(RateLimitingPolicies.ConcurrentLogin,
                                    _ => RateLimitersFactory.Create(rateLimitOptions.LoginConcurrency));

                            if (path.EndsWith("/register", StringComparison.OrdinalIgnoreCase))
                                return RateLimitPartition.GetConcurrencyLimiter(RateLimitingPolicies.ConcurrentRegister,
                                    _ => RateLimitersFactory.Create(rateLimitOptions.RegisterConcurrency));

                            if (path.Contains("/upload", StringComparison.OrdinalIgnoreCase))
                                return RateLimitPartition.GetConcurrencyLimiter(RateLimitingPolicies.ConcurrentUpload,
                                    _ => RateLimitersFactory.Create(rateLimitOptions.UploadConcurrency));

                            if (path.EndsWith("/refresh", StringComparison.OrdinalIgnoreCase))
                                return RateLimitPartition.GetConcurrencyLimiter(RateLimitingPolicies.ConcurrentRefreshTokens,
                                    _ => RateLimitersFactory.Create(rateLimitOptions.RefreshConcurrency));
                        }
                        return RateLimitPartition.GetNoLimiter("no-limit");
                    }));

                options.AddPolicy(RateLimitingPolicies.Login, context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString();
                    if (ip is not null)
                        return RateLimitPartition.GetFixedWindowLimiter(ip,
                            _ => RateLimitersFactory.Create(rateLimitOptions.LoginIp));

                    return RateLimitPartition.GetTokenBucketLimiter("unknown-ip",
                        _ => RateLimitersFactory.Create(rateLimitOptions.LoginUnknownIp));
                });

                options.AddPolicy(RateLimitingPolicies.Logout, context =>
                {
                    var userId = context.Request.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                        return RateLimitPartition.GetFixedWindowLimiter(userId,
                            _ => RateLimitersFactory.Create(rateLimitOptions.LogoutAuthenticatedUser));
                });

                options.AddPolicy(RateLimitingPolicies.Upload, context =>
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    return RateLimitPartition.GetFixedWindowLimiter(userId,
                        _ => RateLimitersFactory.Create(rateLimitOptions.Upload));
                });

                options.AddPolicy(RateLimitingPolicies.ConcurrentRegister, context =>
                {
                    return RateLimitPartition.GetConcurrencyLimiter(RateLimitingPolicies.ConcurrentRegister,
                        _ => RateLimitersFactory.Create(rateLimitOptions.RegisterConcurrency));
                });

                options.AddPolicy(RateLimitingPolicies.RefreshTokens, context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString();
                    if (ip is not null)
                        return RateLimitPartition.GetTokenBucketLimiter(ip,
                            _ => RateLimitersFactory.Create(rateLimitOptions.RefreshIp));

                    return RateLimitPartition.GetTokenBucketLimiter("unknown-ip",
                        _ => RateLimitersFactory.Create(rateLimitOptions.RefreshUnknownIp));
                });
            });

            return services;
        }
    }
}
