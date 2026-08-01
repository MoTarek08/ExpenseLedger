using Application.Interfaces.DateTimeProvider;
using Application.Models;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;

namespace Host.Validation.Validators
{
    public class UpdateScheduledExpenseRequestModelValidator : AbstractValidator<UpdateScheduledExpenseRequestModel>
    {
        private readonly IDateProvider _dateTimeProvider;
        public UpdateScheduledExpenseRequestModelValidator(IDateProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            RuleFor(x => x)
                .Must(x =>
                    x.Title is not null ||
                    x.Amount is not null ||
                    x.Cadence is not null ||
                    x.FirstDue is not null)
                .WithMessage("At least one field must be provided.");

            When(x => x.Title is not null, () =>
            {
                RuleFor(x => x.Title)
                    .Must(title => !string.IsNullOrWhiteSpace(title))
                    .WithMessage("Title cannot be empty or whitespace.")
                    .MaximumLength(BusinessConstants.MaxTitleLength)
                    .WithMessage($"Title cannot be more than {BusinessConstants.MaxTitleLength} characters.");
            });

            When(x => x.Amount is not null, () =>
            {
                RuleFor(x => x.Amount!.Value)
                    .GreaterThan(0)
                    .WithMessage("Amount must be greater than 0.");
            });

            When(x => x.Cadence is not null, () =>
            {
                RuleFor(x => x.Cadence!.Value)
                    .IsInEnum()
                    .WithMessage("Cadence is invalid.");
            });


            When(x => x.FirstDue is not null, () =>
            {
                var today = DateOnly.FromDateTime(_dateTimeProvider.Now.UtcDateTime);
                RuleFor(x => x.FirstDue!.Value)
                    .LessThanOrEqualTo(DateConstants.MaxDate).WithMessage("Invalid date")
                    .GreaterThanOrEqualTo(today)
                    .WithMessage("First due date cannot be in the past")
                    .LessThanOrEqualTo(today.AddYears(1))
                    .WithMessage("First due date cannot be more than one year in the future.");
            });
        }
    }
}
