using Application.UseCases.AuthUseCases.Login.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.ValidatorsNamespace
{
    public class LoginRequestModelValidator : AbstractValidator<LoginRequestModel>
    {
        public LoginRequestModelValidator()
        {
            RuleFor(p => p.Email)
                .ValidEmail();

            RuleFor(p => p.Password)
                .ValidPassword();
        }
    }
}
