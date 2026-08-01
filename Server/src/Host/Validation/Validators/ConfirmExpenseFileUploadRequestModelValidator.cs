using Application.UseCases.ExpensesUseCases.ConfirmExpenseFileUpload.Models;
using FluentValidation;

namespace Host.Validation.Validators
{
    public sealed class ConfirmExpenseFileUploadRequestModelValidator
        : AbstractValidator<ConfirmExpenseFileUploadRequestModel>
    {
        public ConfirmExpenseFileUploadRequestModelValidator()
        {
            RuleFor(x => x.UploadedFileId)
            .NotEmpty()
            .WithMessage("Uploaded file id is required");

            RuleFor(x => x.ExpenseId)
            .NotEmpty()
            .WithMessage("Expense id is required");
        }
    }
}
