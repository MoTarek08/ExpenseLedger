using Application.Interfaces.BusinessQueries;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.BudgetUseCases.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.BudgetUseCases.GetRemainingBudget
{
    public class GetRemainingBudgetUseCase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IDateProvider _dateProvider;
        private readonly IBudgetQueries _budgetQueries;
        private readonly ILogger<GetRemainingBudgetUseCase> _logger;

        public GetRemainingBudgetUseCase(
            IUsersRepository usersRepository,
            IDateProvider dateProvider,
            IBudgetQueries budgetQueries,
            ILogger<GetRemainingBudgetUseCase> logger)
        {
            _usersRepository = usersRepository;
            _dateProvider = dateProvider;
            _budgetQueries = budgetQueries;
            _logger = logger;
        }

        public async Task<Result<decimal>> Execute(Guid userId, CancellationToken cancellationToken = default)
        {
            var userFinancialProfile = (await _usersRepository.GetFinancialProfileByUserIdAsync(userId, cancellationToken))!;

            var to = DateOnly.FromDateTime(_dateProvider.Now.UtcDateTime);
            var from = BudgetComputingHelpers.GetLastPayDay(userFinancialProfile.ResetDay, to);

            var budgetSpent = await _budgetQueries.GetTotalSpentAsync(userId, from, to, cancellationToken);

            var remaining = userFinancialProfile.MonthlyNetIncome - budgetSpent;

            _logger.LogInformation("Budget remaining {UserId} {Remaining}", userId, remaining);

            return Result<decimal>.Success(remaining);
        }
    }
}
