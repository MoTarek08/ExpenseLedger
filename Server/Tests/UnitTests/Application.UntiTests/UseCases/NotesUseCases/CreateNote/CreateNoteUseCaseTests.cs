using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.NotesUseCases.CreateNote;
using Application.UseCases.NotesUseCases.CreateNote.ModelsNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.NoteNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.NotesUseCases.CreateNote
{
    public class CreateNoteUseCaseTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly INotesRepository _notesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateNoteUseCase> _logger;

        private readonly CreateNoteUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ExpenseId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");

        public CreateNoteUseCaseTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _notesRepository = A.Fake<INotesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<CreateNoteUseCase>>();

            _sut = new CreateNoteUseCase(
                _expensesRepository,
                _notesRepository,
                _unitOfWork,
                _dateProvider,
                _logger);
        }

        [Fact]
        public async Task Execute_WhenExpenseDoesNotExist_ShouldReturnFailure()
        {
            var request = new CreateNoteRequestModel(ExpenseId, "Note content");

            A.CallTo(() => _expensesRepository.FindAsync(request.ExpenseId, A<CancellationToken>._))
                .Returns((Expense?)null);

            var result = await _sut.Execute(UserId, request, default);

            Assert.True(result.IsFailure);
            Assert.Equal(NotesErrorCodes.NOTE_EXPENSE_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _notesRepository.Add(A<Note>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenExpenseNotOwnedByUser_ShouldReturnFailure()
        {
            var otherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
            var expense = Expense.CreateManualExpense(
                otherUserId, Guid.NewGuid(), "Test", 100m, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            var request = new CreateNoteRequestModel(ExpenseId, "Note content");

            A.CallTo(() => _expensesRepository.FindAsync(request.ExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(UserId, request, default);

            Assert.True(result.IsFailure);
            Assert.Equal(NotesErrorCodes.NOTE_EXPENSE_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _notesRepository.Add(A<Note>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenValidRequest_ShouldCreateAndReturnId()
        {
            var now = DateTimeOffset.UtcNow;
            var expense = Expense.CreateManualExpense(
                UserId, Guid.NewGuid(), "Test", 100m, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            var request = new CreateNoteRequestModel(ExpenseId, "Note content");

            A.CallTo(() => _expensesRepository.FindAsync(request.ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _dateProvider.Now).Returns(now);

            Note? capturedNote = null;
            A.CallTo(() => _notesRepository.Add(A<Note>._))
                .Invokes(call => capturedNote = call.GetArgument<Note>(0));

            var result = await _sut.Execute(UserId, request, default);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Data);
            Assert.NotNull(capturedNote);
            Assert.Equal(request.ExpenseId, capturedNote!.ExpenseId);
            Assert.Equal(request.Content, capturedNote.Content);
            A.CallTo(() => _notesRepository.Add(A<Note>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
