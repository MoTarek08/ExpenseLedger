using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using FluentValidation;

namespace Host.Validation.Validators
{
    public sealed class GetSpendingGoalByStatusQueryParametersValidator
        : PaginationParametersValidator<GetSpendingGoalsByStatusQueryParameters>
    {
        public GetSpendingGoalByStatusQueryParametersValidator()
        {
            When(x => x.CategoryId is not null, () =>
            {
                RuleFor(x => x.CategoryId)
                    .NotEmpty()
                    .WithMessage("Category id cannot be empty when provided.");
            });

            When(x => x.EndingDateFrom is not null, () =>
            {
                RuleFor(x => x.EndingDateFrom!.Value)
                    .GreaterThanOrEqualTo(DateOnly.MinValue)
                    .WithMessage("Ending date from is invalid.")
                    .LessThanOrEqualTo(DateOnly.MaxValue)
                    .WithMessage("Ending date from is invalid.");
            });

            When(x => x.EndingDateTo is not null, () =>
            {
                RuleFor(x => x.EndingDateTo!.Value)
                    .GreaterThanOrEqualTo(DateOnly.MinValue)
                    .WithMessage("Ending date to is invalid.")
                    .LessThanOrEqualTo(DateOnly.MaxValue)
                    .WithMessage("Ending date to is invalid.");
            });

            When(x => x.EndingDateFrom is not null && x.EndingDateTo is not null, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.EndingDateFrom!.Value <= x.EndingDateTo!.Value)
                    .WithMessage("'EndingDateFrom' cannot be later than 'EndingDateTo'.");
            });
        }
    }
}