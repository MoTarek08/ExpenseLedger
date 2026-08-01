using Application.UseCases.ExpensesUseCases.UpdateExpense.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class UpdateExpenseRequestModelValidator
        : AbstractValidator<UpdateExpenseRequestModel>
    {
        public UpdateExpenseRequestModelValidator()
        {
            RuleFor(x => x)
                .Must(x =>
                    x.Title is not null ||
                    x.Amount is not null ||
                    x.CategoryId is not null ||
                    x.SubCategoryId is not null ||
                    x.SpentOn is not null)
                .WithMessage("At least one field must be provided.");

            When(x => x.Title is not null, () =>
            {
                RuleFor(x => x.Title)
                    .Must(title => !string.IsNullOrWhiteSpace(title))
                    .WithMessage("Title cannot be empty.")
                    .MaximumLength(BusinessConstants.MaxTitleLength)
                    .WithMessage($"Title cannot be more than {BusinessConstants.MaxTitleLength} characters.");
            });

            When(x => x.Amount is not null, () =>
            {
                RuleFor(x => x.Amount!.Value)
                    .GreaterThan(0)
                    .WithMessage("Amount must be greater than 0.")
                    .LessThanOrEqualTo(decimal.MaxValue)
                    .WithMessage("Amount is invalid.");
            });

            When(x => x.CategoryId is not null, () =>
            {
                RuleFor(x => x.CategoryId)
                    .NotEmpty()
                    .WithMessage("Category id cannot be empty.");
            });

            When(x => x.SubCategoryId is not null, () =>
            {
                RuleFor(x => x.SubCategoryId)
                    .NotEmpty()
                    .WithMessage("Sub category id cannot be empty.");
            });

            When(x => x.SpentOn is not null, () =>
            {
                RuleFor(x => x.SpentOn!.Value)
                    .ValidDateOnlyRange();
            });
        }
    }
}
