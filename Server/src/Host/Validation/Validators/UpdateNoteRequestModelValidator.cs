using Application.UseCases.NotesUseCases.UpdateNote.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;

namespace Host.Validation.Validators
{
    public sealed class UpdateNoteRequestModelValidator
        : AbstractValidator<UpdateNoteRequestModel>
    {
        public UpdateNoteRequestModelValidator()
        {
            RuleFor(x => x.Content)
                .ValidNoteContent();
        }
    }
}
