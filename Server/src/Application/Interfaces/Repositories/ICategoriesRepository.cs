using Application.Models;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;

namespace Application.Interfaces.RepositoriesNamespace
{
    public interface ICategoriesRepository
    {
        public Task<ExpenseCategory?> FindAsync(Guid id, CancellationToken cancellationToken);

        public Task<List<CategoryDetails>> GetAllWithSubCategoriesAsync(CancellationToken cancellationToken);

        public Task<ExpenseCategory?> GetCategoryByCodeAsync(string code, CancellationToken cancellationToken);
        public Task<ExpenseCategory?> GetCategoryByCodeIncludingSubCategoriesAsync(string code, CancellationToken cancellationToken);
        public Task<CategoryDetails?> GetCategoryDetailsByCodeAsync(string code, CancellationToken cancellationToken);

        public Task<ExpenseSubCategory?> GetSubCategoryByCodeAsync(string code, CancellationToken cancellationToken);

        public Task<bool> SubBelongsToMainAsync(Guid categoryId, Guid subCategoryId, CancellationToken cancellationToken = default);

    }
}