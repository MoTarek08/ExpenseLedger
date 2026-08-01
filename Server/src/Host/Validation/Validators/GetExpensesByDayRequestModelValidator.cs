using Application.UseCases.ExpensesUseCases.GetExpensesByDay.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class GetExpensesByDayRequestModelValidator
        : PaginationParametersValidator<GetExpensesByDayRequestModel>
    {
        public GetExpensesByDayRequestModelValidator()
        {
            RuleFor(x => x.Day)
                .NotEmpty()
                .WithMessage("Day is required.")
                .ValidDateOnlyRange();
        }
    }
}
