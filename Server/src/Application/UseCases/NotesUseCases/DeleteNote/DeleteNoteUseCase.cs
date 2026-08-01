using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotesUseCases.DeleteNote
{
    public class DeleteNoteUseCase
    {
        private readonly INotesRepository _notesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteNoteUseCase> _logger;

        public DeleteNoteUseCase(
            INotesRepository notesRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteNoteUseCase> logger)
        {
            _notesRepository = notesRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, Guid noteId, CancellationToken cancellationToken)
        {
            var note = await _notesRepository.FindIncludingExpenseAsync(noteId, cancellationToken);
            if (note is null)
                return Result.Success();

            if (note.Expense.UserId != userId)
            {
                _logger.LogWarning(
                    "Delete note denied - note {NoteId} does not belong to user {UserId}",
                    noteId, userId);
                return Result.Failure(new Error(NotesErrorCodes.NOTE_NOT_FOUND));
            }

            _notesRepository.Remove(note);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Note {NoteId} deleted by user {UserId}",
                noteId, userId);

            return Result.Success();
        }
    }
}
