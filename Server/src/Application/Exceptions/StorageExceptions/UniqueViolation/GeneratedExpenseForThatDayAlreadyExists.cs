using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;

namespace Application.Exceptions.StorageExceptions.UniqueViolation
{
    public class GeneratedExpenseForThatDayAlreadyExists : UniqueViolationNamespace.UniqueViolation
    {
        public GeneratedExpenseForThatDayAlreadyExists() : base("An expense from this sceduled expense was generated with the same attributes",ExpensesErrorCodes.GENERATED_EXPENSE_FOR_THAT_DAY_ALREADY_EXISTS) { }

    }
}
