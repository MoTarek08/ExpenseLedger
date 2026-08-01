using Application.Interfaces.DateTimeProvider;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;

namespace Host.Validation.Validators
{
    public class CreateScheduledExpenseRequestModelValidator: AbstractValidator<CreateScheduledExpenseRequestModel>
    {
        public CreateScheduledExpenseRequestModelValidator(
            IDateProvider dateTimeProvider)
        {
            When(x => x.Title is not null, () =>
            {
                RuleFor(x => x.Title)
                    .Must(title => !string.IsNullOrWhiteSpace(title))
                    .WithMessage("Title cannot be empty or whitespace.")
                    .MaximumLength(BusinessConstants.MaxTitleLength)
                    .WithMessage($"Title cannot be more than {BusinessConstants.MaxTitleLength} characters.");
            });

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .WithMessage("Category is required.");

            RuleFor(x => x.SubCategoryId)
                .Must(id => id is null || id != Guid.Empty)
                .WithMessage("Sub-category id is invalid.");

            RuleFor(x => x.Cadence)
                .IsInEnum()
                .WithMessage("Cadence is invalid.");

            var today = DateOnly.FromDateTime(dateTimeProvider.Now.UtcDateTime);

            RuleFor(x => x.FirstDueOn)
                .GreaterThanOrEqualTo(today)
                .WithMessage("First due date cannot be in the past.")

                .LessThanOrEqualTo(today.AddYears(1))
                .WithMessage("First due date cannot be more than one year in the future.");
        }
    }
}
