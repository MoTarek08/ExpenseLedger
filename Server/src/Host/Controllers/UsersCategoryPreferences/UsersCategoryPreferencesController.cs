using Asp.Versioning;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference;
using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference.Models;
using Application.UseCases.UserCategoryPreferencesUseCases.DeleteUserCategoryPreference;
using Application.UseCases.UserCategoryPreferencesUseCases.GetUserCategoryPreferenceById;
using Application.UseCases.UserCategoryPreferencesUseCases.Models;
using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences;
using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences.Models;
using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference;
using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference.Models;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.ProblemDetails.Problems;
using Host.ProblemDetailsNamespace;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;
using Host.ProblemDetailsNamespace.ProblemsNamespace;
using Host.Swagger.ResponsesExamples;
using Host.Validation.Validators;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.UsersCategoryPreferencesNamespace
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsersCategoryPreferencesController : ControllerBase
    {
        private readonly CreateUserCategoryPreferenceUseCase _createUseCase;
        private readonly UpdateUserCategoryPrefereneUseCase _updateUseCase;
        private readonly GetUserCategoryPreferenceByIdUseCase _getByIdUseCase;
        private readonly SearchUserCategoryPreferencesUseCase _searchUseCase;
        private readonly DeleteUserCategoryPreferenceUseCase _deleteUseCase;

        public UsersCategoryPreferencesController(
            CreateUserCategoryPreferenceUseCase createUseCase,
            UpdateUserCategoryPrefereneUseCase updateUseCase,
            GetUserCategoryPreferenceByIdUseCase getByIdUseCase,
            SearchUserCategoryPreferencesUseCase searchUseCase,
            DeleteUserCategoryPreferenceUseCase deleteUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _getByIdUseCase = getByIdUseCase;
            _searchUseCase = searchUseCase;
            _deleteUseCase = deleteUseCase;
        }

        ///<summary>
        /// Create a category preference for the authenticated user
        ///</summary>
        ///<remarks>
        /// Sets the user's spending preference level for a specific category.
        /// The operation is idempotent.
        ///</remarks>
        [HttpPost]
        [Authorize]
        [SwaggerRequestExample(typeof(CreateCategoryPreferenceRequestModel), typeof(CreateCategoryPreferenceRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [ProducesError(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND)]
        [ProducesError(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_ALREADY_EXISTS)]
        public async Task<ActionResult> Create(
            CreateCategoryPreferenceRequestModel requestModel,
            [FromServices] CreateCategoryPrefrenceRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _createUseCase.Execute(userId, requestModel, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Created();
        }

        ///<summary>
        /// Update a category preference for the authenticated user
        ///</summary>
        ///<remarks>
        /// Changes the spending preference level for an existing category preference.
        ///</remarks>
        [HttpPut]
        [Authorize]
        [SwaggerRequestExample(typeof(UpdateCategoryPreferenceRequestModel), typeof(UpdateCategoryPreferenceRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(UpdateUserCategoryPrefereneResponseModel))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateUserCategoryPrefereneResponseModelExample))]
        [ProducesError(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_CATEGORY_NOT_FOUND)]
        [ProducesError(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND)]
        public async Task<ActionResult<UpdateUserCategoryPrefereneResponseModel>> Update(
            UpdateCategoryPreferenceRequestModel requestModel,
            [FromServices] UpdateCategoryPrefrenceRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _updateUseCase.Execute(userId, requestModel, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Get a category preference by category ID
        ///</summary>
        ///<remarks>
        /// Returns the spending preference level for the specified category.
        /// The category preference must belong to the authenticated user.
        ///</remarks>
        [HttpGet("{categoryId:guid}")]
        [Authorize]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(UserCategoryPreferenceDto))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserCategoryPreferenceDtoExample))]
        [ProducesError(CategoryPreferencesErrorCodes.CATEGORY_PREFERENCE_NOT_FOUND)]
        public async Task<ActionResult<UserCategoryPreferenceDto>> GetByCategory(
            Guid categoryId, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _getByIdUseCase.Execute(userId, categoryId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Search category preferences with optional filter by preference level
        ///</summary>
        ///<remarks>
        /// Returns all preferences for the authenticated user, ordered by preference level (highest first) then by creation date.
        /// The SortOrder parameter controls the creation date sort direction (default DESC).
        /// When a preference level filter is provided, only preferences matching that level are returned.
        ///</remarks>
        [HttpGet("search")]
        [Authorize]
        [SwaggerRequestExample(typeof(SearchUserCategoryPreferencesQueryParameters), typeof(SearchUserCategoryPreferencesQueryParametersExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<UserCategoryPreferenceDto>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserCategoryPreferenceDtoListExample))]
        public async Task<ActionResult<List<UserCategoryPreferenceDto>>> Search(
            [FromQuery] SearchUserCategoryPreferencesQueryParameters queryParameters,
            [FromServices] SearchUserCategoryPreferencesQueryParametersValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(queryParameters);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _searchUseCase.Execute(userId, queryParameters, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Delete a category preference
        ///</summary>
        ///<remarks>
        /// Removes the user's spending preference for a specific category.
        /// Idempotent.
        ///</remarks>
        [HttpDelete("{categoryId:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Delete(Guid categoryId, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _deleteUseCase.Execute(userId, categoryId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }
    }
}
