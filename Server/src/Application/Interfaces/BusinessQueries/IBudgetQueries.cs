namespace Application.Interfaces.BusinessQueries
{
    public interface IBudgetQueries
    {
        public Task<decimal> GetTotalSpentAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
        public Task<decimal> GetTotalSpentForCategoryAsync(Guid userId, Guid categoryId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    }
}
