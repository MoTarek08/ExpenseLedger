using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace.ModelsNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace
{
    public class UpdateFinancialProfileUseCase
    {
        private readonly IUsersRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateFinancialProfileUseCase> _logger;

        public UpdateFinancialProfileUseCase(IUsersRepository usersRepository, IUnitOfWork unitOfWork, ILogger<UpdateFinancialProfileUseCase> logger)
        {
            _repository = usersRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, UpdateFinancialProfileRequestModel request, CancellationToken cancellationToken)
        {
            var profile = (await _repository.GetFinancialProfileByUserIdAsync(userId, cancellationToken))!;

            bool shouldSave = true;

            if (request.MonthlyNetIncome.HasValue)
            {
                if (request.MonthlyNetIncome.Value == profile.MonthlyNetIncome)
                    shouldSave = false;

                else profile.UpdateMonthlyNetIncome(request.MonthlyNetIncome.Value);
            }

            if (request.ResetDay.HasValue)
            {
                if (request.ResetDay.Value == profile.ResetDay)
                    shouldSave = false;

                else profile.UpdateResetDay(request.ResetDay.Value);
            }

            if(shouldSave)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Financial profile updated {UserId}", userId);

            return Result.Success();
        }
    }
}
