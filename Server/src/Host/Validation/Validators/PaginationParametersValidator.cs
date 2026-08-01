using Application.Models;
using FluentValidation;

namespace Host.Validation.Validators
{
    public class PaginationParametersValidator<T> : AbstractValidator<T>
        where T : PaginationParameters
    {
        protected PaginationParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(PaginationParametersConstants.MinPageNumber)
                .WithMessage($"PageNumber cannot be less than {PaginationParametersConstants.MinPageNumber}.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(PaginationParametersConstants.MinPageSize)
                .WithMessage($"PageSize cannot be less than {PaginationParametersConstants.MinPageSize}.")
                .LessThanOrEqualTo(PaginationParametersConstants.MaxPageSize)
                .WithMessage($"PageSize cannot be greater than {PaginationParametersConstants.MaxPageSize}.");
        }
    }
}
