using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.NotesUseCases.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotesUseCases.GetNoteById
{
    public class GetNoteByIdUseCase
    {
        private readonly INotesRepository _notesRepository;

        public GetNoteByIdUseCase(
            INotesRepository notesRepository)
        {
            _notesRepository = notesRepository;
        }

        public async Task<Result<NoteDto>> Execute(Guid userId, Guid noteId, CancellationToken cancellationToken)
        {
            var noteDto = await _notesRepository.FindNoteDtoByIdAsync(noteId, userId, cancellationToken);
            if (noteDto is null)
                return Result<NoteDto>.Failure(new Error(NotesErrorCodes.NOTE_NOT_FOUND));

            return Result<NoteDto>.Success(noteDto);
        }
    }
}