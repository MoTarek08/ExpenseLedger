using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace.ModelsNamespace;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;

namespace Host.Validation.ValidatorsNamespace
{
    public class CreateUserFinancialProfileRequestModelValidator : AbstractValidator<CreateUserFinancialProfileRequest>
    {
        public CreateUserFinancialProfileRequestModelValidator()
        {
            RuleFor(x => x.MonthlyNetIncome)
                .NotNull().WithMessage("Monthly net income is required")
                .GreaterThanOrEqualTo(BusinessConstants.MinMonthlyNetIncome)
                .WithMessage($"Monthly net income cannot be less than {BusinessConstants.MinMonthlyNetIncome} EGP");

            RuleFor(x => x.ResetDay)
                .NotNull().WithMessage("Reset day is required")
                .InclusiveBetween(1, 31).WithMessage("Invalid reset day");
        }
    }
}
