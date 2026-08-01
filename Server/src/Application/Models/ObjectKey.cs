using Application.ApplicationConstantsNamesapce;

namespace Application.Models;

    public class ObjectKey
    {
        public string Value { get; }

        public Guid UserId { get;}
        public DateOnly UploadDate { get; }
        public string ContentType { get; }
        public string FolderName { get; }

        public ObjectKey(Guid userId, DateOnly uploadDate, string contentType, string folderName)
        {
            if (!FileObjectConstants.MappedContentTypes.TryGetValue(contentType.ToLowerInvariant(), out var objectExtension))
                throw new InvalidOperationException("Invalid content type");

            UserId = userId;
            UploadDate = uploadDate;
            ContentType = objectExtension;
            FolderName = folderName;

            Value = $"{folderName}/{userId}/{uploadDate.Year}/{uploadDate.Month:D2}/{uploadDate.Day:D2}/{Guid.NewGuid()}.{objectExtension}";
    }
}
