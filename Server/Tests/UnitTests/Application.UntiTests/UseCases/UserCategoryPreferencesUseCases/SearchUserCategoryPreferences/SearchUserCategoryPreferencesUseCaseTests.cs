using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences;
using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.UserCategoryPreferenceNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences
{
    public class SearchUserCategoryPreferencesUseCaseTests
    {
        private readonly IUserCategoryPreferencesRepository _repository;
        private readonly ILogger<SearchUserCategoryPreferencesUseCase> _logger;
        private readonly SearchUserCategoryPreferencesUseCase _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly List<UserCategoryPreference> _preferences;
        private readonly IQueryable<UserCategoryPreference> _query;

        public SearchUserCategoryPreferencesUseCaseTests()
        {
            _repository = A.Fake<IUserCategoryPreferencesRepository>();
            _logger = A.Fake<ILogger<SearchUserCategoryPreferencesUseCase>>();
            _sut = new SearchUserCategoryPreferencesUseCase(_repository, _logger);

            _preferences = new List<UserCategoryPreference>
            {
                CreatePref(CategoryPreferenceLevel.Neutral, new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero)),
                CreatePref(CategoryPreferenceLevel.Important, new DateTimeOffset(2026, 7, 5, 0, 0, 0, TimeSpan.Zero)),
                CreatePref(CategoryPreferenceLevel.Essential, new DateTimeOffset(2026, 7, 10, 0, 0, 0, TimeSpan.Zero)),
                CreatePref(CategoryPreferenceLevel.Important, new DateTimeOffset(2026, 7, 3, 0, 0, 0, TimeSpan.Zero)),
            };
            _query = _preferences.AsQueryable();

            A.CallTo(() => _repository.GetAllForUserQuery(_userId))
                .Returns(_query);

            A.CallTo(() => _repository.ToPreferenceDtoListAsync(
                    A<IQueryable<UserCategoryPreference>>._,
                    A<CancellationToken>._))
                .ReturnsLazily((IQueryable<UserCategoryPreference> q, CancellationToken _) =>
                    Task.FromResult(q.Select(p => new UserCategoryPreferenceDto(
                        "CODE", "Name", p.PreferenceLevel, p.CreatedAt)).ToList()));
        }

        private static UserCategoryPreference CreatePref(CategoryPreferenceLevel level, DateTimeOffset createdAt)
        {
            var pref = UserCategoryPreference.Create(Guid.NewGuid(), Guid.NewGuid(), level, createdAt);
            typeof(UserCategoryPreference)
                .GetField("<CreatedAt>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(pref, createdAt);
            return pref;
        }

        [Fact]
        public async Task Execute_WhenNoFilterDefaultSort_ShouldOrderByLevelDescThenCreatedAtDesc()
        {
            var result = await _sut.Execute(_userId,
                new SearchUserCategoryPreferencesQueryParameters(null), TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(4, result.Data!.Count);
            // Level DESC: Essential(3) > Important(2) > Neutral(1)
            // Then CreatedAt DESC within same level
            Assert.Equal(CategoryPreferenceLevel.Essential, result.Data[0].PreferenceLevel);
            // Important: Jul 5 vs Jul 3, DESC => Jul 5 first
            Assert.Equal(CategoryPreferenceLevel.Important, result.Data[1].PreferenceLevel);
            Assert.Equal(new DateTimeOffset(2026, 7, 5, 0, 0, 0, TimeSpan.Zero), result.Data[1].CreatedAt);
            Assert.Equal(CategoryPreferenceLevel.Important, result.Data[2].PreferenceLevel);
            Assert.Equal(new DateTimeOffset(2026, 7, 3, 0, 0, 0, TimeSpan.Zero), result.Data[2].CreatedAt);
            Assert.Equal(CategoryPreferenceLevel.Neutral, result.Data[3].PreferenceLevel);
        }

        [Fact]
        public async Task Execute_WhenNoFilterCreatedAtAsc_ShouldOrderByLevelDescThenCreatedAtAsc()
        {
            var result = await _sut.Execute(_userId,
                new SearchUserCategoryPreferencesQueryParameters(null, SortOrder: "ASC"), TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(4, result.Data!.Count);
            // Level DESC: Essential(3) > Important(2) > Neutral(1)
            // Then CreatedAt ASC within same level
            Assert.Equal(CategoryPreferenceLevel.Essential, result.Data[0].PreferenceLevel);
            // Important: Jul 3 vs Jul 5, ASC => Jul 3 first
            Assert.Equal(CategoryPreferenceLevel.Important, result.Data[1].PreferenceLevel);
            Assert.Equal(new DateTimeOffset(2026, 7, 3, 0, 0, 0, TimeSpan.Zero), result.Data[1].CreatedAt);
            Assert.Equal(CategoryPreferenceLevel.Important, result.Data[2].PreferenceLevel);
            Assert.Equal(new DateTimeOffset(2026, 7, 5, 0, 0, 0, TimeSpan.Zero), result.Data[2].CreatedAt);
            Assert.Equal(CategoryPreferenceLevel.Neutral, result.Data[3].PreferenceLevel);
        }

        [Fact]
        public async Task Execute_WhenFilteredByLevel_ShouldApplySortOrder()
        {
            var result = await _sut.Execute(_userId,
                new SearchUserCategoryPreferencesQueryParameters(
                    CategoryPreferenceLevel.Important,
                    SortOrder: "ASC"),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Execute_ShouldApplyPagination()
        {
            var result = await _sut.Execute(_userId,
                new SearchUserCategoryPreferencesQueryParameters(null)
                {
                    PageNumber = 1,
                    PageSize = 2
                },
                TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Count);
        }
    }
}
