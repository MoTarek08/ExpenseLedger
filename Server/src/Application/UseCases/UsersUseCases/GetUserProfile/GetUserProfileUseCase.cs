using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Application.UseCases.UsersUseCases.GetUserProfile.ModelsNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UsersUseCases.GetUserProfileNamespace
{
    public class GetUserProfileUseCase
    {
        private readonly IUsersRepository _repository;
        private readonly ILogger<GetUserProfileUseCase> _logger;

        public GetUserProfileUseCase(
            IUsersRepository repository,
            ILogger<GetUserProfileUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<UserProfileDto>> Execute(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _repository.FindAsync(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Get user profile failed — user not found {UserId}", userId);
                return Result<UserProfileDto>.Failure(new Error(UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND));
            }

            var profile = await _repository.GetFinancialProfileByUserIdAsync(userId, cancellationToken);

            FinancialProfileDto? profileDto = profile is not null
                ? new FinancialProfileDto(profile.Id, profile.MonthlyNetIncome, profile.ResetDay, profile.CreatedAt)
                : null;

            var dto = new UserProfileDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.RegisteredAt,
                profileDto);

            return Result<UserProfileDto>.Success(dto);
        }
    }
}
