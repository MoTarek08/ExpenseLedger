namespace Host.RateLimiters.RateLimitersSettings
{
    public sealed class RateLimitingOptions
    {
        public TokenBucketOptions AuthenticatedUser { get; init; } = new();
        public TokenBucketOptions Ip { get; init; } = new();
        public TokenBucketOptions UnknownIp { get; init; } = new();
        public FixedWindowOptions LoginIp { get; init; } = new();
        public TokenBucketOptions LoginUnknownIp { get; init; } = new();
        public TokenBucketOptions RefreshIp { get; init; } = new();
        public TokenBucketOptions RefreshUnknownIp { get; init; } = new();
        public FixedWindowOptions LogoutAuthenticatedUser { get; init; } = new();
        public FixedWindowOptions Upload { get; init; } = new();
        public ConcurrencyOptions LoginConcurrency { get; init; } = new();
        public ConcurrencyOptions UploadConcurrency { get; init; } = new();
        public ConcurrencyOptions RegisterConcurrency { get; init; } = new();
        public ConcurrencyOptions RefreshConcurrency { get; init; } = new();
    }
}
