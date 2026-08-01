using Application.UseCases.UsersUseCases.UpdateUser.ModelsNamespace;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;

namespace Host.Validation.ValidatorsNamespace
{
    public class UpdateUserRequestModelValidator : AbstractValidator<UpdateUserRequestModel>
    {
        public UpdateUserRequestModelValidator()
        {
            RuleFor(x => x)
                .Must(x => x.DisplayName is not null)
                .WithMessage("At least one field must be provided.");

            When(x => x.DisplayName is not null, () =>
            {
                RuleFor(x => x.DisplayName)
                    .Must(displayName => !string.IsNullOrWhiteSpace(displayName))
                    .WithMessage("Display name cannot be empty or whitespace.")
                    .MaximumLength(BusinessConstants.MaxDisplayNameLength)
                    .WithMessage($"Display name cannot be more than {BusinessConstants.MaxDisplayNameLength} characters.");
            });
        }
    }
}
