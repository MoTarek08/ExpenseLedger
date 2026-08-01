
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Exceptions.AuthorizationExceptions;
using Application.Exceptions.StorageExceptions.CheckViolationNamespace;
using Application.Exceptions.StorageExceptions.ForeignKeyViolation;
using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Exceptions.StorageExceptions.UniqueViolationNamespace;
using Domain.ExceptionsNamespace;
using Host.ProblemDetails.Problems;
using Host.ProblemDetailsNamespace;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
namespace Host.MiddlewaresNamespace
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var path = httpContext.Request.Path;
            ExtendedProblemDetails? problem = null;

            if (exception is UniqueViolation uqEx)
            {
                problem = ProblemDetailsGenerator.Generate(
                    UniqueViolation.Title,
                    uqEx.Detail,
                    uqEx.ErrorCode,
                    UniqueViolation.Status,
                    path);

                httpContext.Response.StatusCode = 409;
                _logger.LogWarning("Unique constraint violated {ExceptionMessage} {ExceptionErrorCode}", uqEx.Message, uqEx.ErrorCode);
            }

            else if (exception is ForeginKeyViolation fkEx)
            {
                problem = ProblemDetailsGenerator.Generate(
                    fkEx.Title,
                    fkEx.Detail,
                    fkEx.ErrorCode,
                    fkEx.Status,
                    path);
                httpContext.Response.StatusCode = fkEx.Status;
                _logger.LogWarning("Foreign key constraint violated {ExceptionMessage} {ExceptionErrorCode}", fkEx.Detail, fkEx.ErrorCode);
            }

            else if (exception is CheckViolation cvEx)
            {
                problem = ProblemDetailsGenerator.Generate(
                    CheckViolation.Title,
                    cvEx.Detail,
                    cvEx.ErrorCode,
                    CheckViolation.Status,
                    path);
                httpContext.Response.StatusCode = CheckViolation.Status;
                _logger.LogWarning("Check constraint violated {ExceptionMessage} {ExceptionErrorCode}", cvEx.Detail, cvEx.ErrorCode);
            }

            else if (exception is InvalidAccessToken iatEx)
            {
                problem = ProblemDetailsGenerator.Generate(
                    InvalidAccessToken.Title,
                    iatEx.Detail,
                    iatEx.ErrorCode,
                    InvalidAccessToken.Status,
                    path);
                httpContext.Response.StatusCode = 401;
                _logger.LogWarning("Invalid access token {ExceptionMessage} {ExceptionErrorCode}", iatEx.Detail, iatEx.ErrorCode);
            }

            else if (exception is DomainException domainEx)
            {
                problem = ProblemDetailsGenerator.Generate(
                   "Domain rule violation",
                    domainEx.Message,
                    domainEx.ErrorCode,
                    400,
                    path
                    );
                httpContext.Response.StatusCode = 400;
                _logger.LogWarning("Domain rule violated {ExceptionMessage} {ExpcetionErrorCode}", domainEx.Message, domainEx.ErrorCode);
            }

            else if (exception is OperationCanceledException opCancelledEx)
            {
                _logger.LogInformation("Request was aborted {ExceptionMessage}", opCancelledEx.Message); // Path is already included
                return true;
            }


            else if (exception is NpgsqlException)
            {
                problem = ProblemDetailsGenerator.Generate(AllProblems.Get(StorageErrorCodes.BAD_DB_CONNECTION), path);
                httpContext.Response.StatusCode = 503;
                _logger.LogError(exception, "Bad database connection {ExceptionErrorCode}", StorageErrorCodes.BAD_DB_CONNECTION);
            }

            else
            {
                problem = ProblemDetailsGenerator.Generate(AllProblems.Get(StorageErrorCodes.INTERNAL_SERVER_ERROR), path);
                httpContext.Response.StatusCode = 500;
                _logger.LogError(exception, "Unhandled exception");
            }


            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;

        }
    }

}
