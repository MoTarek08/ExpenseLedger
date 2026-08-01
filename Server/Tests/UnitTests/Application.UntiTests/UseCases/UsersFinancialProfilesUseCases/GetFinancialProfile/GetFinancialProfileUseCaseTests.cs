using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UsersFinancialProfilesUseCases.GetFinancialProfile
{
    public class GetFinancialProfileUseCaseTests
    {
        private readonly IUsersRepository _repository;
        private readonly ILogger<GetFinancialProfileUseCase> _logger;
        private readonly GetFinancialProfileUseCase _sut;
        private readonly Guid _userId;

        public GetFinancialProfileUseCaseTests()
        {
            _repository = A.Fake<IUsersRepository>();
            _logger = A.Fake<ILogger<GetFinancialProfileUseCase>>();
            _sut = new GetFinancialProfileUseCase(_repository, _logger);
            _userId = Guid.NewGuid();
        }

        [Fact]
        public async Task Execute_WhenProfileExists_ShouldReturnDto()
        {
            var profile = UserFinancialProfile.Create(_userId, 5000m, 1, DateTimeOffset.UtcNow);
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(profile);

            var result = await _sut.Execute(_userId, default);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(profile.Id, result.Data.Id);
            Assert.Equal(profile.MonthlyNetIncome, result.Data.MonthlyNetIncome);
            Assert.Equal(profile.ResetDay, result.Data.ResetDay);
            Assert.Equal(profile.CreatedAt, result.Data.CreatedAt);
        }

        [Fact]
        public async Task Execute_WhenProfileNotFound_ShouldReturnFailure()
        {
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns((UserFinancialProfile?)null);

            var result = await _sut.Execute(_userId, default);

            Assert.True(result.IsFailure);
            Assert.Equal(UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND, result.Error!.Code);
        }
    }
}
