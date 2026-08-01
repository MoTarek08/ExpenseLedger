using Asp.Versioning;
using Application.UseCases.BudgetUseCases.GetRemainingBudget;
using Host.Controllers.ControllersExtenstions;
using Host.Models;
using Host.ProblemDetails.Problems;
using Host.Swagger.ResponsesExamples;
using Infrastructure.Authorization.Policies.PloiciesNamesConstantsNamespace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.BudgetController 
{ 
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BudgetController : ControllerBase
    {
        private readonly GetRemainingBudgetUseCase _getRemainingBudgetUseCase;
        public BudgetController(GetRemainingBudgetUseCase getRemainingBudgetUseCase)
        {
            _getRemainingBudgetUseCase = getRemainingBudgetUseCase;
        }

        ///<summary>
        /// Get remaining budget for the current period
        ///</summary>
        ///<remarks>
        /// Returns the remaining budget calculated as the monthly net income minus total expenses
        /// spent since the last reset day. The result is scoped to the authenticated user.
        /// A financial profile must exist before calling this endpoint.
        ///</remarks>
        [HttpGet("remaining")]
        [Authorize(Policy = PoliciesNamesConstants.HasFinancialProfile)]
        [SwaggerResponse(200, Type = typeof(GetRemainingBudgetResponse))]
        [SwaggerResponseExample(200, typeof(GetRemainingBudgetResponseExample))]
        public async Task<ActionResult<GetRemainingBudgetResponse>> GetRemaining(CancellationToken cancellationToken)
        {
            var userId = this.GetUserIdFromClaims();

            var result = await _getRemainingBudgetUseCase.Execute(userId, cancellationToken);
            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(new GetRemainingBudgetResponse(result.Data));
        }
    }
}
