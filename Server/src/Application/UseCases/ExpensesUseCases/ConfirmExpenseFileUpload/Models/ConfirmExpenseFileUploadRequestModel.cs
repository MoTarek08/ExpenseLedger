namespace Application.UseCases.ExpensesUseCases.ConfirmExpenseFileUpload.Models
{
    public sealed record ConfirmExpenseFileUploadRequestModel(
        Guid UploadedFileId,
        Guid ExpenseId);
   
}
