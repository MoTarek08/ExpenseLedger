using Application.Interfaces.DateTimeProvider;
using Application.Models;

namespace Infrastructure.DateTimeProviderNamespace
{
    public class DateProvider : IDateProvider
    {
        public DateTimeOffset Now => DateTimeOffset.UtcNow;

        public DateOnly Today => DateOnly.FromDateTime(Now.UtcDateTime);

        public DateOnly MinDayValue => DateConstants.MinDate;

        public DateOnly MaxDayValue => DateConstants.MaxDate;
    }
}
