using Asp.Versioning;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Models;
using Application.UseCases.NotificationsUseCases.DeleteNotification;
using Application.UseCases.NotificationsUseCases.GetCurrentPeriodNotifications;
using Application.UseCases.NotificationsUseCases.GetNotificationById;
using Application.UseCases.NotificationsUseCases.MarkNotificationAsRead;
using Application.UseCases.NotificationsUseCases.RestoreNotification;
using Application.UseCases.NotificationsUseCases.SearchNotifications;
using Application.UseCases.NotificationsUseCases.Models;
using Application.UseCases.NotificationsUseCases.SearchNotifications.Models;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.ProblemDetails.Problems;
using Host.Swagger.ResponsesExamples;
using Host.Validation.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.NotificationsController
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class NotificationsController : ControllerBase
    {
        private readonly GetCurrentPeriodNotificationsUseCase _getCurrentPeriodNotifications;
        private readonly GetNotificationByIdUseCase _getNotificationById;
        private readonly MarkNotificationAsReadUseCase _markNotificationAsRead;
        private readonly DeleteNotificationUseCase _deleteNotification;
        private readonly RestoreNotificationUseCase _restoreNotification;
        private readonly SearchNotificationsUseCase _searchUseCase;

        public NotificationsController(
            GetCurrentPeriodNotificationsUseCase getCurrentPeriodNotifications,
            GetNotificationByIdUseCase getNotificationById,
            MarkNotificationAsReadUseCase markNotificationAsRead,
            DeleteNotificationUseCase deleteNotification,
            RestoreNotificationUseCase restoreNotification,
            SearchNotificationsUseCase searchUseCase)
        {
            _getCurrentPeriodNotifications = getCurrentPeriodNotifications;
            _getNotificationById = getNotificationById;
            _markNotificationAsRead = markNotificationAsRead;
            _deleteNotification = deleteNotification;
            _restoreNotification = restoreNotification;
            _searchUseCase = searchUseCase;
        }

        ///<summary>
        /// Get notifications for the current budget period
        ///</summary>
        ///<remarks>
        /// Pagination is always applied.
        ///</remarks>
        [HttpGet("current-period")]
        [Authorize]
        [SwaggerResponse(200, Type = typeof(List<NotificationDto>))]
        [SwaggerResponseExample(200, typeof(NotificationDtoListExample))]
        public async Task<ActionResult<List<NotificationDto>>> GetCurrentPeriodNotifications([FromQuery] PaginationParameters paginationParameters, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _getCurrentPeriodNotifications.Execute(userId, paginationParameters, cancellationToken);

            return Ok(result.Data);
        }

        ///<summary>
        /// Search visible notifications with filters
        ///</summary>
        ///<remarks>
        /// Supports filtering by notification type, read/unread status, and date range.
        /// Pagination is always applied.
        ///</remarks>
        [HttpGet("search")]
        [Authorize]
        [SwaggerRequestExample(typeof(SearchNotificationsQueryParameters), typeof(SearchNotificationsQueryParametersExample))]
        [SwaggerResponse(200, Type = typeof(List<NotificationDto>))]
        [SwaggerResponseExample(200, typeof(NotificationDtoListExample))]
        public async Task<ActionResult<List<NotificationDto>>> Search(
            [FromQuery] SearchNotificationsQueryParameters queryParameters,
            [FromServices] SearchNotificationsQueryParametersValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(queryParameters);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var userId = this.GetUserIdFromClaims();

            var result = await _searchUseCase.Execute(userId, queryParameters, cancellationToken);

            return Ok(result.Data);
        }


        ///<summary>
        /// Get a single notification by ID
        ///</summary>
        ///<remarks>
        /// Returns a notification by its ID.
        ///</remarks>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerResponse(200, Type = typeof(NotificationDto))]
        [SwaggerResponseExample(200, typeof(NotificationDtoExample))]
        [SwaggerResponse(404)]
        [ProducesError(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND)]
        public async Task<ActionResult<NotificationDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _getNotificationById.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Mark a notification as read
        ///</summary>
        ///<remarks>
        /// The operation is idempotent.
        ///</remarks>
        [HttpPatch("{id:guid}/read")]
        [Authorize]
        [SwaggerResponse(204)]
        [ProducesError(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND)]
        public async Task<ActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _markNotificationAsRead.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

        ///<summary>
        /// Restore a soft-deleted notification
        ///</summary>
        ///<remarks>
        /// The operation is idempotent.
        ///</remarks>
        [HttpPatch("{id:guid}/restore")]
        [Authorize]
        [SwaggerResponse(204)]
        [ProducesError(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND)]
        public async Task<ActionResult> Restore(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _restoreNotification.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

        ///<summary>
        /// Soft-delete a notification
        ///</summary>
        ///<remarks>
        /// The operation is idempotent.
        ///</remarks>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [SwaggerResponse(204)]
        [ProducesError(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();
            var result = await _deleteNotification.Execute(userId, id, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return NoContent();
        }

    }
}
