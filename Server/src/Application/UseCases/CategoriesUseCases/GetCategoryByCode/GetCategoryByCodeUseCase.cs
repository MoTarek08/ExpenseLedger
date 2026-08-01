using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.CategoriesUseCases.GetCategoryByCode
{
    public class GetCategoryByCodeUseCase
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<GetCategoryByCodeUseCase> _logger;

        public GetCategoryByCodeUseCase(
            ICategoriesRepository categoriesRepository,
            ILogger<GetCategoryByCodeUseCase> logger)
        {
            _categoriesRepository = categoriesRepository;
            _logger = logger;
        }

        public async Task<Result<CategoryDetails>> Execute(string code, CancellationToken cancellationToken)
        {
            var category = await _categoriesRepository.GetCategoryDetailsByCodeAsync(code, cancellationToken);

            if (category is null)
            {
                _logger.LogWarning("Category not found by code {CategoryCode}", code);
                return Result<CategoryDetails>.Failure(new Error(CategoriesErrorCodes.CATEGORY_NOT_FOUND));
            }

            _logger.LogInformation("Category loaded {CategoryCode} {CategoryId}", code, category.Id);

            return Result<CategoryDetails>.Success(category);
        }
    }
}
