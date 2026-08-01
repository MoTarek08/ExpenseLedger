using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.NotesUseCases.GetNoteById;
using Application.UseCases.NotesUseCases.Models;
using FakeItEasy;

namespace UnitTests.Application.UntiTests.UseCases.NotesUseCases.GetNoteById
{
    public class GetNoteByIdUseCaseTests
    {
        private readonly INotesRepository _notesRepository;
        private readonly GetNoteByIdUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid NoteId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");

        public GetNoteByIdUseCaseTests()
        {
            _notesRepository = A.Fake<INotesRepository>();
            _sut = new GetNoteByIdUseCase(_notesRepository);
        }

        [Fact]
        public async Task Execute_WhenNoteNotFound_ShouldReturnFailure()
        {
            A.CallTo(() => _notesRepository.FindNoteDtoByIdAsync(NoteId, UserId, A<CancellationToken>._))
                .Returns((NoteDto?)null);

            var result = await _sut.Execute(UserId, NoteId, default);

            Assert.True(result.IsFailure);
            Assert.Equal(NotesErrorCodes.NOTE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenNoteFound_ShouldReturnDto()
        {
            var noteDto = new NoteDto(NoteId, Guid.NewGuid(), "Content", DateTimeOffset.UtcNow);

            A.CallTo(() => _notesRepository.FindNoteDtoByIdAsync(NoteId, UserId, A<CancellationToken>._))
                .Returns(noteDto);

            var result = await _sut.Execute(UserId, NoteId, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(noteDto, result.Data);
        }
    }
}
