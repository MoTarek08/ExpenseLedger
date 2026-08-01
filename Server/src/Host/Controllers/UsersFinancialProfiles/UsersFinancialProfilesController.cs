using Asp.Versioning;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace.ModelsNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace.ModelsNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace.ModelsNamespace;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.Models;
using Host.ProblemDetails.Problems;
using Host.ProblemDetailsNamespace.ProblemsNamespace;
using Host.Swagger.ResponsesExamples;
using Host.Validation.ValidatorsNamespace;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.UsersFinancialProfilesNamespace
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsersFinancialProfilesController : ControllerBase
    {
        private readonly CreateUserFinancialProfileUseCase _createUseCase;
        private readonly GetFinancialProfileUseCase _getUseCase;
        private readonly UpdateFinancialProfileUseCase _updateUseCase;

        public UsersFinancialProfilesController(
            CreateUserFinancialProfileUseCase createUseCase,
            GetFinancialProfileUseCase getUseCase,
            UpdateFinancialProfileUseCase updateUseCase)
        {
            _createUseCase = createUseCase;
            _getUseCase = getUseCase;
            _updateUseCase = updateUseCase;
        }

        ///<summary>
        /// Get the authenticated user's financial profile
        ///</summary>
        ///<remarks>
        /// Returns the financial profile if one exists, or 404 if not.
        ///</remarks>
        [HttpGet]
        [Authorize]
        [SwaggerResponse(200, Type = typeof(FinancialProfileDto))]
        [SwaggerResponseExample(200, typeof(FinancialProfileDtoExample))]
        [ProducesError(UsersErrorCodes.FINANCIAL_PROFILE_NOT_FOUND)]
        public async Task<ActionResult<FinancialProfileDto>> Get(CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();

            var result = await _getUseCase.Execute(userId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Create a financial profile for the authenticated user
        ///</summary>
        ///<remarks>
        /// Defines the monthly net income and the billing reset day.
        ///</remarks>
        [HttpPost]
        [Authorize]
        [SwaggerRequestExample(typeof(CreateUserFinancialProfileRequest), typeof(CreateUserFinancialProfileRequestExample))]
        [SwaggerResponse(201, Type = typeof(CreatedResourceId<Guid>))]
        [SwaggerResponseExample(201, typeof(CreatedResourceIdGuidExample))]
        [ProducesError(UsersErrorCodes.FINANCIAL_PROFILE_ALREADY_EXISTS)]
        public async Task<ActionResult<CreatedResourceId<Guid>>> Create(
            CreateUserFinancialProfileRequest createUserFinancialProfileRequest,
            [FromServices] CreateUserFinancialProfileRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(createUserFinancialProfileRequest);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _createUseCase.Execute(userId, createUserFinancialProfileRequest, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Created((Uri?)null, new CreatedResourceId<Guid>(result.Data));
        }

        ///<summary>
        /// Update the authenticated user's financial profile
        ///</summary>
        ///<remarks>
        /// Updates the monthly net income and/or reset day of the current financial profile.
        /// Only the fields provided in the request body are applied.
        ///</remarks>
        [HttpPatch]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(UpdateFinancialProfileRequestModel), typeof(UpdateFinancialProfileRequestModelExample))]
        [SwaggerResponse(204)]
        public async Task<ActionResult> Update(
            UpdateFinancialProfileRequestModel updateFinancialProfileRequestModel,
            [FromServices] UpdateFinancialProfileRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(updateFinancialProfileRequestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _updateUseCase.Execute(userId, updateFinancialProfileRequestModel, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }
    }
}
