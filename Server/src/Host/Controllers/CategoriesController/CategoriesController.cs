using Asp.Versioning;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Models;
using Application.UseCases.CategoriesUseCases.GetAllCategories;
using Application.UseCases.CategoriesUseCases.GetCategoryByCode;
using Application.UseCases.CategoriesUseCases.GetCategoryByCode.Models;
using Host.Attributes;
using Host.Controllers.ControllersExtenstions;
using Host.ProblemDetails.Problems;
using Host.Validation.Validators;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Host.Swagger.ResponsesExamples;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Controllers.CategoriesControllerNamespace
{
    [Route("api/v{version:ApiVersion}/categories")]
    [ApiController]
    [ApiVersion("1.0")]
    public class CategoriesController : ControllerBase
    {
        private readonly GetAllCategoriesUseCase _getAllCategoriesUseCase;
        private readonly GetCategoryByCodeUseCase _getCategoryByCodeUseCase;

        public CategoriesController(
            GetAllCategoriesUseCase getAllCategoriesUseCase,
            GetCategoryByCodeUseCase getCategoryByCodeUseCase)
        {
            _getAllCategoriesUseCase = getAllCategoriesUseCase;
            _getCategoryByCodeUseCase = getCategoryByCodeUseCase;
        }

        ///<summary>
        /// Get all expense categories
        ///</summary>
        ///<remarks>
        /// Returns all expense categories with their sub-categories, ordered by code.
        /// This endpoint is restricted to administrators.
        ///</remarks>
        [HttpGet]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        [SwaggerResponse(200, Type = typeof(List<CategoryDetails>))]
        [SwaggerResponseExample(200, typeof(CategoryDetailsListExample))]
        public async Task<ActionResult<List<CategoryDetails>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _getAllCategoriesUseCase.Execute(cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }

        ///<summary>
        /// Get a category by its code
        ///</summary>
        ///<remarks>
        /// Returns a single expense category with its sub-categories for the given code.
        /// This endpoint is restricted to administrators.
        ///</remarks>
        [HttpGet("{code}")]
        [Authorize(Roles = AuthorizationConstants.Admin)]
        [SwaggerResponse(200, Type = typeof(CategoryDetails))]
        [SwaggerResponseExample(200, typeof(CategoryDetailsExample))]
        [ProducesError(CategoriesErrorCodes.CATEGORY_NOT_FOUND)]
        public async Task<ActionResult<CategoryDetails>> GetByCode(
            [FromRoute] GetCategoryByCodeRequestModel model,
            [FromServices] GetCategoryByCodeRequestModelValidator validator,
            CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                return this.ValidationFailureResponse(validationResult.Errors);

            var result = await _getCategoryByCodeUseCase.Execute(model.Code, cancellationToken);

            if (result.IsFailure)
                return this.FromProblem(AllProblems.Get(result.Error!.Code));

            return Ok(result.Data);
        }
    }
}
