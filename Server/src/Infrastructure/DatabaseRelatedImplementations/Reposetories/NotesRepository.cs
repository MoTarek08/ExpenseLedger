using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.NotesUseCases.Models;
using Domain.Entities.NoteNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class NotesRepository : INotesRepository
    {
        private readonly AppDbContext _dbContext;

        public NotesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Note note)
        {
            _dbContext.Notes.Add(note);
        }

        public void Remove(Note note)
        {
            _dbContext.Notes.Remove(note);
        }

        public async Task<Note?> FindIncludingExpenseAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Notes.Include(x => x.Expense).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<NoteDto?> FindNoteDtoByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Notes
                .AsNoTracking()
                .Where(x => x.Id == id && x.Expense.UserId == userId)
                .Select(x => new NoteDto(x.Id, x.ExpenseId, x.Content, x.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
