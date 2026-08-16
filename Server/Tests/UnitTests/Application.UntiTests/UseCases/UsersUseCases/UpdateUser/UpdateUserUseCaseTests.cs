using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.UsersUseCases.UpdateUser.ModelsNamespace;
using Application.UseCases.UsersUseCases.UpdateUserNamespace;
using Domain.Entities.DomainEnums;
using Domain.Entities.UserNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UsersUseCases.UpdateUser
{
    public class UpdateUserUseCaseTests
    {
        private readonly IUsersRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserUseCase> _logger;
        private readonly UpdateUserUseCase _sut;
        private readonly Guid _userId;

        public UpdateUserUseCaseTests()
        {
            _repository = A.Fake<IUsersRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<UpdateUserUseCase>>();
            _sut = new UpdateUserUseCase(_repository, _unitOfWork, _logger);
            _userId = Guid.NewGuid();
        }

        [Fact]
        public async Task Execute_WhenUserExists_ShouldUpdateDisplayName()
        {
            var user = User.Register("test@example.com", "hash", "Original", Role.User, DateTimeOffset.UtcNow);
            A.CallTo(() => _repository.FindAsync(_userId, A<CancellationToken>._))
                .Returns(user);

            var result = await _sut.Execute(_userId, new UpdateUserRequestModel("Updated Name"), TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Updated Name", user.DisplayName);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            A.CallTo(() => _repository.FindAsync(_userId, A<CancellationToken>._))
                .Returns((User?)null);

            var result = await _sut.Execute(_userId, new UpdateUserRequestModel("Name"), TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenDisplayNameIsNull_ShouldNotUpdateAnything()
        {
            var user = User.Register("test@example.com", "hash", "Original", Role.User, DateTimeOffset.UtcNow);
            A.CallTo(() => _repository.FindAsync(_userId, A<CancellationToken>._))
                .Returns(user);

            var result = await _sut.Execute(_userId, new UpdateUserRequestModel(null), TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Original", user.DisplayName);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}
