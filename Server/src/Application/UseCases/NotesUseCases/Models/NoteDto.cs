namespace Application.UseCases.NotesUseCases.Models
{
    public sealed record NoteDto(
        Guid Id,
        Guid ExpenseId,
        string Content,
        DateTimeOffset CreatedAt);
}
