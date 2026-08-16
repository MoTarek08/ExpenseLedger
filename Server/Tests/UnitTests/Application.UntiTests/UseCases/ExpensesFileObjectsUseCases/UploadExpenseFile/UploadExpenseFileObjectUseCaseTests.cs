using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.ObjectStorage;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models;
using Application.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile;
using Application.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.FileObjectNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile
{
    public class UploadExpenseFileObjectUseCaseTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExpensesFileObjectsRepository _fileObjectsRepository;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<UploadExpenseFileObjectUseCase> _logger;
        private readonly UploadExpenseFileObjectUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");

        public UploadExpenseFileObjectUseCaseTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _fileObjectsRepository = A.Fake<IExpensesFileObjectsRepository>();
            _objectStorageService = A.Fake<IObjectStorageService>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<UploadExpenseFileObjectUseCase>>();
            _sut = new UploadExpenseFileObjectUseCase(
                _unitOfWork,
                _fileObjectsRepository,
                _objectStorageService,
                _dateTimeProvider,
                _logger);
        }

        [Fact]
        public async Task Execute_WhenValidRequest_ShouldReturnSuccessWithUrlAndFileId()
        {
            var now = DateTimeOffset.UtcNow;
            var request = new UploadExpenseFileRequestModel("image/jpeg", 1024, "receipt.jpg");

            A.CallTo(() => _dateTimeProvider.Now).Returns(now);
            A.CallTo(() => _objectStorageService.Provider).Returns(StorageProvider.MinIO);
            A.CallTo(() => _objectStorageService.GetUploadUrlLifeTime()).Returns(5);
            A.CallTo(() => _objectStorageService.GenerateUploadUrlAsync(
                A<ObjectKey>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
                .Returns("https://storage.local/presigned-url");

            var result = await _sut.Execute(UserId, request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("https://storage.local/presigned-url", result.Data!.UploadUrl);
            Assert.NotEqual(Guid.Empty, result.Data.FileObjectId);
            A.CallTo(() => _fileObjectsRepository.Add(A<ExpenseFileObject>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}