
using Application.ErrorNamespace.ErrorCodesNamespace;
using FluentValidation.Results;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;
using Host.ProblemDetails.Problems;
using Host.Validators.ValidatonError;


namespace Host.ProblemDetailsNamespace
{
    public static class ProblemDetailsGenerator
    {


        public static ExtendedProblemDetails Generate(string title,
            string detail,
            string errorCode,
            int status,
            string instance)
        {
            return new ExtendedProblemDetails()
            {
                Title = title,
                Detail = detail,
                ErrorCode = errorCode,
                Status = status,
                Instance = instance,
            };

        }

        public static ExtendedProblemDetails Generate(ProblemDefinition problemDefinition, string instance)
        {
            return new ExtendedProblemDetails()
            {
                Title = problemDefinition.Title,
                Detail = problemDefinition.Detail,
                ErrorCode = problemDefinition.ErrorCode,
                Status = problemDefinition.StatusCode,
                Instance = instance,
            };
        }

        public static ExtendedProblemDetails GenerateValidationFailureDetails(
            string instance,
            IEnumerable<ValidationFailure> validationFailures)
        {
            var problem = Generate(AllProblems.Get(OtherErrorCodes.VALIDATION_ERROR),instance);

            var failuresDetails = new List<ValidationErrorDetails>();
            foreach (var error in validationFailures)
            {
                failuresDetails.Add(new ValidationErrorDetails(error.PropertyName, error.ErrorMessage));
            }

            problem.Extensions["ErrorFields"] = failuresDetails;
            return problem;
        }
    }
}
