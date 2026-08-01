using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal.Models;
using Domain.Entities.SpendingGoalNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal
{
    public class CreateSpendingGoalUseCase
    {
        private readonly ISpendingGoalsRepository _spendingGoalRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateSpendingGoalUseCase> _logger;

        public CreateSpendingGoalUseCase(
            ISpendingGoalsRepository spendingGoalRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateProvider,
            ILogger<CreateSpendingGoalUseCase> logger)
        {
            _spendingGoalRepository = spendingGoalRepository;
            _unitOfWork = unitOfWork;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result<Guid>> Execute(
            Guid userId,
            CreateSpendingGoalRequestModel requestModel,
            CancellationToken cancellationToken)
        {
            var willConflict = await _spendingGoalRepository.ExistsForPeriodAsync(
                userId,
                requestModel.CategoryId,
                requestModel.StartDate,
                requestModel.EndDate);

            if (willConflict)
            {
                _logger.LogWarning("Spending goal conflict detected for user {UserId}", userId);
                return Result<Guid>.Failure(new Error(SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS));  
            }

            var spendingGoal = SpendingGoal.Create(
                userId,
                requestModel.Description,
                requestModel.CategoryId,
                requestModel.MaximumTargetAmount,
                requestModel.MinimumTargetAmount,
                requestModel.StartDate,
                requestModel.EndDate,
                _dateProvider.Now);

            _spendingGoalRepository.Add(spendingGoal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Spending goal created {GoalId} for user {UserId}", spendingGoal.Id, userId);
            return Result<Guid>.Success(spendingGoal.Id);
        }
    } 
}