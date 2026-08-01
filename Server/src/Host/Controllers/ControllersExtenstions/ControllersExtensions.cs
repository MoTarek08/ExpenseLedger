using Application.Exceptions.AuthorizationExceptions;
using Domain.Entities.DomainEnums;
using FluentValidation.Results;
using Host.ProblemDetailsNamespace;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Host.Controllers.ControllersExtenstions
{

    public static class ControllersExtensions 
    {
        public static BadRequestObjectResult ValidationFailureResponse(
            this ControllerBase controller,
            List<ValidationFailure> failures)
        {

            var problem = ProblemDetailsGenerator.GenerateValidationFailureDetails(
                controller.HttpContext.Request.Path,
                failures);
          
            return controller.BadRequest(problem);
        }

        public static Guid GetUserIdFromClaims(this ControllerBase controller) {

            var isParsed = Guid.TryParse(controller.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
            if (!isParsed)
                throw new InvalidAccessToken();
            return id;
        }

        public static Role GetUserRoleFromClaims(this ControllerBase controller)
        {

            var isParsed = Enum.TryParse(controller.User.FindFirstValue(ClaimTypes.Role), out Role role);
            if (!isParsed)
                throw new InvalidAccessToken();

            return role;
        }

        public static ObjectResult FromProblem(this ControllerBase controller, ProblemDefinition problemDefinition)
        {
            var problem = ProblemDetailsGenerator.Generate(problemDefinition, controller.Request.Path);
            return controller.StatusCode(problem.Status!.Value, problem);
        }



    }
}
