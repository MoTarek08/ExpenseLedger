using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Application.UseCases.CategoriesUseCases.GetCategoryByCode;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.CategoriesUseCases.GetCategoryByCode
{
    public class GetCategoryByCodeUseCaseTests
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<GetCategoryByCodeUseCase> _logger;
        private readonly GetCategoryByCodeUseCase _sut;

        public GetCategoryByCodeUseCaseTests()
        {
            _categoriesRepository = A.Fake<ICategoriesRepository>();
            _logger = A.Fake<ILogger<GetCategoryByCodeUseCase>>();
            _sut = new GetCategoryByCodeUseCase(_categoriesRepository, _logger);
        }

        [Fact]
        public async Task Execute_WhenCategoryExists_ShouldReturnCategory()
        {
            var category = new CategoryDetails(
                Guid.NewGuid(), "FOOD", "Food", "Food expenses", new List<SubCategoryDetails>());

            A.CallTo(() => _categoriesRepository.GetCategoryDetailsByCodeAsync("FOOD", A<CancellationToken>._))
                .Returns(category);

            var result = await _sut.Execute("FOOD", TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("FOOD", result.Data!.Code);
        }

        [Fact]
        public async Task Execute_WhenCategoryDoesNotExist_ShouldReturnCategoryNotFound()
        {
            A.CallTo(() => _categoriesRepository.GetCategoryDetailsByCodeAsync("INVALID", A<CancellationToken>._))
                .Returns((CategoryDetails?)null);

            var result = await _sut.Execute("INVALID", TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoriesErrorCodes.CATEGORY_NOT_FOUND, result.Error!.Code);
        }
    }
}
