using Application.ErrorNamespace.ErrorCodesNamespace;
using FluentValidation.Results;
using Host.ProblemDetails.Problems;
using Host.ProblemDetailsNamespace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;

namespace Host.SetupExtensions
{
    public static class ModelStateProblemFactory
    {
        public static ExtendedProblemDetails Create(ActionContext context)
        {
            var path = context.HttpContext.Request.Path;
            var failures = new List<ValidationFailure>();
            var actionParametersNames = context.ActionDescriptor.Parameters
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in context.ModelState)
            {
                if (actionParametersNames.Contains(entry.Key))
                    continue;

                foreach (var error in entry.Value!.Errors)
                {
                    if (IsMalformedJson(error.ErrorMessage))
                        return ProblemDetailsGenerator.Generate(AllProblems.Get(OtherErrorCodes.INVALID_JSON_FORMAT), path);

                    failures.Add(BuildFailure(entry.Key, error));
                }
            }

            return ProblemDetailsGenerator.GenerateValidationFailureDetails(path,failures);
        }

        private static ValidationFailure BuildFailure(string key, ModelError error)
        {
            var message = error.ErrorMessage;

            if (IsInvalidType(message))
                return new ValidationFailure( ExtractPropertyName(message, key),"The provided value has an invalid type.");

            return new ValidationFailure(key, message);
        }

        private static string ExtractPropertyName(string message, string key)
        {
            var match = Regex.Match(message, @"Path:\s*\$\.(.*?)\s*\|");

            if (match.Success)
                return match.Groups[1].Value;

            return key.TrimStart('$','.');
        }
        private static bool IsMalformedJson(string message)
        {
            return message.Contains("JSON") ||
                   message.Contains("Expected depth") ||
                   message.Contains("',' is invalid after") ||
                   message.Contains("'}' is invalid after");
        }

        private static bool IsInvalidType(string message) => message.Contains("could not be converted to");
    }
}
