using Application.UseCases.NotesUseCases.Models;
using Domain.Entities.NoteNamespace;

namespace Application.Interfaces.RepositoriesNamespace
{
    public interface INotesRepository
    {
        public void Add(Note note);
        public void Remove(Note note);

        public Task<Note?> FindIncludingExpenseAsync(Guid id, CancellationToken cancellationToken);

        public Task<NoteDto?> FindNoteDtoByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    }
}
