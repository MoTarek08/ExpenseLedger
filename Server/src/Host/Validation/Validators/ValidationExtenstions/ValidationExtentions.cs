using Application.ApplicationConstantsNamesapce;
using Application.Models;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using FluentValidation.Validators;

namespace Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidEmail<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress(EmailValidationMode.AspNetCoreCompatible).WithMessage("Invalid email format")
                .MaximumLength(BusinessConstants.MaxEmailLength).WithMessage($"Email cannot be more than {BusinessConstants.MaxEmailLength} characters");

        }

        public static IRuleBuilderOptions<T, string> ValidPassword<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(BusinessConstants.MinPasswordLength).WithMessage($"Password cannot be less than {BusinessConstants.MinPasswordLength} characters")
                .MaximumLength(BusinessConstants.MaxPasswordLength).WithMessage($"Password cannot be more than {BusinessConstants.MaxPasswordLength} characters")
                .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]+").WithMessage("Password must contain at least one number");
        }

        public static IRuleBuilderOptions<T, string> ValidCategoryCode<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            var minimumLengthExceedMessage = BusinessConstants.MinCategoryCodeLength >= 1 ?
                "Category code cannot be empty" :
                $"Category code cannot be less than {BusinessConstants.MaxCategoryCodeLength} characters.";

            return ruleBuilder
                .NotEmpty().WithMessage("Category code is required.")
                .MinimumLength(BusinessConstants.MinCategoryCodeLength)
                .WithMessage(minimumLengthExceedMessage)
                .MaximumLength(BusinessConstants.MaxCategoryCodeLength)
                .WithMessage($"Category code cannot be more than {BusinessConstants.MaxCategoryCodeLength} characters.");
        }

        public static IRuleBuilderOptions<T, string> ValidNoteContent<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Note content is required")
                .MinimumLength(BusinessConstants.MinNoteContentLength)
                .WithMessage($"Note content must be more than {BusinessConstants.MinNoteContentLength} characters")
                .MaximumLength(BusinessConstants.MaxNoteContentLength)
                .WithMessage($"Note content cannot be more than {BusinessConstants.MaxNoteContentLength} characters");
        }

        public static IRuleBuilderOptions<T, int> ValidPageSize<T>(
          this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(0).WithMessage("Page size cannot be less than 0")
                .LessThanOrEqualTo(PaginationParametersConstants.MaxPageSize)
                .WithMessage($"Page size cannot be more than {PaginationParametersConstants.MaxPageSize} items");

        }

        public static IRuleBuilderOptions<T, DateOnly> ValidDateOnlyRange<T>(
            this IRuleBuilder<T, DateOnly> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(DateConstants.MinDate)
                .WithMessage("Date is out of valid range.")
                .LessThanOrEqualTo(DateConstants.MaxDate)
                .WithMessage("Date is out of valid range.");
        }

        public static IRuleBuilderOptions<T, string> ValidSortOrder<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(v => ApplicationConstants.SortOrders.All.Contains(v, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"SortOrder must be one of: {string.Join(", ", ApplicationConstants.SortOrders.All)}.");
        }
    }

}
