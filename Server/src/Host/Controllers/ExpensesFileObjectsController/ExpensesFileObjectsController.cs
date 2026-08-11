using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.UseCases.ExpensesFileObjectsUseCases.ConfirmExpenseFileUpload;
using Application.UseCases.ExpensesFileObjectsUseCases.ConfirmExpenseFileUpload.Models;
using Application.UseCases.ExpensesFileObjectsUseCases.DeleteExpenseFile;
using Application.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile;
using Application.UseCases.ExpensesFileObjectsUseCases.UploadExpenseFile.Models;
using Asp.Versioning;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.ProblemDetails.Problems;
using Host.RateLimiters;
using Host.Swagger.ResponsesExamples;
using Host.Validation.Validators;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.ExpensesFileObjectsController
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ExpensesFileObjectsController : ControllerBase
    {
        private readonly UploadExpenseFileObjectUseCase _uploadUseCase;
        private readonly ConfirmExpenseFileObjectUploadUseCase _confirmUseCase;
        private readonly DeleteExpenseFileObjectUseCase _deleteUseCase;

        public ExpensesFileObjectsController(
            UploadExpenseFileObjectUseCase uploadUseCase,
            ConfirmExpenseFileObjectUploadUseCase confirmUseCase,
            DeleteExpenseFileObjectUseCase deleteUseCase)
        {
            _uploadUseCase = uploadUseCase;
            _confirmUseCase = confirmUseCase;
            _deleteUseCase = deleteUseCase;
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
            var result = await _uploadUseCase.Execute(userId, requestModel, cancellationToken);
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
        /// The expense must not already have a linked file, an expense can only have one file at a time.
        /// A financial profile is required.
        /// </remarks>
        [HttpPost("confirm-upload")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(ConfirmExpenseFileUploadRequestModel), typeof(ConfirmExpenseFileUploadRequestModelExample))]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_NOT_FOUND)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_ALREADY_HAS_A_FILE)]
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
            var result = await _confirmUseCase.Execute(requestModel.UploadedFileId, requestModel.ExpenseId, userId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

        /// <summary>
        /// Deletes a file object for the authenticated user.
        /// </summary>
        /// <remarks>
        /// The file is deleted from object storage and removed from the database.
        /// The endpoint is idempotent.
        /// A financial profile is required.
        /// </remarks>
        [HttpDelete("{fileId:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [ProducesError(ExpensesErrorCodes.EXPENSE_FILE_NOT_FOUND)]
        public async Task<ActionResult> DeleteFile(Guid fileId, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _deleteUseCase.Execute(userId, fileId, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }
    }
}
