using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace.ModelsNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace
{
    public class CreateUserFinancialProfileUseCase
    {
        private readonly IUsersRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateUserFinancialProfileUseCase> _logger;

        public CreateUserFinancialProfileUseCase(
            IUsersRepository usersRepository,
            IUnitOfWork unitOfWork,
            IDateProvider dateProvider,
            ILogger<CreateUserFinancialProfileUseCase> logger)
        {
            _repository = usersRepository;
            _unitOfWork = unitOfWork;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result<Guid>> Execute(Guid userId, CreateUserFinancialProfileRequest createUserFinancialProfileRequest, CancellationToken cancellationToken)
        {
            var existingRecord = await _repository.GetFinancialProfileByUserIdAsync(userId, cancellationToken);
            if (existingRecord is not null)
            {
                _logger.LogWarning("Create financial profile failed — already exists {UserId}", userId);
                return Result<Guid>.Failure(new Error(UsersErrorCodes.FINANCIAL_PROFILE_ALREADY_EXISTS));
            }

            var entry = UserFinancialProfile.Create(
                userId,
                createUserFinancialProfileRequest.MonthlyNetIncome,
                createUserFinancialProfileRequest.ResetDay,
                _dateProvider.Now);

            _repository.AddFinancialProfile(entry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Financial profile created {UserId} {ProfileId}", userId, entry.Id);

            return Result<Guid>.Success(entry.Id);
        }
    }
}
