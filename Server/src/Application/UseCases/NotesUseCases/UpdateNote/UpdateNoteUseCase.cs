using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.NotesUseCases.UpdateNote.Models;
using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotesUseCases.UpdateNote
{
    public class UpdateNoteUseCase
    {
        private readonly INotesRepository _notesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateNoteUseCase> _logger;

        public UpdateNoteUseCase(
            INotesRepository notesRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateNoteUseCase> logger)
        {
            _notesRepository = notesRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, Guid noteId, UpdateNoteRequestModel requestModel, CancellationToken cancellationToken)
        {
            var note = await _notesRepository.FindIncludingExpenseAsync(noteId, cancellationToken);
            if (note is null || note.Expense.UserId != userId)
                return Result.Failure(new Error(NotesErrorCodes.NOTE_NOT_FOUND));

            note.UpdateContent(requestModel.Content);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Note {NoteId} updated by user {UserId}",
                noteId, userId);

            return Result.Success();
        }
    }
}
