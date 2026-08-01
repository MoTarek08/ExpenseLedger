using Application.ApplicationConstantsNamesapce;
using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.HashingService;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.AuthUseCases.Register.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.UserNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.AuthUseCases.Register
{
    public class RegisterUseCase
    {
        private readonly IUsersRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashingService _hashingService;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<RegisterUseCase> _logger;

        public RegisterUseCase(
            IUsersRepository repository,
            IUnitOfWork unitOfWork,
            IHashingService hashingService,
            IDateProvider dateProvider,
            ILogger<RegisterUseCase> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _hashingService = hashingService;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result<Guid>> Execute(RegisterRequestModel registerRequestModel, CancellationToken cancellationToken)
        {
            var lowerCasedEmail = registerRequestModel.Email.Trim().ToLowerInvariant();
            var existingUser = await _repository.FindByEmailAsync(lowerCasedEmail, cancellationToken);
                
            if (existingUser is not null)
            {
                _logger.LogWarning(
                    "Registeration failed because email exists {Email}",
                    registerRequestModel.Email);

                return Result<Guid>.Failure(new Error(AuthErrorCodes.AUTH_EMAIL_ALREADY_EXISTS));
            }

            var hashedPassword = _hashingService.Hash(
                registerRequestModel.Password,
                ApplicationConstants.HashingWorkFactor);

            var entry = User.Register(
                lowerCasedEmail,
                hashedPassword,
                registerRequestModel.DisplayName.Trim(),
                Role.User,
                _dateProvider.Now);

            _repository.Add(entry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User registered {UserId} {Email}",
                entry.Id,
                registerRequestModel.Email);

            return Result<Guid>.Success(entry.Id);
        }
    }
}
