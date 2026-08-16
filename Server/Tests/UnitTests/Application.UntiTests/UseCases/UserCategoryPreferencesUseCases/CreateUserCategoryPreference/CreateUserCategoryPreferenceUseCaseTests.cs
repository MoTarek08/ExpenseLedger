using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference;
using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.UserCategoryPreferenceNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference
{
    public class CreateUserCategoryPreferenceUseCaseTests
    {
        private readonly IUserCategoryPreferencesRepository _preferencesRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateUserCategoryPreferenceUseCase> _logger;
        private readonly CreateUserCategoryPreferenceUseCase _sut;
        private readonly Guid _userId;
        private readonly Guid _categoryId;
        private readonly CreateCategoryPreferenceRequestModel _request;

        public CreateUserCategoryPreferenceUseCaseTests()
        {
            _preferencesRepository = A.Fake<IUserCategoryPreferencesRepository>();
            _categoriesRepository = A.Fake<ICategoriesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<CreateUserCategoryPreferenceUseCase>>();
            _sut = new CreateUserCategoryPreferenceUseCase(
                _preferencesRepository, _categoriesRepository, _unitOfWork, _dateProvider, _logger);
            _userId = Guid.NewGuid();
            _categoryId = Guid.NewGuid();
            _request = new CreateCategoryPreferenceRequestModel(_categoryId, CategoryPreferenceLevel.Important);
        }

        [Fact]
        public async Task Execute_WhenCategoryExistsAndNoPreference_ShouldCreate()
        {
            var category = ExpenseCategory.Create("FOOD", "Food", "Food category");
            A.CallTo(() => _categoriesRepository.FindAsync(_categoryId, A<CancellationToken>._))
                .Returns(category);
            A.CallTo(() => _preferencesRepository.FindAsync(_userId, _categoryId, A<CancellationToken>._))
                .Returns((UserCategoryPreference?)null);
            A.CallTo(() => _dateProvider.Now).Returns(DateTimeOffset.UtcNow);

            var result = await _sut.Execute(_userId, _request, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _preferencesRepository.Add(A<UserCategoryPreference>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenCategoryDoesNotExist_ShouldReturnFailure()
        {
            A.CallTo(() => _categoriesRepository.FindAsync(_categoryId, A<CancellationToken>._))
                .Returns((ExpenseCategory?)null);

            var result = await _sut.Execute(_userId, _request, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _preferencesRepository.Add(A<UserCategoryPreference>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenPreferenceAlreadyExists_ShouldReturnFailure()
        {
            var category = ExpenseCategory.Create("FOOD", "Food", "Food category");
            var existingPreference = UserCategoryPreference.Create(_userId, _categoryId, CategoryPreferenceLevel.Neutral, DateTimeOffset.UtcNow);
            A.CallTo(() => _categoriesRepository.FindAsync(_categoryId, A<CancellationToken>._))
                .Returns(category);
            A.CallTo(() => _preferencesRepository.FindAsync(_userId, _categoryId, A<CancellationToken>._))
                .Returns(existingPreference);

            var result = await _sut.Execute(_userId, _request, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_ALREADY_EXISTS, result.Error!.Code);
            A.CallTo(() => _preferencesRepository.Add(A<UserCategoryPreference>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }
    }
}
