using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class UpdateCategoryPrefrenceRequestModelValidator
        : AbstractValidator<UpdateCategoryPreferenceRequestModel>
    {
        public UpdateCategoryPrefrenceRequestModelValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category id is required");

            RuleFor(x => x.PreferenceLevel)
                .NotEmpty().WithMessage("Prefernece level is required")
                .IsInEnum().WithMessage("Invalid prefernece level");
        }
    }
}

