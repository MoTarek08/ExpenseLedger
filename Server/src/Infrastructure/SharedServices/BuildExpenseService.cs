using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.SharedServices;
using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Domain.Entities.ExpenseNamespace;

namespace Infrastructure.SharedServices
{
    public class BuildExpenseService : IBuildExpenseService
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IDateProvider _dateProvider;

        public BuildExpenseService(ICategoriesRepository categoriesRepository,IDateProvider dateProvider)
        {
            _categoriesRepository = categoriesRepository;
            _dateProvider = dateProvider;
        }

        public virtual async Task<Result<Expense>> BuildExpense(Guid userId, CreateExpenseRequestModel requestModel, CancellationToken cancellationToken = default)
        {
            if (requestModel.SubCategoryId is not null)
            {
                if (!await _categoriesRepository.SubBelongsToMainAsync(requestModel.CategoryId, requestModel.SubCategoryId.Value, cancellationToken))
                    return Result<Expense>.Failure(new Error(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER));
            }

            var validatedTitle = requestModel.Title is not null ? requestModel.Title.Trim() : null;
            var entry = Expense.CreateManualExpense(
                userId,
                requestModel.CategoryId,
                requestModel.Title?.Trim(),
                requestModel.Amount,
                requestModel.SpentOn,
                _dateProvider.Now,
                requestModel.SubCategoryId);

            return Result<Expense>.Success(entry);
        }
    }
}
