
namespace Host.ProblemDetailsNamespace
{
    public class ExtendedProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public string ErrorCode { get; set; } = string.Empty;
    }
}
