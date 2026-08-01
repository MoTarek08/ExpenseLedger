using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace.ModelsNamespace;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;

namespace Host.Validation.ValidatorsNamespace
{
    public class UpdateFinancialProfileRequestModelValidator : AbstractValidator<UpdateFinancialProfileRequestModel>
    {
        public UpdateFinancialProfileRequestModelValidator()
        {
            RuleFor(x => x)
                .Must(x => x.MonthlyNetIncome is not null || x.ResetDay is not null)
                .WithMessage("At least one field must be provided.");

            When(x => x.MonthlyNetIncome is not null, () =>
            {
                RuleFor(x => x.MonthlyNetIncome!.Value)
                    .GreaterThanOrEqualTo(BusinessConstants.MinMonthlyNetIncome)
                    .WithMessage($"Monthly net income cannot be less than {BusinessConstants.MinMonthlyNetIncome}.");
            });

            When(x => x.ResetDay is not null, () =>
            {
                RuleFor(x => x.ResetDay!.Value)
                    .InclusiveBetween(1, 31)
                    .WithMessage("Reset day must be between 1 and 31.");
            });
        }
    }
}
