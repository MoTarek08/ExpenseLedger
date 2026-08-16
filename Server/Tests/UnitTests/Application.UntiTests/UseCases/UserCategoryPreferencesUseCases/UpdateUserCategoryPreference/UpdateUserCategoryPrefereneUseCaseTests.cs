using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference;
using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.UserCategoryPreferenceNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference
{
    public class UpdateUserCategoryPreferenceUseCaseTests
    {
        private readonly IUserCategoryPreferencesRepository _preferencesRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserCategoryPrefereneUseCase> _logger;
        private readonly UpdateUserCategoryPrefereneUseCase _sut;
        private readonly Guid _userId;
        private readonly Guid _categoryId;
        private readonly ExpenseCategory _category;

        public UpdateUserCategoryPreferenceUseCaseTests()
        {
            _preferencesRepository = A.Fake<IUserCategoryPreferencesRepository>();
            _categoriesRepository = A.Fake<ICategoriesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<UpdateUserCategoryPrefereneUseCase>>();
            _sut = new UpdateUserCategoryPrefereneUseCase(
                _preferencesRepository, _categoriesRepository, _unitOfWork, _logger);
            _userId = Guid.NewGuid();
            _categoryId = Guid.NewGuid();
            _category = ExpenseCategory.Create("FOOD", "Food", "Food category");
        }

        [Fact]
        public async Task Execute_WhenPreferenceExists_ShouldUpdateAndReturnResponse()
        {
            var existingPreference = UserCategoryPreference.Create(_userId, _category.Id, CategoryPreferenceLevel.Neutral, DateTimeOffset.UtcNow);
            A.CallTo(() => _categoriesRepository.FindAsync(_categoryId, A<CancellationToken>._))
                .Returns(_category);
            A.CallTo(() => _preferencesRepository.FindAsync(_userId, _category.Id, A<CancellationToken>._))
                .Returns(existingPreference);

            var result = await _sut.Execute(
                _userId,
                new UpdateCategoryPreferenceRequestModel(_categoryId, CategoryPreferenceLevel.Essential),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(CategoryPreferenceLevel.Neutral, result.Data!.OldPreferenceLevel);
            Assert.Equal(CategoryPreferenceLevel.Essential, result.Data.NewPreferenceLevel);
            Assert.Equal(CategoryPreferenceLevel.Essential, existingPreference.PreferenceLevel);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenCategoryDoesNotExist_ShouldReturnFailure()
        {
            A.CallTo(() => _categoriesRepository.FindAsync(_categoryId, A<CancellationToken>._))
                .Returns((ExpenseCategory?)null);

            var result = await _sut.Execute(
                _userId,
                new UpdateCategoryPreferenceRequestModel(_categoryId, CategoryPreferenceLevel.Essential),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenPreferenceDoesNotExist_ShouldReturnFailure()
        {
            A.CallTo(() => _categoriesRepository.FindAsync(_categoryId, A<CancellationToken>._))
                .Returns(_category);
            A.CallTo(() => _preferencesRepository.FindAsync(_userId, _category.Id, A<CancellationToken>._))
                .Returns((UserCategoryPreference?)null);

            var result = await _sut.Execute(
                _userId,
                new UpdateCategoryPreferenceRequestModel(_categoryId, CategoryPreferenceLevel.Essential),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND, result.Error!.Code);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenPreferenceLevelUnchanged_ShouldNotPersist()
        {
            var existingPreference = UserCategoryPreference.Create(_userId, _category.Id, CategoryPreferenceLevel.Essential, DateTimeOffset.UtcNow);
            A.CallTo(() => _categoriesRepository.FindAsync(_categoryId, A<CancellationToken>._))
                .Returns(_category);
            A.CallTo(() => _preferencesRepository.FindAsync(_userId, _category.Id, A<CancellationToken>._))
                .Returns(existingPreference);

            var result = await _sut.Execute(
                _userId,
                new UpdateCategoryPreferenceRequestModel(_categoryId, CategoryPreferenceLevel.Essential),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }
    }
}
