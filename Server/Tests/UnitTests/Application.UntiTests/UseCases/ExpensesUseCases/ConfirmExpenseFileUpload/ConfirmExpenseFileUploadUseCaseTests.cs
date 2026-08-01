using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.ObjectStorage.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ExpensesUseCases.ConfirmImageUpload;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.FileObjectNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesUseCases.ConfirmExpenseFileUpload
{
    public class ConfirmExpenseFileUploadUseCaseTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensesFileObjectsRepository _expensesFileObjectsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<ConfirmExpenseFileUploadUseCase> _logger;
        private readonly ConfirmExpenseFileUploadUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ExpenseId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");
        private static readonly Guid FileId = Guid.Parse("ae0e7bf0-5a42-45ee-a7e1-4aff4a5765f1");

        public ConfirmExpenseFileUploadUseCaseTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _expensesFileObjectsRepository = A.Fake<IExpensesFileObjectsRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _objectStorageService = A.Fake<IObjectStorageService>();
            _backgroundJobsService = A.Fake<IBackgroundJobsService>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<ConfirmExpenseFileUploadUseCase>>();
            _sut = new ConfirmExpenseFileUploadUseCase(
                _expensesRepository,
                _expensesFileObjectsRepository,
                _unitOfWork,
                _objectStorageService,
                _backgroundJobsService,
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

        [Fact]
        public async Task Execute_WhenExpenseNotFound_ShouldReturnExpenseNotFound()
        {
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns((Expense?)null);

            var result = await _sut.Execute(FileId, ExpenseId, UserId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenExpenseNotOwned_ShouldReturnExpenseNotFound()
        {
            var otherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
            var expense = Expense.CreateManualExpense(
                otherUserId,
                Guid.Parse("574e0f8e-baac-4a9b-949e-0225c2b89c93"),
                "Test",
                100,
                new DateOnly(2026, 7, 22),
                DateTimeOffset.UtcNow);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(FileId, ExpenseId, UserId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenFileNotFound_ShouldReturnFileNotFound()
        {
            var expense = CreateTestExpense();

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns((ExpenseFileObject?)null);

            var result = await _sut.Execute(FileId, ExpenseId, UserId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenFileNotOwned_ShouldReturnFileNotFound()
        {
            var otherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
            var expense = CreateTestExpense();
            var fileObject = ExpenseFileObject.CreatePendingUpload(
                otherUserId,
                "test-key",
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMinutes(5),
                "receipt.jpg");

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns(fileObject);

            var result = await _sut.Execute(FileId, ExpenseId, UserId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenFileNotUploadedToStorage_ShouldReturnNotUploadedYet()
        {
            var expense = CreateTestExpense();
            var fileObject = ExpenseFileObject.CreatePendingUpload(
                UserId,
                "test-key",
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMinutes(5),
                "receipt.jpg");

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns(fileObject);
            A.CallTo(() => _objectStorageService.GetFileInfoAsync(A<string>._, A<CancellationToken>._))
                .Returns(new FileObjectInfo(false, null));

            var result = await _sut.Execute(FileId, ExpenseId, UserId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_FILE_NOT_UPLOADED_YET, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenValidRequest_ShouldMarkUploadedAndLink()
        {
            var now = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.Zero);
            var expense = CreateTestExpense();
            var fileObject = ExpenseFileObject.CreatePendingUpload(
                UserId,
                "test-key",
                StorageProvider.MinIO,
                "image/jpeg",
                1024,
                now.AddHours(-1),
                now.AddHours(1),
                "receipt.jpg");

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns(expense);
            A.CallTo(() => _expensesFileObjectsRepository.FindAsync(FileId, A<CancellationToken>._))
                .Returns(fileObject);
            A.CallTo(() => _objectStorageService.GetFileInfoAsync(A<string>._, A<CancellationToken>._))
                .Returns(new FileObjectInfo(true, 2048));
            A.CallTo(() => _dateTimeProvider.Now).ReturnsLazily(() => now);

            var result = await _sut.Execute(FileId, ExpenseId, UserId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(ExpenseId, fileObject.ExpenseId);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
