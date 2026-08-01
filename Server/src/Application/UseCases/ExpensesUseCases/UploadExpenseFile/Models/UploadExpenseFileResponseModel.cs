namespace Application.UseCases.ExpensesUseCases.UploadExpenseFile.Models
{
    public sealed record UploadExpenseFileResponseModel(string UploadUrl, Guid FileObjectId);
}
