using System.Threading.RateLimiting;

namespace Host.RateLimiters.RateLimitersSettings
{
    public sealed record TokenBucketOptions
    {
        public int TokenLimit { get; init; }
        public int TokensPerPeriod { get; init; }
        public int ReplenishmentPeriodInSeconds { get; init; }
        public int QueueLimit { get; init; }
        public QueueProcessingOrder QueueProcessingOrder { get; init; }
        public bool AutoReplenishment { get; init; }
    }

}
