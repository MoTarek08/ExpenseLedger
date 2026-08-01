using Application.UseCases.NotesUseCases.CreateNote.ModelsNamespace;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{

    public class CreateNoteRequestModelValidator : AbstractValidator<CreateNoteRequestModel>
    {
        public CreateNoteRequestModelValidator()
        {
            RuleFor(x => x.ExpenseId)
                .NotEmpty().WithMessage("Expense id is required");

            RuleFor(x => x.Content)
                .ValidNoteContent();
        }
    }
}
