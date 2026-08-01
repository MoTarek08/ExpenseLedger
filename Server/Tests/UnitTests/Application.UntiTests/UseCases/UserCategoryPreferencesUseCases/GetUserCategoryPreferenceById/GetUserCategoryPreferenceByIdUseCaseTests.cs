using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.UserCategoryPreferencesUseCases.GetUserCategoryPreferenceById;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.UserCategoryPreferenceNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UserCategoryPreferencesUseCases.GetUserCategoryPreferenceById
{
    public class GetUserCategoryPreferenceByIdUseCaseTests
    {
        private readonly IUserCategoryPreferencesRepository _repository;
        private readonly ILogger<GetUserCategoryPreferenceByIdUseCase> _logger;
        private readonly GetUserCategoryPreferenceByIdUseCase _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public GetUserCategoryPreferenceByIdUseCaseTests()
        {
            _repository = A.Fake<IUserCategoryPreferencesRepository>();
            _logger = A.Fake<ILogger<GetUserCategoryPreferenceByIdUseCase>>();
            _sut = new GetUserCategoryPreferenceByIdUseCase(_repository, _logger);
        }

        [Fact]
        public async Task Execute_WhenNotFound_ShouldReturnFailure()
        {
            A.CallTo(() => _repository.FindIncludingCategoryAsync(_userId, _categoryId, A<CancellationToken>._))
                .Returns(Task.FromResult<UserCategoryPreference?>(null));

            var result = await _sut.Execute(_userId, _categoryId, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenFound_ShouldReturnDto()
        {
            var category = ExpenseCategory.Create("FOOD", "Food", "desc");
            var preference = UserCategoryPreference.Create(
                _userId, _categoryId, CategoryPreferenceLevel.Essential, DateTimeOffset.UtcNow);

            typeof(UserCategoryPreference)
                .GetField("<Category>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(preference, category);

            A.CallTo(() => _repository.FindIncludingCategoryAsync(_userId, _categoryId, A<CancellationToken>._))
                .Returns(Task.FromResult<UserCategoryPreference?>(preference));

            var result = await _sut.Execute(_userId, _categoryId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("FOOD", result.Data!.CategoryCode);
            Assert.Equal("Food", result.Data.CategoryName);
            Assert.Equal(CategoryPreferenceLevel.Essential, result.Data.PreferenceLevel);
        }
    }
}
