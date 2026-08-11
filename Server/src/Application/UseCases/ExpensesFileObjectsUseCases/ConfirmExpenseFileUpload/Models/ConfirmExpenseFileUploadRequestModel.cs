namespace Application.UseCases.ExpensesFileObjectsUseCases.ConfirmExpenseFileUpload.Models
{
    public sealed record ConfirmExpenseFileUploadRequestModel(
        Guid UploadedFileId,
        Guid ExpenseId);
   
}
