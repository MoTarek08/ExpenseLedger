using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.NotesUseCases.DeleteNote;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.NoteNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.NotesUseCases.DeleteNote
{
    public class DeleteNoteUseCaseTests
    {
        private readonly INotesRepository _notesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteNoteUseCase> _logger;
        private readonly DeleteNoteUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid NoteId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");

        public DeleteNoteUseCaseTests()
        {
            _notesRepository = A.Fake<INotesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<DeleteNoteUseCase>>();
            _sut = new DeleteNoteUseCase(_notesRepository, _unitOfWork, _logger);
        }

        private Note CreateNoteOwnedBy(Guid userId)
        {
            var expense = Expense.CreateManualExpense(
                userId, Guid.NewGuid(), "Test", 100m, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            var note = Note.Create(expense.Id, "Note content", DateTimeOffset.UtcNow);
            typeof(Note).GetProperty("Expense")!.SetValue(note, expense);
            return note;
        }

        [Fact]
        public async Task Execute_WhenNoteNotFound_ShouldReturnSuccess()
        {
            A.CallTo(() => _notesRepository.FindIncludingExpenseAsync(NoteId, A<CancellationToken>._))
                .Returns((Note?)null);

            var result = await _sut.Execute(UserId, NoteId, default);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _notesRepository.Remove(A<Note>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenNoteNotOwnedByUser_ShouldReturnFailure()
        {
            var note = CreateNoteOwnedBy(OtherUserId);
            A.CallTo(() => _notesRepository.FindIncludingExpenseAsync(NoteId, A<CancellationToken>._))
                .Returns(note);

            var result = await _sut.Execute(UserId, NoteId, default);

            Assert.True(result.IsFailure);
            Assert.Equal(NotesErrorCodes.NOTE_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _notesRepository.Remove(A<Note>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenNoteOwnedByUser_ShouldRemoveAndReturnSuccess()
        {
            var note = CreateNoteOwnedBy(UserId);
            A.CallTo(() => _notesRepository.FindIncludingExpenseAsync(NoteId, A<CancellationToken>._))
                .Returns(note);

            var result = await _sut.Execute(UserId, NoteId, default);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _notesRepository.Remove(note)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
