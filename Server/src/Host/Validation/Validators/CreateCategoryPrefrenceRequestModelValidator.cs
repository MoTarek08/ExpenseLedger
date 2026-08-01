using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class CreateCategoryPrefrenceRequestModelValidator
        : AbstractValidator<CreateCategoryPreferenceRequestModel>
    {
        public CreateCategoryPrefrenceRequestModelValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category id is required");

            RuleFor(x => x.PreferenceLevel)
                .NotEmpty().WithMessage("Prefernece level is required")
                .IsInEnum().WithMessage("Invalid prefernece level");
        }
    }
}
