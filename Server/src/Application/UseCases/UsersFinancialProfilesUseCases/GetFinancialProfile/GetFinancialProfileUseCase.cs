using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace
{
    public class GetFinancialProfileUseCase
    {
        private readonly IUsersRepository _repository;
        private readonly ILogger<GetFinancialProfileUseCase> _logger;

        public GetFinancialProfileUseCase(
            IUsersRepository repository,
            ILogger<GetFinancialProfileUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<FinancialProfileDto>> Execute(Guid userId, CancellationToken cancellationToken)
        {
            var profile = await _repository.GetFinancialProfileByUserIdAsync(userId, cancellationToken);

            if (profile is null)
            {
                _logger.LogWarning("Get financial profile failed — not found {UserId}", userId);
                return Result<FinancialProfileDto>.Failure(new Error(UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND));
            }

            var dto = new FinancialProfileDto(
                profile.Id,
                profile.MonthlyNetIncome,
                profile.ResetDay,
                profile.CreatedAt);

            return Result<FinancialProfileDto>.Success(dto);
        }
    }
}
