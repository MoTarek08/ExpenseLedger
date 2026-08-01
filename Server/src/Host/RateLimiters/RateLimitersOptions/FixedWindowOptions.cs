namespace Host.RateLimiters.RateLimitersSettings
{
    public sealed record FixedWindowOptions
    {
        public int PermitLimit { get; init; }
        public int WindowInSeconds { get; init; }
        public bool AutoReplenishment { get; init; }
    }
}
