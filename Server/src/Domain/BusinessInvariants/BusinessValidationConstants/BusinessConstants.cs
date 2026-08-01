
namespace Domain.BusinessInvariants.BusinessValidationConstantsNamespace
{
    public static class BusinessConstants
    {
        public const int MaxEmailLength = 100;

        public static int MinPasswordLength = 8;
        public static int MaxPasswordLength = 32;
        public const int MaxPasswordHashLength = 100;

        public static int MinDisplayNameLength = 1;
        public static int MaxDisplayNameLength = 50;

        public static int MaxTitleLength = 100;


        public const int MaxDescriptionLength = 500;

        public const decimal MinMonthlyNetIncome = 0m;

        public const int MinCategoryNameLength = 1;
        public const int MaxCategoryNameLength = 50;

        public const int MinCategoryCodeLength = 1;
        public const int MaxCategoryCodeLength = 50;

        public const int MinNoteContentLength = 1;
        public const int MaxNoteContentLength = 2000;

        public const int MaxFileNameLength = 250;

        public const int MaxNotificationTitleLength = 250;

        public const int MaxNotificationBodyLength = 1000;



    }
}
