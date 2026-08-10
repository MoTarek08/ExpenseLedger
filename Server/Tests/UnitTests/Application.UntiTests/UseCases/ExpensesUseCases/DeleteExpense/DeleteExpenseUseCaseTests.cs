using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.ObjectStorageExceptions;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ExpensesUseCases.DeleteExpense;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.FileObjectNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesUseCases.DeleteExpense
{
    public class DeleteExpenseUseCaseTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<DeleteExpenseUseCase> _logger;
        private readonly DeleteExpenseUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ExpenseId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");

        public DeleteExpenseUseCaseTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _objectStorageService = A.Fake<IObjectStorageService>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<DeleteExpenseUseCase>>();
            _sut = new DeleteExpenseUseCase(
                _expensesRepository,
                _objectStorageService,
                _unitOfWork,
                _dateTimeProvider,
                _logger);
        }

        private Expense CreateTestExpense() =>
            Expense.CreateManualExpense(
                UserId,
                Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93"),
                "Test",
                100,
                new DateOnly(2026, 7, 22),
                DateTimeOffset.UtcNow);

        private static ExpenseFileObject CreateLinkedFile(Expense expense)
        {
            var now = DateTimeOffset.UtcNow;
            var file = ExpenseFileObject.CreatePendingUpload(
                expense.UserId,
                "test-key",
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                now.AddHours(-1),
                now.AddHours(1),
                "receipt.jpg");
            file.MarkAsUploaded(now);
            file.LinkToExpense(expense.Id);
            return file;
        }

        private static void AttachFileToExpense(Expense expense, ExpenseFileObject file)
        {
            typeof(Expense).GetProperty(nameof(Expense.FileObject))!.SetValue(expense, file);
        }

        [Fact]
        public async Task Execute_WhenExpenseNotFound_ShouldReturnSuccess()
        {
            A.CallTo(() => _expensesRepository.FindIncludingFileObjectAsync(ExpenseId, A<CancellationToken>._))
                .Returns((Expense?)null);

            var result = await _sut.Execute(UserId, ExpenseId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _expensesRepository.Remove(A<Expense>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenExpenseNotOwned_ShouldReturnNotFound()
        {
            var expense = Expense.CreateManualExpense(
                OtherUserId,
                Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93"),
                "Test",
                100,
                new DateOnly(2026, 7, 22),
                DateTimeOffset.UtcNow);

            A.CallTo(() => _expensesRepository.FindIncludingFileObjectAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(UserId, ExpenseId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenExpenseOwnedWithoutFile_ShouldRemoveAndReturnSuccess()
        {
            var expense = CreateTestExpense();

            A.CallTo(() => _expensesRepository.FindIncludingFileObjectAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(UserId, ExpenseId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _objectStorageService.DeleteAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _expensesRepository.Remove(expense)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenExpenseOwnedWithFile_ShouldDeleteFromStorageImmediatelyAndRemove()
        {
            var expense = CreateTestExpense();
            var file = CreateLinkedFile(expense);
            AttachFileToExpense(expense, file);

            A.CallTo(() => _expensesRepository.FindIncludingFileObjectAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(UserId, ExpenseId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _objectStorageService.DeleteAsync(file.ObjectKey, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _expensesRepository.Remove(expense)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenFileAlreadyDeletedInStorage_ShouldStillRemoveExpense()
        {
            var expense = CreateTestExpense();
            var file = CreateLinkedFile(expense);
            AttachFileToExpense(expense, file);

            A.CallTo(() => _expensesRepository.FindIncludingFileObjectAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _objectStorageService.DeleteAsync(file.ObjectKey, A<CancellationToken>._))
                .Throws<FileObjectAlreadyDeleted>();

            var result = await _sut.Execute(UserId, ExpenseId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _expensesRepository.Remove(expense)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenStorageDeletionFails_ShouldNotRemoveExpense()
        {
            var expense = CreateTestExpense();
            var file = CreateLinkedFile(expense);
            AttachFileToExpense(expense, file);

            A.CallTo(() => _expensesRepository.FindIncludingFileObjectAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _objectStorageService.DeleteAsync(file.ObjectKey, A<CancellationToken>._))
                .Throws(new InvalidOperationException("storage unavailable"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Execute(UserId, ExpenseId, CancellationToken.None));

            A.CallTo(() => _expensesRepository.Remove(expense)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}