using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using EStoreX.Core.DTO.Common;

namespace EStoreX.API.Filters
{
    /// <summary>
    /// A custom action filter that handles model validation errors for account-related actions.
    /// Returns a uniform error response if the ModelState is invalid.
    /// </summary>
    public class AccountValidationFilter : IAsyncActionFilter
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountValidationFilter(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }
        /// <summary>
        /// Executes the action filter asynchronously.
        /// If the model state is invalid, it short-circuits the pipeline and returns a standardized error response.
        /// Otherwise, it allows the request to proceed to the action method.
        /// </summary>
        /// <param name="context">The context for the current action execution.</param>
        /// <param name="next">The delegate to execute the next action filter or action method.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Any())
                    .SelectMany(kvp => kvp.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var response = new ApiErrorResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = _localizer["ValidationFailed"].Value,
                    Errors = errors
                };

                context.Result = new BadRequestObjectResult(response);
                return;
            }

            await next();
        }
    }
}
