using Asp.Versioning;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense.Models;
using Application.UseCases.ScheduledExpensesUseCases.DeleteScheduledExpense;
using Application.UseCases.ScheduledExpensesUseCases.GetScheduledExpenseById;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses.Models;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense;
using Application.UseCases.ScheduledExpensesUseCases.Models;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense.Models;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.Models;
using Host.ProblemDetails.Problems;
using Host.Swagger.ResponsesExamples;
using Host.Validation.Validators;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers.ScheduledExpensesController
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ScheduledExpensesController : ControllerBase
    {
        private readonly CreateScheduledExpenseUseCase _createUseCase;
        private readonly UpdateScheduledExpenseUseCase _updateUseCase;
        private readonly GetScheduledExpenseByIdUseCase _getByIdUseCase;
        private readonly DeleteScheduledExpenseUseCase _deleteUseCase;
        private readonly SearchScheduledExpensesUseCase _searchUseCase;

        public ScheduledExpensesController(
            CreateScheduledExpenseUseCase createUseCase,
            UpdateScheduledExpenseUseCase updateUseCase,
            GetScheduledExpenseByIdUseCase getByIdUseCase,
            DeleteScheduledExpenseUseCase deleteUseCase,
            SearchScheduledExpensesUseCase searchUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _getByIdUseCase = getByIdUseCase;
            _deleteUseCase = deleteUseCase;
            _searchUseCase = searchUseCase;
        }

        ///<summary>
        /// Search scheduled expenses with filtering, sorting, and pagination
        ///</summary>
        ///<remarks>
        /// Supports filtering by active status.
        /// Pagination is always applied.
        ///</remarks>
        [HttpGet("search")]
        [Authorize]
        [SwaggerRequestExample(typeof(SearchScheduledExpensesQueryParameters), typeof(SearchScheduledExpensesQueryParametersExample))]
        [SwaggerResponse(200, Type = typeof(List<ScheduledExpenseDto>))]
        [SwaggerResponseExample(200, typeof(ScheduledExpenseDtoListExample))]
        public async Task<ActionResult<List<ScheduledExpenseDto>>> Search(
            [FromQuery] SearchScheduledExpensesQueryParameters queryParameters,
            [FromServices] SearchScheduledExpensesQueryParametersValidator validator,
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
        /// Get a single scheduled expense by ID
        ///</summary>
        ///<remarks>
        /// Returns the scheduled expense if it belongs to the authenticated user.
        ///</remarks>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerResponse(200, Type = typeof(ScheduledExpenseDto))]
        [SwaggerResponseExample(200, typeof(ScheduledExpenseDtoExample))]
        [SwaggerResponse(404)]
        [ProducesError(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND)]
        public async Task<ActionResult<ScheduledExpenseDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();

            var result = await _getByIdUseCase.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Create a scheduled expense
        ///</summary>
        ///<remarks>
        /// Creates a recurring expense template. A background job is scheduled on the next due date
        /// to generate the actual expense automatically.
        /// A financial profile is required.
        ///</remarks>
        [HttpPost]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(CreateScheduledExpenseRequestModel), typeof(CreateScheduledExpenseRequestModelExample))]
        [SwaggerResponse(201, Type = typeof(CreatedResourceId<Guid>))]
        [SwaggerResponseExample(201, typeof(CreatedResourceIdGuidExample))]
        [ProducesError(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER)]
        public async Task<ActionResult<CreatedResourceId<Guid>>> Create(
            CreateScheduledExpenseRequestModel requestModel,
            [FromServices] CreateScheduledExpenseRequestModelValidator validator,
            CancellationToken cancellationToken
            )
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
        /// Update a scheduled expense
        ///</summary>
        ///<remarks>
        /// Only the provided fields are applied. The background job is rescheduled only when
        /// the next due date changes (e.g., after a cadence change).
        /// A financial profile is required.
        ///</remarks>
        [HttpPatch("{id:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(UpdateScheduledExpenseRequestModel), typeof(UpdateScheduledExpenseRequestModelExample))]
        [SwaggerResponse(204)]
        [ProducesError(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND)]
        [ProducesError(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_ACTIVE)]
        [ProducesError(ExpensesErrorCodes.SCHEDULED_EXPENSE_PROCESSED_BEFORE_AND_CANNOT_CHANGE_FIRST_DUE)]
        public async Task<ActionResult> Update(
            Guid id,
            UpdateScheduledExpenseRequestModel requestModel,
            [FromServices] UpdateScheduledExpenseRequestModelValidator validator,
            CancellationToken cancellationToken
            )
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _updateUseCase.Execute(id, userId, requestModel, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

        ///<summary>
        /// Delete a scheduled expense
        ///</summary>
        ///<remarks>
        /// Hard-deletes the scheduled expense.
        ///</remarks>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerResponse(204)]
        [ProducesError(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND)]
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
