using Application.UseCases.AuthUseCases.Register.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using FluentValidation.Validators;
using Host.Controllers;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.ValidatorsNamespace
{
    public class RegisterRequestModelValidator : AbstractValidator<RegisterRequestModel>
    {
    
        public RegisterRequestModelValidator()
        {
            RuleFor(p => p.Email)
                .ValidEmail();

            RuleFor(p => p.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MinimumLength(1).WithMessage("Display name cannot be empty")
                .MaximumLength(BusinessConstants.MaxDisplayNameLength).WithMessage($"Displayname cannot be more than {BusinessConstants.MaxDisplayNameLength} characters");

            RuleFor(p => p.Password)
                .ValidPassword();

            RuleFor(p => p.PasswordConfirmation)
                .NotEmpty().WithMessage("Password confirmation is required")
                .Equal(p => p.Password).WithMessage("Password and password confirmation must match");


        }
    }
}
