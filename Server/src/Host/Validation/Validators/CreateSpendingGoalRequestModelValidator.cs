using Application.Interfaces.DateTimeProvider;
using Application.Models;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class CreateSpendingGoalRequestModelValidator
        : AbstractValidator<CreateSpendingGoalRequestModel>
    {
        private readonly IDateProvider _dateProvider;

        public CreateSpendingGoalRequestModelValidator(IDateProvider dateTimeProvider)
        {
            _dateProvider = dateTimeProvider;

            When(x => x.Description is not null, () =>
            {
                RuleFor(x => x.Description!)
                    .MaximumLength(BusinessConstants.MaxDescriptionLength);
            });

            RuleFor(x => x.MinimumTargetAmount)
                .GreaterThan(0);

            RuleFor(x => x.MaximumTargetAmount)
                .GreaterThan(0);

            RuleFor(x => x.MaximumTargetAmount)
                .GreaterThanOrEqualTo(x => x.MinimumTargetAmount)
                .WithMessage("Maximum target amount must be greater than or equal to the minimum target amount");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("Start date must be before the end date.");

            RuleFor(x => x.StartDate)
                .ValidDateOnlyRange();

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(_dateProvider.Today.AddDays(1))
                .WithMessage("The goal must end at least one day in the future.")
                .LessThanOrEqualTo(DateConstants.MaxDate)
                .WithMessage("End date is out of valid range.");

            RuleFor(x => x)
                .Must(x => x.EndDate <= x.StartDate.AddYears(1))
                .WithMessage("Gap between start date and end date cannot be more than a year");


            When(x => x.CategoryId.HasValue, () =>
            {
                RuleFor(x => x.CategoryId!.Value)
                    .NotEqual(Guid.Empty);
            });
        }
    }
}

