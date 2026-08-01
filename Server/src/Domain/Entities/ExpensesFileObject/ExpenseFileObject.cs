using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.UserNamespace;
using Domain.ExceptionsNamespace;

namespace Domain.Entities.FileObjectNamespace
{
    public class ExpenseFileObject
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }

        public Guid? ExpenseId { get; private set; }
        public string ObjectKey { get; private set; } = string.Empty;

        public StorageProvider StorageProvider { get; private set; }
        public string ContentType { get; private set; } = string.Empty;
        public long FileSizeInBytes { get; private set; }
        public string? OriginalFileName { get; private set; } = string.Empty;

        public FileObjectStatus Status { get; private set; }

        public DateTimeOffset StartedProcessingAt { get; private set; } // Represents the moment the pre-signed url for upload was created
        public DateTimeOffset UploadUrlExpiresAt { get; private set; }

        public DateTimeOffset? UploadedAt { get; private set; }

        public User User { get; private set; } = null!;
        public Expense? Expense { get; private set; } = null!;

        //private readonly List<ExpenseImport> _expenseImports = [];
        //public IReadOnlyList<ExpenseImport> ExpenseImports => _expenseImports.AsReadOnly();

        private ExpenseFileObject() { }

        private ExpenseFileObject(
            Guid userId,
            string objectKey,
            StorageProvider storageProvider,
            string contentType,
            long fileSizeInBytes,
            FileObjectStatus status,
            DateTimeOffset startedProcessingAt,
            DateTimeOffset uploadUrlExpiresAt,
            string? originalFileName = null)

        {
            Id = Guid.NewGuid();
            UserId = userId;
            ObjectKey = objectKey;
            StorageProvider = storageProvider;
            ContentType = contentType;
            FileSizeInBytes = fileSizeInBytes;
            OriginalFileName = originalFileName;
            Status = status;
            StartedProcessingAt = startedProcessingAt;
            UploadUrlExpiresAt = uploadUrlExpiresAt;
            OriginalFileName = originalFileName;
        }

        public static ExpenseFileObject CreatePendingUpload(
            Guid userId,
            string objectKey,
            StorageProvider storageProvider,
            string contentType,
            long fileSizeInBytes,
            DateTimeOffset startedProcessingAt,
            DateTimeOffset uploadUrlExpiresAt,
            string? originalFileName = null
            )

        {
            if (userId == Guid.Empty)
                throw new DomainException("User id is required");

            if (string.IsNullOrEmpty(objectKey))
                throw new DomainException("object key is required");

            if (string.IsNullOrEmpty(contentType))
               throw new DomainException("Content type is required");

            if (fileSizeInBytes <= 0)
                throw new DomainException("File size cannot be zero bytes");

            if (originalFileName is not null && originalFileName.Length > BusinessConstants.MaxFileNameLength)
                throw new DomainException($"File name cannot be more than {BusinessConstants.MaxFileNameLength} characters");

            if (startedProcessingAt > uploadUrlExpiresAt)
                throw new DomainException("Upload url expiry cannot be before the start processing timestamp");

            return new ExpenseFileObject(
                userId,
                objectKey,
                storageProvider,
                contentType,
                fileSizeInBytes,
                FileObjectStatus.PendingUpload,
                startedProcessingAt,
                uploadUrlExpiresAt,
                originalFileName);
        }

        public ExpenseFileObject MarkAsUploaded(DateTimeOffset uploadedAt)
        {
            if (Status != FileObjectStatus.PendingUpload)
                throw new DomainException("Only pending upload files can be uploaded");

            if (uploadedAt < StartedProcessingAt)
                throw new DomainException("Upload timestamp cannot be before start processing timestamp");


            UploadedAt = uploadedAt;
            Status = FileObjectStatus.Uploaded;

            return this;
        }

        public ExpenseFileObject ChangeFileSize(long fileSizeInBytes)
        {

            if(Status == FileObjectStatus.Failed)
                throw new DomainException("Files that failed to upload cannot be modefied");

            FileSizeInBytes = fileSizeInBytes;
            return this;
        }

        public ExpenseFileObject LinkToExpense(Guid expenseId)
        {
            if (expenseId == Guid.Empty)
                throw new DomainException("Expense id cannot be empty");

            if (ExpenseId is not null)
                throw new DomainException("File is already linked to an expense");

            if (Status != FileObjectStatus.Uploaded)
                throw new DomainException("Only uploaded files can be linked to an expense");

            ExpenseId = expenseId;
            return this;
        }

        public ExpenseFileObject UnlinkFromExpense()
        {
            if (ExpenseId is null)
                throw new DomainException("File is not linked to any expense");

            ExpenseId = null;
            return this;
        }
    }
}
