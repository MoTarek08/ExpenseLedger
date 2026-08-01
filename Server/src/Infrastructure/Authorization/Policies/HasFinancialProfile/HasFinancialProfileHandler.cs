using Application.Exceptions.AuthorizationExceptions;
using Application.Interfaces.RepositoriesNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Authorization.Policies.HasFinancialProfileNamespace
{
    public class HasFinancialProfileHandler : AuthorizationHandler<HasFinancialProfileRequirement>
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<HasFinancialProfileHandler> _logger;

        public HasFinancialProfileHandler(IUsersRepository usersRepository, ILogger<HasFinancialProfileHandler> logger)
        {
            _logger = logger;
            _usersRepository = usersRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasFinancialProfileRequirement requirement)
        {
            if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                throw new InvalidAccessToken();

            var hasFinancialProfile = await _usersRepository.GetFinancialProfileByUserIdAsync(userId) is not null;

            if (hasFinancialProfile)
                context.Succeed(requirement);

            else
                context.Fail();
        }

    }
}
