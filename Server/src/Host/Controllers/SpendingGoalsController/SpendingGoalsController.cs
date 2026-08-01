using Asp.Versioning;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal.Models;
using Application.UseCases.SpendingGoalsUseCases.DeleteSpendingGoal;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal;
using Application.UseCases.NotificationsUseCases.Models;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal.Models;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.Models;
using Host.ProblemDetails.Problems;
using Host.Swagger.ResponsesExamples;
using Host.Validation.Validators;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.SpendingGoalsController
{
    ///<summary>
    /// Manages spending goals for budget tracking
    ///</summary>
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class SpendingGoalsController : ControllerBase
    {
        private readonly CreateSpendingGoalUseCase _createUseCase;
        private readonly UpdateSpendingGoalUseCase _updateUseCase;
        private readonly DeleteSpendingGoalUseCase _deleteUseCase;
        private readonly GetSpendingGoalByIdUseCase _getByIdUseCase;
        private readonly GetSpendingGoalsByStatusUseCase _getByStatusUseCase;

        public SpendingGoalsController(
            CreateSpendingGoalUseCase createUseCase,
            UpdateSpendingGoalUseCase updateUseCase,
            DeleteSpendingGoalUseCase deleteUseCase,
            GetSpendingGoalByIdUseCase getByIdUseCase,
            GetSpendingGoalsByStatusUseCase getByStatusUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _deleteUseCase = deleteUseCase;
            _getByIdUseCase = getByIdUseCase;
            _getByStatusUseCase = getByStatusUseCase;
        }


        /// <summary>
        /// Gets spending goals filtered by status
        /// </summary>
        /// <param name="status">Status filter: Succeeded, Failed, InProgress, or Pending</param>
        /// <param name="queryParameters">Pagination and optional filters</param>
        /// <param name="validator">Validator for query parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [HttpGet("{status}")]
        [Authorize]
        [SwaggerRequestExample(typeof(GetSpendingGoalsByStatusQueryParameters), typeof(GetSpendingGoalsByStatusQueryParametersExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<GetSpendingGoalsByStatusDto>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetSpendingGoalsByStatusDtoListExample))]
        public async Task<ActionResult<List<GetSpendingGoalsByStatusDto>>> GetByStatus(SpendingGoalStatus status,
            [FromQuery] GetSpendingGoalsByStatusQueryParameters queryParameters,
            [FromServices] GetSpendingGoalByStatusQueryParametersValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(queryParameters);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _getByStatusUseCase.Execute(userId,status, queryParameters, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }
        ///<summary>
        /// Create a new spending goal
        ///</summary>
        ///<remarks>
        /// Creates a spending goal for the authenticated user. A goal defines a spending range (minimum and maximum) over a period. If no category is specified, the goal applies to all expenses. Goals cannot overlap with an existing goal for the same period and category.
        ///</remarks>
        [HttpPost]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(CreateSpendingGoalRequestModel), typeof(CreateSpendingGoalRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(CreatedResourceId<Guid>))]
        [SwaggerResponseExample(StatusCodes.Status201Created, typeof(CreatedResourceIdGuidExample))]
        [ProducesError(SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS)]
        public async Task<ActionResult<CreatedResourceId<Guid>>> Create(
            CreateSpendingGoalRequestModel requestModel,
            [FromServices] CreateSpendingGoalRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _createUseCase.Execute(userId, requestModel, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Created((Uri?)null, new CreatedResourceId<Guid>(result.Data));
        }

        ///<summary>
        /// Update an existing spending goal
        ///</summary>
        ///<remarks>
        /// Updates one or more fields of a spending goal. Only the provided fields are changed. If the goal currently meets its targets after the update, a notification may be created (unless only the description was updated). The goal must belong to the authenticated user and must not be completed.
        ///</remarks>
        [HttpPatch("{id}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(UpdateSpendingGoalRequestModel), typeof(UpdateSpendingGoalRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<NotificationDto>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(NotificationDtoListExample))]
        [ProducesError(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND)]
        [ProducesError(SpendingGoalsErrorCodes.SPENDING_GOAL_COMPLETED)]
        [ProducesError(SpendingGoalsErrorCodes.SPENDING_GOAL_ALREADY_EXISTS)]
        [ProducesError(SpendingGoalsErrorCodes.SPENDING_GOAL_LONG_PERIOD_GAP)]
        public async Task<ActionResult> Update(
            Guid id,
            UpdateSpendingGoalRequestModel requestModel,
            [FromServices] UpdateSpendingGoalRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _updateUseCase.Execute(id, userId, requestModel, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            if (result.Data!.Notifications.Count > 0)
                return Ok(result.Data.Notifications);

            return NoContent();
        }

        /// <summary>
        /// Gets a spending goal by ID with its computed status and current spending
        /// </summary>
        /// <remarks>
        /// Returns the spending goal if it belongs to the authenticated user.
        /// The status is computed from the goal lifecycle and outcome.
        /// </remarks>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(SpendingGoalDto))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SpendingGoalDtoExample))]
        [ProducesError(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND)]
        public async Task<ActionResult<SpendingGoalDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _getByIdUseCase.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        /// <summary>
        /// Deletes a spending goal
        /// </summary>
        /// <remarks>
        /// Hard-deletes a spending goal. Idempotent
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [ProducesError(SpendingGoalsErrorCodes.SPENDING_GOAL_NOT_FOUND)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _deleteUseCase.Execute(id, userId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }
    }
}
