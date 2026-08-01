using Application.Exceptions.StorageExceptions.ForeignKeyViolation;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;
using Infrastructure.Database.AppDbContextNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DatabaseRelatedImplementations.Reposetories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<CategoriesRepository> _logger;

        public CategoriesRepository(AppDbContext dbContext,ILogger<CategoriesRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ExpenseCategory?> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.ExpenseCategories.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<ExpenseCategory?> GetCategoryByCodeAsync(string code, CancellationToken cancellationToken)
        {
            return await _dbContext.ExpenseCategories.AsNoTracking().SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<ExpenseCategory?> GetCategoryByCodeIncludingSubCategoriesAsync(string code, CancellationToken cancellationToken)
        {
            return await _dbContext.ExpenseCategories.Include(p => p.SubCategories).AsNoTracking().SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<ExpenseSubCategory?> GetSubCategoryByCodeAsync(string code, CancellationToken cancellationToken)
        {
            return await _dbContext.ExpenseSubCategories.AsNoTracking().SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<bool> SubBelongsToMainAsync(Guid categoryId, Guid subCategoryId, CancellationToken cancellationToken = default)
        {
            var mainCategory = await _dbContext.ExpenseCategories.Include(x => x.SubCategories).AsNoTracking().SingleOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
            if (mainCategory is null)
                throw new ReferencedEntityNotFound("Category");

            return mainCategory.SubCategories.FirstOrDefault(x => x.Id == subCategoryId) is not null;
        }


        public async Task<List<CategoryDetails>> GetAllWithSubCategoriesAsync(CancellationToken cancellationToken)
        {
            var categories = await _dbContext.ExpenseCategories
                .AsNoTracking()
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Code)
                .ToListAsync(cancellationToken);
                
            return categories.Select(MapToDetails).ToList();
        }

        public async Task<CategoryDetails?> GetCategoryDetailsByCodeAsync(string code, CancellationToken cancellationToken)
        {
            var category = await _dbContext.ExpenseCategories
                .AsNoTracking()
                .Include(c => c.SubCategories)
                .SingleOrDefaultAsync(x => x.Code == code, cancellationToken);

            return category is null ? null : MapToDetails(category);
        }

        private static CategoryDetails MapToDetails(ExpenseCategory category)
        {
            var subCategories = category.SubCategories
                .OrderBy(s => s.Code)
                .Select(s => new SubCategoryDetails(
                    s.Id,
                    s.Code,
                    s.Name,
                    s.Description))
                .ToList();

            return new CategoryDetails(
                category.Id,
                category.Code,
                category.Name,
                category.Description,
                subCategories);
        }
    }
}
