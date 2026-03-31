using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Discount.Request;
using EStoreX.Core.DTO.Discount.Response;
using EStoreX.Core.ServiceContracts.Discount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Provides endpoints for clients to apply and validate discount codes.
    /// </summary>
    [Authorize]
    public class DiscountsController : CustomControllerBase
    {
        private readonly IDiscountService _discountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountsController"/> class.
        /// </summary>
        /// <param name="discountService">Service for managing discounts.</param>
        public DiscountsController(IDiscountService discountService)
        {
            _discountService = discountService;
        }
        ///// <summary>
        ///// Applies a discount code to a specific product.
        ///// </summary>
        ///// <param name="request">The request containing the product ID and discount code.</param>
        ///// <returns>
        ///// The result of the discount application, including the discounted price if valid.
        ///// </returns>
        ///// <response code="200">Discount applied successfully.</response>
        ///// <response code="400">Invalid or inactive discount code.</response>
        ///// <response code="404">Product not found.</response>
        //[HttpPost("apply")]
        //[ProducesResponseType(typeof(ApiResponseWithData<AppliedDiscountResponse>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> Apply([FromBody] ApplyDiscountRequest request)
        //{
        //    var result = await _discountService.ApplyDiscountToProductAsync(request.ProductId, request.Code);
        //    return StatusCode(result.StatusCode, result);
        //}

        /// <summary>
        /// Validates a discount code to check if it is active and usable.
        /// </summary>
        /// <param name="code">The discount code to validate.</param>
        /// <returns>Status of the discount code validation.</returns>
        /// <response code="200">Discount code is valid.</response>
        /// <response code="400">Discount code is invalid or inactive.</response>
        [HttpGet("validate/{code}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Validate(string code)
        {
            var result = await _discountService.ValidateDiscountCodeAsync(code);
            return StatusCode(result.StatusCode, result);
        }
    }
}
