using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.UserCategoryPreferencesUseCases.DeleteUserCategoryPreference;
using Domain.Entities.DomainEnums;
using Domain.Entities.UserCategoryPreferenceNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UserCategoryPreferencesUseCases.DeleteUserCategoryPreference
{
    public class DeleteUserCategoryPreferenceUseCaseTests
    {
        private readonly IUserCategoryPreferencesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteUserCategoryPreferenceUseCase> _logger;
        private readonly DeleteUserCategoryPreferenceUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid CategoryId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");

        public DeleteUserCategoryPreferenceUseCaseTests()
        {
            _repository = A.Fake<IUserCategoryPreferencesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<DeleteUserCategoryPreferenceUseCase>>();
            _sut = new DeleteUserCategoryPreferenceUseCase(_repository, _unitOfWork, _logger);
        }

        [Fact]
        public async Task Execute_WhenPreferenceNotFound_ShouldReturnSuccess()
        {
            A.CallTo(() => _repository.FindAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns((UserCategoryPreference?)null);

            var result = await _sut.Execute(UserId, CategoryId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Remove(A<UserCategoryPreference>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenPreferenceFound_ShouldRemoveAndReturnSuccess()
        {
            var preference = UserCategoryPreference.Create(UserId, CategoryId, CategoryPreferenceLevel.Important, DateTimeOffset.UtcNow);
            A.CallTo(() => _repository.FindAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns(preference);

            var result = await _sut.Execute(UserId, CategoryId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Remove(preference)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
