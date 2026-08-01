using Application.Interfaces.DateTimeProvider;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public class UpdateSpendingGoalRequestModelValidator
          : AbstractValidator<UpdateSpendingGoalRequestModel>
    {
        private readonly IDateProvider _dateProvider;

        public UpdateSpendingGoalRequestModelValidator(
            IDateProvider dateTimeProvider)
        {
            _dateProvider = dateTimeProvider;

            RuleFor(x => x)
                .Must(x =>
                    x.Description is not null ||
                    x.MinimumTargetAmount is not null ||
                    x.MaximumTargetAmount is not null ||
                    x.StartDate is not null ||
                    x.EndDate is not null)
                .WithMessage("At least one field must be provided.");

            When(x => x.Description is not null, () =>
            {
                RuleFor(x => x.Description!)
                    .MaximumLength(BusinessConstants.MaxDescriptionLength)
                    .WithMessage($"Description cannot exceed {BusinessConstants.MaxDescriptionLength} characters.");
            });

            When(x => x.MinimumTargetAmount is not null, () =>
            {
                RuleFor(x => x.MinimumTargetAmount!.Value)
                    .GreaterThan(0)
                    .WithMessage("Minimum target amount must be greater than zero.");
            });

            When(x => x.MaximumTargetAmount is not null, () =>
            {
                RuleFor(x => x.MaximumTargetAmount!.Value)
                    .GreaterThan(0)
                    .WithMessage("Maximum target amount must be greater than zero.");
            });

            When(x => x.MinimumTargetAmount is not null && x.MaximumTargetAmount is not null, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.MaximumTargetAmount >= x.MinimumTargetAmount)
                    .WithMessage("Maximum target amount must be greater than or equal to the minimum target amount.");
            });

            When(x => x.StartDate is not null, () =>
            {
                RuleFor(x => x.StartDate!.Value)
                    .ValidDateOnlyRange();
            });

            When(x => x.EndDate is not null, () =>
            {
                RuleFor(x => x.EndDate!.Value)
                    .ValidDateOnlyRange()
                    .GreaterThanOrEqualTo(_dateProvider.Today.AddDays(1))
                    .WithMessage("The goal must end at least one day in the future.");
            });

            When(x => x.StartDate is not null && x.EndDate is not null, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.StartDate <= x.EndDate)
                    .WithMessage("Start date cannot be later than end date.");

                RuleFor(x => x)
                    .Must(x => x.EndDate <= x.StartDate!.Value.AddYears(1))
                    .WithMessage("Gap between start date and end date cannot be more than a year");
            });
        }
    }
}
