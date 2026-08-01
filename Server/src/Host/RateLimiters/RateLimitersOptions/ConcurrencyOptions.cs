using System.Threading.RateLimiting;

namespace Host.RateLimiters.RateLimitersSettings
{
    public sealed record ConcurrencyOptions
    {
        public int PermitLimit { get; init; }
        public int QueueLimit { get; init; }
        public QueueProcessingOrder QueueProcessingOrder { get; init; }
    }
}
