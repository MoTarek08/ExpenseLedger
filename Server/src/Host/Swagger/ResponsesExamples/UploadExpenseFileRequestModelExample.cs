using Application.UseCases.ExpensesUseCases.UploadExpenseFile.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UploadExpenseFileRequestModelExample : IExamplesProvider<UploadExpenseFileRequestModel>
    {
        public UploadExpenseFileRequestModel GetExamples() => new(
            "image/jpeg",
            2_097_152,
            "receipt.jpg");
    }
}
