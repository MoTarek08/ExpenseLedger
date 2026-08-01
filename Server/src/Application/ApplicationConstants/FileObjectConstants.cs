namespace Application.ApplicationConstantsNamesapce
{
    public static class FileObjectConstants
    {
        public const long MaxFileSizeBytes = 10_485_760;

        public const string jpeg = "image/jpeg";
        public const string jpg = "image/jpg";
        public const string png = "image/png";

        public static readonly IReadOnlyCollection<string> AllowedContentTypes =
            [jpeg, jpg, png];

        public static readonly IReadOnlyDictionary<string, string> MappedContentTypes = new Dictionary<string, string>()
        {
            [jpeg] = "jpeg",
            [jpg] = "jpg",
            [png] = "png",
        };

        public const string ImagesFolderName = "Images";
        
    }
}
