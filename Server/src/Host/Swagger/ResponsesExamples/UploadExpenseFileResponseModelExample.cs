using Application.UseCases.ExpensesUseCases.UploadExpenseFile.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UploadExpenseFileResponseModelExample : IExamplesProvider<UploadExpenseFileResponseModel>
    {
        public UploadExpenseFileResponseModel GetExamples() => new(
            "https://minio.local/expense-ledger/images/bff0aee8-b680-4546-8e9e-f253cd2d5930/2026/07/22/851813ff-22a0-49f8-b13b-9e1b8879da9a.jpeg?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=...",
            Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"));
    }
}
