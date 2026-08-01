using Application.ApplicationConstantsNamesapce;
using Application.UseCases.ExpensesUseCases.UploadExpenseFile.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;

namespace Host.Validation.Validators
{
    public sealed class UploadExpenseFileRequestModelValidator
        : AbstractValidator<UploadExpenseFileRequestModel>
    {
        public UploadExpenseFileRequestModelValidator()
        {
            RuleFor(x => x.ContentType)
                .NotEmpty()
                .WithMessage("Content type is required.")
                .Must(ct => FileObjectConstants.AllowedContentTypes
                    .Contains(ct.ToLowerInvariant()))
                .WithMessage($"Content type must be one of: " +
                    $"{string.Join(", ", FileObjectConstants.AllowedContentTypes)}.");

            RuleFor(x => x.FileSizeInBytes)
                .GreaterThan(0)
                .WithMessage("File size must be greater than zero.")
                .LessThanOrEqualTo(FileObjectConstants.MaxFileSizeBytes)
                .WithMessage($"File size cannot exceed {FileObjectConstants.MaxFileSizeBytes / 1_048_576}MB.");

            When(x => x.OriginalFileName is not null, () =>
            {
                RuleFor(x => x.OriginalFileName)
                    .MaximumLength(BusinessConstants.MaxFileNameLength)
                    .WithMessage($"File name cannot have more than {BusinessConstants.MaxFileNameLength} characters.");
            });
        }
    }
}
