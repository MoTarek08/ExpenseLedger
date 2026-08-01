using Asp.Versioning;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.UseCases.ExpensesUseCases.ConfirmExpenseFileUpload.Models;
using Application.UseCases.ExpensesUseCases.ConfirmImageUpload;
using Application.UseCases.ExpensesUseCases.CreateExpense.Models;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Application.UseCases.ExpensesUseCases.DeleteExpense;
using Application.UseCases.ExpensesUseCases.GetExpenseById;
using Application.UseCases.ExpensesUseCases.GetExpensesByDay;
using Application.UseCases.ExpensesUseCases.GetExpensesByDay.Models;
using Application.UseCases.ExpensesUseCases.SearchExpenses;
using Application.UseCases.ExpensesUseCases.SearchExpenses.Models;
using Application.UseCases.ExpensesUseCases.UpdateExpense;
using Application.UseCases.ExpensesUseCases.UpdateExpense.Models;
using Application.UseCases.ExpensesUseCases.UploadExpenseFile;
using Application.UseCases.ExpensesUseCases.UploadExpenseFile.Models;
using Application.UseCases.ExpensesUseCases.Models;
using Application.UseCases.NotificationsUseCases.Models;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.ProblemDetails.Problems;
using Host.RateLimiters;
using Host.Swagger.ResponsesExamples;
using Host.Validation.Validators;
using Host.Validation.ValidatorsNamespace;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.ExpensesControllerNamespace
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ExpensesController : ControllerBase
    {

        private readonly CreateExpenseUseCase _createExpenseUseCase;
        private readonly GetExpensesByDayUseCase _getExpensesByDayUseCase;
        private readonly SearchExpensesUseCase _searchExpensesUseCase;
        private readonly UploadExpenseFileUseCase _uploadExpenseFileUseCase;
        private readonly ConfirmExpenseFileUploadUseCase _confirmExpenseFileUploadUseCase;
        private readonly UpdateExpenseUseCase _updateUseCase;
        private readonly DeleteExpenseUseCase _deleteExpenseUseCase;
        private readonly GetExpenseByIdUseCase _getExpenseByIdUseCase;

        public ExpensesController(
            CreateExpenseUseCase createExpenseUseCase,
            GetExpensesByDayUseCase getExpensesByDayUseCase,
            SearchExpensesUseCase searchExpensesUseCase,
            UploadExpenseFileUseCase uploadExpenseFileUseCase,
            ConfirmExpenseFileUploadUseCase confirmExpenseFileUploadUseCase,
            UpdateExpenseUseCase updateUseCase,
            DeleteExpenseUseCase deleteExpenseUseCase,
            GetExpenseByIdUseCase getExpenseByIdUseCase)
        {
            _createExpenseUseCase = createExpenseUseCase;
            _getExpensesByDayUseCase = getExpensesByDayUseCase;
            _searchExpensesUseCase = searchExpensesUseCase;
            _uploadExpenseFileUseCase = uploadExpenseFileUseCase;
            _confirmExpenseFileUploadUseCase = confirmExpenseFileUploadUseCase;
            _updateUseCase = updateUseCase;
            _deleteExpenseUseCase = deleteExpenseUseCase;
            _getExpenseByIdUseCase = getExpenseByIdUseCase;
        }
        /// <summary>
        /// Searches expenses for the authenticated user with the provided query parameters.
        /// </summary>
        /// <returns>A paginated list of expense DTOs matching the search criteria.</returns>
        [HttpGet("search")]
        [Authorize]
        [SwaggerRequestExample(typeof(SearchExpensesQueryParameters), typeof(SearchExpensesQueryParametersExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<ExpenseDto>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ExpenseDtoListExample))]
        public async Task<ActionResult<List<ExpenseDto>>> Search(
            [FromQuery] SearchExpensesQueryParameters queryParameters,
            [FromServices] SearchExpensesQueryParametersValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(queryParameters);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _searchExpensesUseCase.Execute(userId, queryParameters, cancellationToken);

            return Ok(result.Data);
        }


        /// <summary>
        /// Retrieves all expenses for the authenticated user on a specific day.
        /// </summary>
        /// <returns>A list of expense DTOs for the given day.</returns>
        [HttpGet]
        [Authorize]
        [SwaggerRequestExample(typeof(GetExpensesByDayRequestModel), typeof(GetExpensesByDayQueryParametersExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<ExpenseDto>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ExpenseDtoListExample))]
        public async Task<ActionResult<List<ExpenseDto>>> GetByDay(
            [FromQuery] GetExpensesByDayRequestModel query,
            [FromServices] GetExpensesByDayRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(query);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _getExpensesByDayUseCase.Execute(userId, query, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }


        /// <summary>
        /// Retrieves a single expense by its ID for the authenticated user.
        /// </summary>
        /// <param name="id">The expense ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The expense DTO if found; otherwise a 404 problem response.</returns>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ExpenseDto))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ExpenseDtoExample))]
        [ProducesError(ExpensesErrorCodes.EXPENSE_NOT_FOUND)]
        public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();

            var result = await _getExpenseByIdUseCase.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }


        /// <summary>
        /// Creates a new manual expense for the authenticated user.
        /// </summary>
        /// <returns>The created expense ID and any notifications generated during creation.</returns>
        /// <remarks>
        /// A financial profile is required to create expenses.
        /// Budget threshold checks and category preference checks are processed in the background after creation.
        /// Spending goal achievement is also evaluated asynchronously.
        /// </remarks>
        [HttpPost]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(CreateExpenseRequestModel), typeof(CreateExpenseRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(CreateExpenseResponseModel))]
        [SwaggerResponseExample(StatusCodes.Status201Created, typeof(CreateExpenseResponseModelExample))]
        [ProducesError(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER)]
        public async Task<ActionResult<CreateExpenseResponseModel>> Create(
            CreateExpenseRequestModel createExpenseRequestModel,
            [FromServices] CreateExpenseRequestValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(createExpenseRequestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _createExpenseUseCase.Execute(userId, createExpenseRequestModel, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Created((Uri?)null, result.Data);
        }


        /// <summary>
        /// Updates an existing expense for the authenticated user.
        /// </summary>
        /// <returns>A list of notifications generated during the update, if any.</returns>
        /// <remarks>
        /// Budget checks and goal achievement evaluations are triggered in the background only when relevant fields change.
        /// A financial profile is required.
        /// </remarks>
        [HttpPatch("{id:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(UpdateExpenseRequestModel), typeof(UpdateExpenseRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<NotificationDto>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(NotificationDtoListExample))]
        [ProducesError(ExpensesErrorCodes.EXPENSE_NOT_FOUND)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER)]
        public async Task<ActionResult> Update(
            Guid id,
            UpdateExpenseRequestModel requestModel,
            [FromServices] UpdateExpenseRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _updateUseCase.Execute(userId, id, requestModel, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }


        /// <summary>
        /// Initiates a file upload by creating a pending file record and returning a presigned upload URL.
        /// </summary>
        /// <returns>A presigned URL for direct upload and the file object ID for later confirmation.</returns>
        /// <remarks>
        /// This is the first step of a two-step upload flow. After uploading to the presigned URL,
        /// call the confirm endpoint to link the file to an expense.
        /// A financial profile is required.
        /// </remarks>
        [HttpPut("upload")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [EnableRateLimiting(RateLimitingPolicies.Upload)]
        [SwaggerRequestExample(typeof(UploadExpenseFileRequestModel), typeof(UploadExpenseFileRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(UploadExpenseFileResponseModel))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UploadExpenseFileResponseModelExample))]
        public async Task<ActionResult<UploadExpenseFileResponseModel>> Upload(
            UploadExpenseFileRequestModel requestModel,
            [FromServices] UploadExpenseFileRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _uploadExpenseFileUseCase.Execute(userId, requestModel, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        /// <summary>
        /// Confirms a previously uploaded file and links it to an expense.
        /// </summary>
        /// <remarks>
        /// The file must have been uploaded to the presigned URL obtained from the upload endpoint.
        /// The file must be in PendingUpload state and not already linked to another expense.
        /// A financial profile is required.
        /// </remarks>
        [HttpPost("upload/confirm")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(ConfirmExpenseFileUploadRequestModel), typeof(ConfirmExpenseFileUploadRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_NOT_FOUND)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_FILE_ALREADY_LINKED_TO_OTHER_EXPENSE)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_INVALID_FILE_STATE)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_FILE_NOT_UPLOADED_YET)]
        public async Task<ActionResult> Confirm(
            ConfirmExpenseFileUploadRequestModel requestModel,
            [FromServices] ConfirmExpenseFileUploadRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();
            var result = await _confirmExpenseFileUploadUseCase.Execute(requestModel.UploadedFileId, requestModel.ExpenseId, userId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

        /// <summary>
        /// Deletes an expense for the authenticated user.
        /// </summary>
        /// <remarks>
        /// If the expense has a linked file, a deletion request is queued for async cleanup of object storage.
        /// A financial profile is required.
        /// </remarks>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_NOT_FOUND)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();

            var result = await _deleteExpenseUseCase.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

    }
}
