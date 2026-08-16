using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ExpensesFileObjectsUseCases.DeleteExpenseFile;
using Domain.Entities.DomainEnums;
using Domain.Entities.FileObjectNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesFileObjectsUseCases.DeleteExpenseFile
{
    public class DeleteExpenseFileObjectUseCaseTests
    {
        private readonly IExpensesFileObjectsRepository _expensesFileObjectsRepository;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteExpenseFileObjectUseCase> _logger;
        private readonly DeleteExpenseFileObjectUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
        private static readonly Guid FileId = Guid.Parse("ae0e7bf0-5a42-45ee-a7e1-4aff4a5765f1");

        public DeleteExpenseFileObjectUseCaseTests()
        {
            _expensesFileObjectsRepository = A.Fake<IExpensesFileObjectsRepository>();
            _objectStorageService = A.Fake<IObjectStorageService>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<DeleteExpenseFileObjectUseCase>>();
            _sut = new DeleteExpenseFileObjectUseCase(
                _expensesFileObjectsRepository,
                _objectStorageService,
                _unitOfWork,
                _logger);
        }

        private static ExpenseFileObject CreateTestFile() =>
            ExpenseFileObject.CreatePendingUpload(
                UserId,
                "test-key",
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                DateTimeOffset.UtcNow.AddHours(-1),
                DateTimeOffset.UtcNow.AddHours(1),
                "receipt.jpg");

        [Fact]
        public async Task Execute_WhenFileNotFound_ShouldReturnSuccessIdempotently()
        {
            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns((ExpenseFileObject?)null);

            var result = await _sut.Execute(UserId, FileId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _objectStorageService.DeleteAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _expensesFileObjectsRepository.Remove(A<ExpenseFileObject>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenFileNotOwnedByUser_ShouldReturnFileNotFound()
        {
            var file = ExpenseFileObject.CreatePendingUpload(
                OtherUserId,
                "test-key",
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                DateTimeOffset.UtcNow.AddHours(-1),
                DateTimeOffset.UtcNow.AddHours(1),
                "receipt.jpg");

            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns(file);

            var result = await _sut.Execute(UserId, FileId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _objectStorageService.DeleteAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _expensesFileObjectsRepository.Remove(A<ExpenseFileObject>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenOwnedFile_ShouldDeleteFromStorageAndRemoveFromDb()
        {
            var file = CreateTestFile();

            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns(file);

            var result = await _sut.Execute(UserId, FileId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _objectStorageService.DeleteAsync(file.ObjectKey, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _expensesFileObjectsRepository.Remove(file)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenFileAlreadyDeletedInStorage_ShouldStillRemoveFromDb()
        {
            var file = CreateTestFile();

            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns(file);
            A.CallTo(() => _objectStorageService.DeleteAsync(file.ObjectKey, A<CancellationToken>._))
                .Throws<FileObjectAlreadyDeleted>();

            var result = await _sut.Execute(UserId, FileId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _expensesFileObjectsRepository.Remove(file)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenFileStatusFailed_ShouldReturnSuccessWithoutTouchingStorageOrDb()
        {
            var file = CreateTestFile();
            typeof(ExpenseFileObject).GetProperty(nameof(ExpenseFileObject.Status))!
                .SetValue(file, ExpenseFileObjectStatus.Failed);

            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns(file);

            var result = await _sut.Execute(UserId, FileId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _objectStorageService.DeleteAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _expensesFileObjectsRepository.Remove(A<ExpenseFileObject>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}