using Host.RateLimiters.RateLimitersSettings;
using System.Threading.RateLimiting;

namespace Host.RateLimiters.Factories
{
    public static class RateLimitersFactory
    {
        public static TokenBucketRateLimiterOptions Create(TokenBucketOptions settings)
        {
            return new()
            {
                TokenLimit = settings.TokenLimit,
                TokensPerPeriod = settings.TokensPerPeriod,
                ReplenishmentPeriod = TimeSpan.FromSeconds(settings.ReplenishmentPeriodInSeconds),
                QueueLimit = settings.QueueLimit,
                QueueProcessingOrder = settings.QueueProcessingOrder,
                AutoReplenishment = settings.AutoReplenishment,
            };
        }

        public static FixedWindowRateLimiterOptions Create(FixedWindowOptions settings)
        {
            return new()
            {
                PermitLimit = settings.PermitLimit,
                Window = TimeSpan.FromSeconds(settings.WindowInSeconds),
                AutoReplenishment = settings.AutoReplenishment,
            };
        }

        public static ConcurrencyLimiterOptions Create(ConcurrencyOptions settings)
        {
            return new()
            {
                PermitLimit = settings.PermitLimit,
                QueueLimit = settings.QueueLimit,
                QueueProcessingOrder = settings.QueueProcessingOrder,
            };
        }
    }
}
