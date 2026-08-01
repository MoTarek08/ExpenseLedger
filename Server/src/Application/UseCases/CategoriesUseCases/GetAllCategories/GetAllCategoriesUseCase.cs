using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Application.Models.Result;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.CategoriesUseCases.GetAllCategories
{
    public class GetAllCategoriesUseCase
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<GetAllCategoriesUseCase> _logger;

        public GetAllCategoriesUseCase(
            ICategoriesRepository categoriesRepository,
            ILogger<GetAllCategoriesUseCase> logger)
        {
            _categoriesRepository = categoriesRepository;
            _logger = logger;
        }

        public async Task<Result<List<CategoryDetails>>> Execute(CancellationToken cancellationToken)
        {
            var categories = await _categoriesRepository.GetAllWithSubCategoriesAsync(cancellationToken);

            _logger.LogInformation("Loaded {CategoryCount} categories", categories.Count);

            return Result<List<CategoryDetails>>.Success(categories);
        }
    }
}
