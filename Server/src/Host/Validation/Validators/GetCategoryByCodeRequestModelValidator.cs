using Application.UseCases.CategoriesUseCases.GetCategoryByCode.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class GetCategoryByCodeRequestModelValidator : AbstractValidator<GetCategoryByCodeRequestModel>
    {
        public GetCategoryByCodeRequestModelValidator()
        {
            RuleFor(x => x.Code).ValidCategoryCode();
        }
    }
}
