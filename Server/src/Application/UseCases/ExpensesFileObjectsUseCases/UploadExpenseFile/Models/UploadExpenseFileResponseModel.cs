namespace Application.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile.Models
{
    public sealed record UploadExpenseFileResponseModel(string UploadUrl, Guid FileObjectId);
}
