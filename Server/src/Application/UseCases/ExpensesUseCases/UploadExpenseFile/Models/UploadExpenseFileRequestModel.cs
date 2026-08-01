namespace Application.UseCases.ExpensesUseCases.UploadExpenseFile.Models
{
    public sealed record UploadExpenseFileRequestModel(
        string ContentType,
        long FileSizeInBytes,
        string? OriginalFileName);
}
