using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolation
{
    public class CategoryPreferenceAlreadyExists : UniqueViolationNamespace.UniqueViolation
    {
        public CategoryPreferenceAlreadyExists() : base("Category preference already exists",CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_ALREADY_EXISTS) { }
    }
}