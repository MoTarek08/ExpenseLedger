using Application.ApplicationConstantsNamesapce;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.HashingService;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.AuthUseCases.Register;
using Application.UseCases.AuthUseCases.Register.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.UserNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.AuthUseCases.Register
{
    public class RegisterUseCaseTests
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly IHashingService _hashingService;
        private readonly ILogger<RegisterUseCase> _logger;

        private readonly RegisterUseCase _sut;

        private readonly RegisterRequestModel _request;
        public RegisterUseCaseTests()
        {
            _usersRepository = A.Fake<IUsersRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateProvider = A.Fake<IDateProvider>();
            _hashingService = A.Fake<IHashingService>();
            _logger = A.Fake<ILogger<RegisterUseCase>>();

            _sut = new RegisterUseCase(_usersRepository, _unitOfWork, _hashingService, _dateProvider, _logger);

            _request = new RegisterRequestModel(
                "test@test.com",
                "Test",
                "password123!",
                "password123!");
        }

        [Fact]
        public async Task Execute_WhenEmailAlreadyExists_ShouldReturnFailure()
        {
            var existingUser = User.Register(
                _request.Email,
                "PasswordHash",
                "Existing User",
                Role.User,
                DateTimeOffset.UtcNow);

            A.CallTo(() => _usersRepository.FindByEmailAsync(A<string>._, A<CancellationToken>._))
                .Returns(existingUser);

            var result = await _sut.Execute(_request, default);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrorCodes.AUTH_EMAIL_ALREADY_EXISTS, result.Error!.Code);
            A.CallTo(() => _usersRepository.Add(A<User>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenValidRequest_ShouldRegisterUserAndReturnId()
        {
            var now = DateTimeOffset.UtcNow;
            const string hashedPassword = "PasswordHash";

            A.CallTo(() => _usersRepository.FindByEmailAsync(_request.Email, A<CancellationToken>._))
                .Returns((User?)null);
            A.CallTo(() => _hashingService.Hash(_request.Password, ApplicationConstants.HashingWorkFactor))
                .Returns(hashedPassword);
            A.CallTo(() => _dateProvider.Now)
                .Returns(now);

            User? addedUser = null;
            A.CallTo(() => _usersRepository.Add(A<User>._))
                .Invokes(call => addedUser = call.GetArgument<User>(0));

            var result = await _sut.Execute(_request, default);

            Assert.True(result.IsSuccess);
            Assert.NotNull(addedUser);
            Assert.Equal(_request.Email, addedUser!.Email);
            Assert.Equal(hashedPassword, addedUser.PasswordHash);
            Assert.Equal(_request.DisplayName, addedUser.DisplayName);
            Assert.Equal(Role.User, addedUser.Role);
            Assert.Equal(now, addedUser.RegisteredAt);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_ShouldNormalizeEmailToLowercase()
        {
            var request = new RegisterRequestModel(
                "Test@TEST.COM",
                "User",
                "Password123!",
                "Password123!");

            A.CallTo(() => _usersRepository.FindByEmailAsync(A<string>._, A<CancellationToken>._))
                .Returns((User?)null);
            A.CallTo(() => _hashingService.Hash(A<string>._, A<int>._))
                .Returns("hash");
            A.CallTo(() => _dateProvider.Now)
                .Returns(DateTimeOffset.UtcNow);

            User? addedUser = null;
            A.CallTo(() => _usersRepository.Add(A<User>._))
                .Invokes(call => addedUser = call.GetArgument<User>(0));

            await _sut.Execute(request, default);

            Assert.NotNull(addedUser);
            A.CallTo(() => _usersRepository.FindByEmailAsync("test@test.com", A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            Assert.Equal("test@test.com", addedUser!.Email);
        }

        [Fact]
        public async Task Execute_ShouldTrimDisplayName()
        {
            var request = new RegisterRequestModel(
                "user@test.com",
                "  My Name  ",
                "Password123!",
                "Password123!");

            A.CallTo(() => _usersRepository.FindByEmailAsync(A<string>._, A<CancellationToken>._))
                .Returns((User?)null);
            A.CallTo(() => _hashingService.Hash(A<string>._, A<int>._))
                .Returns("hash");
            A.CallTo(() => _dateProvider.Now)
                .Returns(DateTimeOffset.UtcNow);

            User? addedUser = null;
            A.CallTo(() => _usersRepository.Add(A<User>._))
                .Invokes(call => addedUser = call.GetArgument<User>(0));

            await _sut.Execute(request, default);

            Assert.NotNull(addedUser);
            Assert.Equal("My Name", addedUser!.DisplayName);
        }
    }
}
