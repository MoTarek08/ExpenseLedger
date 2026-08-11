namespace Application.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile.Models
{
    public sealed record UploadExpenseFileRequestModel(
        string ContentType,
        long FileSizeInBytes,
        string? OriginalFileName);
}
