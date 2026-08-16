using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace.ModelsNamespace;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UsersFinancialProfilesUseCases.CreateUserFinancialProfile
{
    public class CreateUserFinancialProfileUseCaseTests
    {
        private readonly IUsersRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateUserFinancialProfileUseCase> _logger;
        private readonly CreateUserFinancialProfileUseCase _sut;
        private readonly Guid _userId;

        public CreateUserFinancialProfileUseCaseTests()
        {
            _repository = A.Fake<IUsersRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<CreateUserFinancialProfileUseCase>>();
            _sut = new CreateUserFinancialProfileUseCase(_repository, _unitOfWork, _dateProvider, _logger);
            _userId = Guid.NewGuid();
            A.CallTo(() => _dateProvider.Now).Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Execute_WhenNoExistingProfile_ShouldCreateAndReturnId()
        {
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns((UserFinancialProfile?)null);

            var result = await _sut.Execute(_userId, new CreateUserFinancialProfileRequest(5000m, 1), TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Data);
            A.CallTo(() => _repository.AddFinancialProfile(A<UserFinancialProfile>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenProfileAlreadyExists_ShouldReturnFailure()
        {
            var existingProfile = UserFinancialProfile.Create(_userId, 3000m, 1, DateTimeOffset.UtcNow);
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(existingProfile);

            var result = await _sut.Execute(_userId, new CreateUserFinancialProfileRequest(5000m, 1), TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(UsersErrorCodes.FINANCIAL_PROFILE_ALREADY_EXISTS, result.Error!.Code);
            A.CallTo(() => _repository.AddFinancialProfile(A<UserFinancialProfile>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WithMinMonthlyNetIncome_ShouldCreateSuccessfully()
        {
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns((UserFinancialProfile?)null);

            var result = await _sut.Execute(
                _userId,
                new CreateUserFinancialProfileRequest(BusinessConstants.MinMonthlyNetIncome, 15),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}
