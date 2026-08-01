using Domain.Entities.DomainEnums;

namespace Application.ApplicationConstantsNamesapce
{
    public static class ApplicationConstants
    {
        public const int HashingWorkFactor = 11;

        public const string PlainTextDummyPassword = "Dummy123!";
        public const string HashedDummyPassword = "$2a$11$C03CASL2IbltEN.iy3zQAO46kX2efaSvNavUYY4CIeczwCWI7nGzq";


        public static class ExpensesSortOptions
        {
            public const string SpentOn = "SpentOn";
            public const string Category = "Category";
            public const string Amount = "Amount";

            public static readonly IReadOnlyList<string> All =
                [SpentOn, Category, Amount];
        }

        public static class NotificationsSortOptions
        {
            public const string CreationDate = "CreationDate";
            public const string NotificationType = "NotificationType";

            public static readonly IReadOnlyList<string> All =
                [CreationDate, NotificationType];
        }


        public static class ScheduledExpensesSortOptions
        {
            public const string CreatedAt = "CreatedAt";
            public const string FirstDueOn = "FirstDueOn";
            public const string NextDueOn = "NextDueOn";
            public const string LastProcessedAt = "LastProcessedAt";
            public const string Cadence = "Cadence";

            public static readonly IReadOnlyList<string> All =
                [CreatedAt, FirstDueOn, NextDueOn, LastProcessedAt, Cadence];
        }

        public static class SortOrders
        {
            public const string Ascending = "ASC";
            public const string Descending = "DESC";

            public static readonly IReadOnlyList<string> All =
                [Ascending, Descending];
        }
    }
}
