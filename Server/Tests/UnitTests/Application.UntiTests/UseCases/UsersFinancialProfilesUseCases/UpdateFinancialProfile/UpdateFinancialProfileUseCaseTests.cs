using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace.ModelsNamespace;
using Domain.BusinessInvariants.BusinessValidationConstantsNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UsersFinancialProfilesUseCases.UpdateFinancialProfile
{
    public class UpdateFinancialProfileUseCaseTests
    {
        private readonly IUsersRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateFinancialProfileUseCase> _logger;
        private readonly UpdateFinancialProfileUseCase _sut;
        private readonly Guid _userId;
        private readonly UserFinancialProfile _profile;

        public UpdateFinancialProfileUseCaseTests()
        {
            _repository = A.Fake<IUsersRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<UpdateFinancialProfileUseCase>>();
            _sut = new UpdateFinancialProfileUseCase(_repository, _unitOfWork, _logger);
            _userId = Guid.NewGuid();
            _profile = UserFinancialProfile.Create(_userId, 5000m, 1, DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Execute_WhenProfileExists_ShouldApplyUpdates()
        {
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(_profile);

            var result = await _sut.Execute(_userId, new UpdateFinancialProfileRequestModel(6000m, 15), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(6000m, _profile.MonthlyNetIncome);
            Assert.Equal(15, _profile.ResetDay);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WithMinMonthlyNetIncome_ShouldSucceed()
        {
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(_profile);

            var result = await _sut.Execute(
                _userId,
                new UpdateFinancialProfileRequestModel(BusinessConstants.MinMonthlyNetIncome, null),
                default);

            Assert.True(result.IsSuccess);
            Assert.Equal(BusinessConstants.MinMonthlyNetIncome, _profile.MonthlyNetIncome);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WithSameValues_ShouldNotUpdateOrSave()
        {
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(_profile);

            var originalIncome = _profile.MonthlyNetIncome;
            var originalResetDay = _profile.ResetDay;

            var result = await _sut.Execute(_userId, new UpdateFinancialProfileRequestModel(originalIncome, originalResetDay), default);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();

            Assert.Equal(originalIncome, _profile.MonthlyNetIncome);
            Assert.Equal(originalResetDay, _profile.ResetDay);
        }
    }
}
