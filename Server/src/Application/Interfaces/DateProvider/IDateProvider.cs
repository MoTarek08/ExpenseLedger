namespace Application.Interfaces.DateTimeProvider
{
    public interface IDateProvider
    {
        public DateTimeOffset Now { get; }
        public DateOnly Today { get; }
        public DateOnly MinDayValue { get; }
        public DateOnly MaxDayValue { get; }
    }
}