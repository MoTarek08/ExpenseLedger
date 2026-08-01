using Application.UseCases.ExpensesUseCases.ConfirmExpenseFileUpload.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class ConfirmExpenseFileUploadRequestModelExample : IExamplesProvider<ConfirmExpenseFileUploadRequestModel>
    {
        public ConfirmExpenseFileUploadRequestModel GetExamples() => new(
            Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
            Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a"));
    }
}
