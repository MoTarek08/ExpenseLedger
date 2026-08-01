using Asp.Versioning;
using Application.UseCases.NotesUseCases.CreateNote.ModelsNamespace;
using Application.UseCases.NotesUseCases.CreateNote;
using Application.UseCases.NotesUseCases.DeleteNote;
using Application.UseCases.NotesUseCases.GetNoteById;
using Application.UseCases.NotesUseCases.UpdateNote;
using Application.UseCases.NotesUseCases.UpdateNote.Models;
using Application.UseCases.NotesUseCases.Models;
using Application.ErrorNamespace.ErrorCodesNamespace;
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

namespace Host.Controllers.NotesNamespace
{
    ///<summary>
    /// Manages notes attached to expenses
    ///</summary>
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class NotesController : ControllerBase
    {
        private readonly CreateNoteUseCase _createUseCase;
        private readonly UpdateNoteUseCase _updateUseCase;
        private readonly GetNoteByIdUseCase _getNoteByIdUseCase;
        private readonly DeleteNoteUseCase _deleteUseCase;
        public NotesController(
            CreateNoteUseCase createUseCase,
            UpdateNoteUseCase updateNoteUseCase,
            GetNoteByIdUseCase getNoteByIdUseCase,
            DeleteNoteUseCase deleteUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateNoteUseCase;
            _getNoteByIdUseCase = getNoteByIdUseCase;
            _deleteUseCase = deleteUseCase;
        }

        ///<summary>
        /// Retrieve a note by its ID
        ///</summary>
        ///<remarks>
        /// Returns the note content and metadata for the given note ID. The note must belong to an expense owned by the authenticated user.
        ///</remarks>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerResponse(200, Type = typeof(NoteDto))]
        [SwaggerResponseExample(200,typeof(NoteDtoExample))]
        [ProducesError(NotesErrorCodes.NOTE_NOT_FOUND)]
        public async Task<ActionResult<NoteDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();

            var result = await _getNoteByIdUseCase.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Create a new note on an expense
        ///</summary>
        ///<remarks>
        /// Adds a text note to the specified expense. The expense must exist and be owned by the authenticated user.
        ///</remarks>
        [HttpPost]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(CreateNoteRequestModel), typeof(CreateNoteRequestModelExample))]
        [SwaggerResponse(201, Type = typeof(CreatedResourceId<Guid>))]
        [SwaggerResponseExample(201, typeof(CreatedResourceIdGuidExample))]
        [ProducesError(NotesErrorCodes.NOTE_EXPENSE_NOT_FOUND)]
        public async Task<ActionResult<CreatedResourceId<Guid>>> Create(
            CreateNoteRequestModel requestModel,
            [FromServices] CreateNoteRequestModelValidator validator,
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
        /// Update an existing note's content
        ///</summary>
        ///<remarks>
        /// Replaces the content of a note. The note must belong to an expense owned by the authenticated user.
        ///</remarks>
        [HttpPatch("{id:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerRequestExample(typeof(UpdateNoteRequestModel), typeof(UpdateNoteRequestModelExample))]
        [SwaggerResponse(204)]
        [ProducesError(NotesErrorCodes.NOTE_NOT_FOUND)]
        public async Task<ActionResult> Update(
            Guid id,
            UpdateNoteRequestModel requestModel,
            [FromServices] UpdateNoteRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(requestModel);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _updateUseCase.Execute(userId, id, requestModel, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

        ///<summary>
        /// Delete a note
        ///</summary>
        ///<remarks>
        /// Permanently removes the note. The note must belong to an expense owned by the authenticated user. If the note does not exist, the endpoint returns 204 (idempotent delete).
        ///</remarks>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerResponse(204)]
        [ProducesError(NotesErrorCodes.NOTE_NOT_FOUND)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _deleteUseCase.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }
    }
}
