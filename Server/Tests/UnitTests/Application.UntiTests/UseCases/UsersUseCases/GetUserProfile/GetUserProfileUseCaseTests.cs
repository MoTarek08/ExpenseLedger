using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.UsersUseCases.GetUserProfile.ModelsNamespace;
using Application.UseCases.UsersUseCases.GetUserProfileNamespace;
using Domain.Entities.DomainEnums;
using Domain.Entities.UserFinancialProfileNamespace;
using Domain.Entities.UserNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UsersUseCases.GetUserProfile
{
    public class GetUserProfileUseCaseTests
    {
        private readonly IUsersRepository _repository;
        private readonly ILogger<GetUserProfileUseCase> _logger;
        private readonly GetUserProfileUseCase _sut;
        private readonly Guid _userId;

        public GetUserProfileUseCaseTests()
        {
            _repository = A.Fake<IUsersRepository>();
            _logger = A.Fake<ILogger<GetUserProfileUseCase>>();
            _sut = new GetUserProfileUseCase(_repository, _logger);
            _userId = Guid.NewGuid();
        }

        [Fact]
        public async Task Execute_WhenUserExistsAndHasProfile_ShouldReturnFullDto()
        {
            var user = User.Register("test@example.com", "hash", "Test User", Role.User, DateTimeOffset.UtcNow);
            var profile = UserFinancialProfile.Create(_userId, 5000m, 1, DateTimeOffset.UtcNow);

            A.CallTo(() => _repository.FindAsync(_userId, A<CancellationToken>._))
                .Returns(user);
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns(profile);

            var result = await _sut.Execute(_userId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(user.Id, result.Data.Id);
            Assert.Equal(user.Email, result.Data.Email);
            Assert.Equal(user.DisplayName, result.Data.DisplayName);
            Assert.Equal(user.RegisteredAt, result.Data.RegisteredAt);
            Assert.NotNull(result.Data.FinancialProfile);
            Assert.Equal(profile.Id, result.Data.FinancialProfile.Id);
            Assert.Equal(profile.MonthlyNetIncome, result.Data.FinancialProfile.MonthlyNetIncome);
            Assert.Equal(profile.ResetDay, result.Data.FinancialProfile.ResetDay);
        }

        [Fact]
        public async Task Execute_WhenUserExistsAndNoProfile_ShouldReturnDtoWithNullProfile()
        {
            var user = User.Register("test@example.com", "hash", "Test User", Role.User, DateTimeOffset.UtcNow);

            A.CallTo(() => _repository.FindAsync(_userId, A<CancellationToken>._))
                .Returns(user);
            A.CallTo(() => _repository.GetFinancialProfileByUserIdAsync(_userId, A<CancellationToken>._))
                .Returns((UserFinancialProfile?)null);

            var result = await _sut.Execute(_userId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(user.DisplayName, result.Data.DisplayName);
            Assert.Null(result.Data.FinancialProfile);
        }

        [Fact]
        public async Task Execute_WhenUserNotFound_ShouldReturnFailure()
        {
            A.CallTo(() => _repository.FindAsync(_userId, A<CancellationToken>._))
                .Returns((User?)null);

            var result = await _sut.Execute(_userId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND, result.Error!.Code);
        }
    }
}
