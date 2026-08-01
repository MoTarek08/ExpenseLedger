using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Application.UseCases.CategoriesUseCases.GetAllCategories;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.CategoriesUseCases.GetAllCategories
{
    public class GetAllCategoriesUseCaseTests
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<GetAllCategoriesUseCase> _logger;
        private readonly GetAllCategoriesUseCase _sut;

        public GetAllCategoriesUseCaseTests()
        {
            _categoriesRepository = A.Fake<ICategoriesRepository>();
            _logger = A.Fake<ILogger<GetAllCategoriesUseCase>>();
            _sut = new GetAllCategoriesUseCase(_categoriesRepository, _logger);
        }

        [Fact]
        public async Task Execute_WhenCategoriesExist_ShouldReturnAllCategories()
        {
            var categories = new List<CategoryDetails>
            {
                new(Guid.NewGuid(), "FOOD", "Food", "Food expenses", new List<SubCategoryDetails>()),
                new(Guid.NewGuid(), "TRANSPORT", "Transport", "Transport expenses", new List<SubCategoryDetails>()),
            };

            A.CallTo(() => _categoriesRepository.GetAllWithSubCategoriesAsync(A<CancellationToken>._))
                .Returns(categories);

            var result = await _sut.Execute(default);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Count);
        }

        [Fact]
        public async Task Execute_WhenNoCategoriesExist_ShouldReturnEmptyList()
        {
            A.CallTo(() => _categoriesRepository.GetAllWithSubCategoriesAsync(A<CancellationToken>._))
                .Returns(new List<CategoryDetails>());

            var result = await _sut.Execute(default);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }
    }
}
