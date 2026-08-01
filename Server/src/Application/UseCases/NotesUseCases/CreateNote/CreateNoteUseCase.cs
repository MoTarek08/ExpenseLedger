using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.NotesUseCases.CreateNote.ModelsNamespace;
using Domain.Entities.NoteNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotesUseCases.CreateNote
{
    public class CreateNoteUseCase
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly INotesRepository _notesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateNoteUseCase> _logger;

        public CreateNoteUseCase(
            IExpensesRepository expensesRepository,
            INotesRepository notesRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider,
            ILogger<CreateNoteUseCase> logger
            )
        {
            _expensesRepository = expensesRepository;
            _notesRepository = notesRepository;
            _unitOfWork = unitOfWork;
            _dateProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<Result<Guid>> Execute(
            Guid userId,
            CreateNoteRequestModel requestModel,
            CancellationToken cancellationToken
            )
        {
            var existingExpenseRecord = await _expensesRepository.FindAsync(requestModel.ExpenseId, cancellationToken);
            if (existingExpenseRecord is null || existingExpenseRecord.UserId != userId)
            {
                _logger.LogWarning(
                    "Create note denied - expense {ExpenseId} not found or not owned by user {UserId}",
                    requestModel.ExpenseId, userId);
                return Result<Guid>.Failure(new Error(NotesErrorCodes.NOTE_EXPENSE_NOT_FOUND));
            }

            var note = Note.Create(requestModel.ExpenseId, requestModel.Content, _dateProvider.Now);
            _notesRepository.Add(note);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Note {NoteId} created for expense {ExpenseId} by user {UserId}",
                note.Id, requestModel.ExpenseId, userId);

            return Result<Guid>.Success(note.Id);
        }
    }
}
