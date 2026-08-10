// COMMENTED OUT: Object storage deletion requests are no longer used.
// Deletion of file objects is now performed immediately when the owning entity is deleted.
// Keep this code for potential future use.
/*
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolation
{
    public class ObjectStorageDeletionRequestForThatObjecyKeyAlreadyExists : UniqueViolationNamespace.UniqueViolation
    {
        public ObjectStorageDeletionRequestForThatObjecyKeyAlreadyExists() : 
            base("A request for deleting this file object already exists",OtherErrorCodes.OBJECT_STORAGE_DELETION_REQUEST_ALREADY_EXISTS) { }

    }
}
*/