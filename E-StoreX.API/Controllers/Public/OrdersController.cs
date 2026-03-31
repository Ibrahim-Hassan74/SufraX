using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;
using System.Security.Claims;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Controller responsible for handling order-related operations.
    /// Requires the user to be authenticated.
    /// </summary>
    [Authorize]
    public class OrdersController : CustomControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="orderService">The service that handles order-related operations.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public OrdersController(IOrderService orderService, IStringLocalizer<SharedResource> localizer)
        {
            _orderService = orderService;
            _localizer = localizer;
        }

        /// <summary>
        /// Creates a new order for the authenticated user.
        /// </summary>
        /// <param name="order">The order data to be created.</param>
        /// <returns>
        /// <c>200 OK</c> with the created order details if successful;
        /// <c>400 Bad Request</c> if the user email is not found in the JWT token;
        /// <c>500 Internal Server Error</c> for unexpected errors.
        /// </returns>
        /// <response code="200">Returns the newly created order.</response>
        /// <response code="400">If the user email is not present in the JWT token.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateOrder(OrderAddRequest order)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return NotFound(ApiResponseFactory.NotFound(_localizer["UserEmailNotFoundInToken"].Value));

            var createdOrder = await _orderService.CreateOrdersAsync(order, email);

            return Ok(createdOrder);
        }
        /// <summary>
        /// Retrieves all orders associated with the currently authenticated user.
        /// </summary>
        /// <returns>A list of <see cref="OrderResponse"/> objects representing the user's orders.</returns>
        /// <response code="200">Returns the list of user's orders.</response>
        /// <response code="400">If the user email is missing or invalid in the token.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrders()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return NotFound(ApiResponseFactory.NotFound(_localizer["InvalidUserCredentials"].Value));
            var orderResponse = await _orderService.GetAllOrdersAsync(email);
            return Ok(orderResponse);
        }

        /// <summary>
        /// Retrieves a specific order by its ID for the currently authenticated user.
        /// </summary>
        /// <param name="Id">The unique identifier of the order.</param>
        /// <returns>
        /// Returns <see cref="OrderResponse"/> if the order exists and belongs to the user; 
        /// otherwise, returns <c>null</c>.
        /// </returns>
        /// <response code="200">Returns the requested order (can be null if not found).</response>
        /// <response code="400">If the user email is missing in the JWT token.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("{Id:guid}")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrdersById(Guid Id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return NotFound(ApiResponseFactory.NotFound(_localizer["InvalidUserCredentials"].Value));
            var orderResponse = await _orderService.GetOrderByIdAsync(Id, email);
            return Ok(orderResponse);
        }

        /// <summary>
        /// Retrieves all available delivery methods.
        /// </summary>
        /// <returns>
        /// A list of <see cref="DeliveryMethodResponse"/> objects representing delivery options.
        /// </returns>
        /// <response code="200">Successfully retrieved the delivery methods.</response>
        [HttpGet("delivery-methods")]
        [ProducesResponseType(typeof(IEnumerable<DeliveryMethodResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeliveryMethods()
        {
            var methods = await _orderService.GetDeliveryMethodAsync();
            return Ok(methods);
        }

    }
}
