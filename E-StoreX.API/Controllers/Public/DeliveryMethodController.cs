using Asp.Versioning;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// DeliveryMethodController handles operations related to delivery methods.
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/delivery-method")]
    [ApiVersion(1.0)]
    public class DeliveryMethodController : CustomControllerBase
    {
        private readonly IDeliveryMethodService _deliveryMethod;
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryMethodController"/> class.
        /// </summary>
        /// <param name="deliveryMethod">object of <see cref="IDeliveryMethodService"/></param>
        public DeliveryMethodController(IDeliveryMethodService deliveryMethod)
        {
            _deliveryMethod = deliveryMethod;
        }

        /// <summary>
        /// Retrieves all available delivery methods.
        /// </summary>
        /// <returns>
        /// A list of <see cref="DeliveryMethodResponse"/> objects representing delivery options.
        /// </returns>
        /// <response code="200">Successfully retrieved the delivery methods.</response>
        /// <response code="401">Unauthorized user.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DeliveryMethodResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDeliveryMethods()
        {
            var methods = await _deliveryMethod.GetAllAsync();
            return Ok(methods);
        }
        /// <summary>
        /// Retrieves a delivery method by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the delivery method.</param>
        /// <returns>The delivery method with the specified ID.</returns>
        /// <response code="200">Returns the delivery method.</response>
        /// <response code="404">Delivery method not found.</response>
        /// <response code="401">Unauthorized user.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DeliveryMethodResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var method = await _deliveryMethod.GetByIdAsync(id);
            if (method == null)
                return NotFound(ApiResponseFactory.NotFound());
            return Ok(method);
        }
        /// <summary>
        /// Gets a delivery method by its name.
        /// </summary>
        /// <param name="name">The exact name of the delivery method to search for.</param>
        /// <returns>The delivery method with the specified NameEn</returns>
        /// <response code="200">Returns the delivery method.</response>
        /// <response code="404">Delivery method not found.</response>
        /// <response code="401">Unauthorized user.</response>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(typeof(DeliveryMethodResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByName(string name)
        {
            var method = await _deliveryMethod.GetByNameAsync(name);
            if (method == null)
                return NotFound(ApiResponseFactory.NotFound());
            return Ok(method);
        }
    }
}
