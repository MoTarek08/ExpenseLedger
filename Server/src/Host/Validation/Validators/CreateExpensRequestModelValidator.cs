using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.ValidatorsNamespace
{
    public sealed class CreateExpenseRequestValidator
        : AbstractValidator<CreateExpenseRequestModel>
    {
        public CreateExpenseRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category id is required");

            RuleFor(x => x.SubCategoryId)
                .NotEmpty().WithMessage("Invalid sub category id value")
                .When(x => x.SubCategoryId is not null);

            When(x => x.Title is not null, () =>
            {
                RuleFor(x => x.Title)
                    .Must(title => !string.IsNullOrWhiteSpace(title))
                    .WithMessage("Title cannot be empty or whitespace.")
                    .MaximumLength(BusinessConstants.MaxTitleLength)
                    .WithMessage($"Title cannot be more than {BusinessConstants.MaxTitleLength} characters.");
            });

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.SpentOn)
                .NotEmpty().WithMessage("Spending date is required.")
                .ValidDateOnlyRange();
        }
    }
}