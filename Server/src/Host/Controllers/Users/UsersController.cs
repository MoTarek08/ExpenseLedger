using Asp.Versioning;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Application.UseCases.UsersUseCases.GetUserProfile.ModelsNamespace;
using Application.UseCases.UsersUseCases.GetUserProfileNamespace;
using Application.UseCases.UsersUseCases.UpdateUser.ModelsNamespace;
using Application.UseCases.UsersUseCases.UpdateUserNamespace;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.ProblemDetails.Problems;
using Host.Swagger.ResponsesExamples;
using Host.Validation.ValidatorsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.UsersNamespace
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsersController : ControllerBase
    {
        private readonly GetUserProfileUseCase _getProfileUseCase;
        private readonly UpdateUserUseCase _updateUserUseCase;

        public UsersController(
            GetUserProfileUseCase getProfileUseCase,
            UpdateUserUseCase updateUserUseCase)
        {
            _getProfileUseCase = getProfileUseCase;
            _updateUserUseCase = updateUserUseCase;
        }
        ///<summary>
        /// Get the authenticated user's profile
        ///</summary>
        ///<remarks>
        /// Returns display name, email, registration date, and financial profile (if exists).
        ///</remarks>
        [HttpGet("profile")]
        [Authorize]
        [SwaggerResponse(200, Type = typeof(UserProfileDto))]
        [SwaggerResponseExample(200, typeof(UserProfileDtoExample))]
        [ProducesError(UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND)]
        public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();

            var result = await _getProfileUseCase.Execute(userId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Update the authenticated user's profile
        ///</summary>
        ///<remarks>
        /// Only the fields provided in the request body are applied.
        ///</remarks>
        [HttpPatch]
        [Authorize]
        [SwaggerRequestExample(typeof(UpdateUserRequestModel), typeof(UpdateUserRequestModelExample))]
        [SwaggerResponse(204)]
        [ProducesError(UsersErrorCodes.AUTHORIZED_USER_NOT_FOUND)]
        public async Task<ActionResult> UpdateDisplayName(
            UpdateUserRequestModel updateDisplayNameRequestModel,
            [FromServices] UpdateUserRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(updateDisplayNameRequestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _updateUserUseCase.Execute(userId, updateDisplayNameRequestModel, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }
    }
}
