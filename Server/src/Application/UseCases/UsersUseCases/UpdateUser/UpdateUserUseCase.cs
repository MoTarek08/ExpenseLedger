using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.UsersUseCases.UpdateUser.ModelsNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.UsersUseCases.UpdateUserNamespace
{
    public class UpdateUserUseCase
    {
        private readonly IUsersRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserUseCase> _logger;

        public UpdateUserUseCase(IUsersRepository usersRepository, IUnitOfWork unitOfWork, ILogger<UpdateUserUseCase> logger)
        {
            _repository = usersRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Execute(Guid userId, UpdateUserRequestModel request, CancellationToken cancellationToken)
        {
            var user = await _repository.FindAsync(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Update user failed — user not found {UserId}", userId);
                return Result.Failure(new Error(UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND));
            }

            if (request.DisplayName is not null)
            {
                user.UpdateDisplayName(request.DisplayName.Trim());
                _logger.LogInformation("User display name updated {UserId}", userId);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
