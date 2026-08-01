using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolation
{
    public class NotificationDeuplicationKeyAlreadyExists : UniqueViolationNamespace.UniqueViolation
    {
        public NotificationDeuplicationKeyAlreadyExists() : base("Notification with the same deduplication key already exists", NotificationsErrorCodes.NOTIFICATION_DEDUP_KEY_ALREADY_EXISTS) { }
    }
}
