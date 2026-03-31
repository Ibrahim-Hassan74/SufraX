using Asp.Versioning;
using Domain.Entities.Baskets;
using EStoreX.API.Filters;
using EStoreX.Core.DTO.Basket;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Basket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Security.Claims;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// API controller for managing customer baskets.
    /// </summary>
    [ApiVersion(1.0)]
    public class BasketsController : CustomControllerBase
    {
        private readonly IBasketService _basketService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="BasketsController"/> class.
        /// </summary>
        /// <param name="basketService">basket service</param>
        /// <param name="localizer">localizer</param>
        public BasketsController(IBasketService basketService, IStringLocalizer<SharedResource> localizer)
        {
            _basketService = basketService;
            _localizer = localizer;
        }

        /// <summary>
        /// Retrieves a customer basket by ID.
        /// </summary>
        /// <param name="id">
        /// The customer ID (must be a valid GUID).
        /// </param>
        /// <returns>
        /// Returns the basket data or an error response depending on the outcome.
        /// </returns>
        /// <response code="200">
        /// Basket found. Returns <see cref="CustomerBasketDTO"/> containing the basket details.
        /// </response>
        /// <response code="400">
        /// Bad Request – The provided ID is null or not a valid GUID format.
        /// </response>
        /// <response code="404">
        /// Not Found – No basket exists for the given ID.
        /// </response>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerBasketDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBasket(string id)
        {
            if (!Guid.TryParse(id, out _))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidIdFormat"].Value));

            var basket = await _basketService.GetBasketAsync(id);
            return Ok(basket);
        }

        /// <summary>
        /// Adds or updates a customer basket.
        /// </summary>
        /// <param name="basket">
        /// The basket to add or update, represented as a <see cref="CustomerBasketDTO"/>.
        /// </param>
        /// <returns>
        /// Returns the updated or newly created basket, or an error response depending on the outcome.
        /// </returns>
        /// <response code="200">
        /// OK – Basket successfully created or updated.  
        /// Returns <see cref="CustomerBasketDTO"/> containing the basket details.
        /// </response>
        /// <response code="400">
        /// Bad Request – The basket ID format is invalid, or there are no valid items to update.
        /// </response>

        [HttpPost]
        [ProducesResponseType(typeof(CustomerBasketDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddOrUpdateBasket([FromBody] BasketAddRequest basket)
        {
            if (!Guid.TryParse(basket.BasketId, out _))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidIdFormat"]));

            var updatedBasket = await _basketService.AddItemToBasketAsync(basket);

            if (updatedBasket == null)
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["NoValidItems"]));

            return Ok(updatedBasket);
        }

        /// <summary>
        /// Deletes a customer basket by ID.
        /// </summary>
        /// <param name="id">
        /// The customer basket ID (must be a valid GUID).
        /// </param>
        /// <returns>
        /// Returns a success or error response depending on the outcome.
        /// </returns>
        /// <response code="200">
        /// OK – Basket successfully deleted.  
        /// Returns <see cref="ApiResponse"/> with a success message.
        /// </response>
        /// <response code="400">
        /// Bad Request – The basket ID format is invalid.
        /// </response>
        /// <response code="404">
        /// Not Found – No basket exists with the given ID or it has already been deleted.
        /// </response>

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBasket(string id)
        {
            if (!Guid.TryParse(id, out _))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidIdFormat"]));

            var result = await _basketService.DeleteBasketAsync(id);

            return result
                ? Ok(ApiResponseFactory.Success(_localizer["ItemDeleted"]))
                : NotFound(ApiResponseFactory.NotFound(_localizer["BasketNotFoundOrDeleted"]));
        }
        /// <summary>
        /// Merges the guest basket (created before login) with the authenticated user's basket.
        /// </summary>
        /// <param name="guestId">
        /// The basket ID generated for the guest session before login (must be a valid GUID).
        /// </param>
        /// <returns>
        /// A merged basket if the operation succeeds.
        /// </returns>
        /// <response code="200">
        /// OK – Returns the merged <see cref="CustomerBasketDTO"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – The guest basket ID format is invalid.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not logged in.
        /// </response>

        [Authorize]
        [HttpPost("merge")]
        [ProducesResponseType(typeof(CustomerBasketDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MergeBasket(string guestId)
        {
            if (!Guid.TryParse(guestId, out _))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidIdFormat"]));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseFactory.Unauthorized(_localizer["UserNotLoggedIn"]));

            var mergedBasket = await _basketService.MergeBasketsAsync(guestId, userId);

            return Ok(mergedBasket);
        }

        /// <summary>
        /// Decreases the quantity of a specific item in the customer's basket by 1.  
        /// If the quantity reaches zero, the item will be removed from the basket.
        /// </summary>
        /// <param name="basketId">
        /// The unique identifier of the basket (must be a valid GUID).
        /// </param>
        /// <param name="productId">
        /// The unique identifier of the product to decrease the quantity for.
        /// </param>
        /// <returns>
        /// The updated basket if the operation succeeds.
        /// </returns>
        /// <response code="200">
        /// OK – Returns the updated <see cref="CustomerBasketDTO"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – The basket ID format is invalid.
        /// </response>
        /// <response code="404">
        /// Not Found – The basket or item does not exist.
        /// </response>
        [HttpPatch("{basketId}/items/{productId}/decrease")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomerBasketDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> DecreaseItemQuantity(string basketId, Guid productId)
        {
            if (!Guid.TryParse(basketId, out _))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidBasketIdFormat"]));

            var updatedBasket = await _basketService.DecreaseItemQuantityAsync(basketId, productId);

            if (updatedBasket == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["BasketOrItemNotFound"]));

            return Ok(updatedBasket);
        }

        /// <summary>
        /// Removes a specific item from the customer's basket.
        /// </summary>
        /// <param name="basketId">
        /// The unique identifier of the basket (must be a valid GUID).
        /// </param>
        /// <param name="productId">
        /// The unique identifier of the product to remove.
        /// </param>
        /// <returns>
        /// The updated basket if the operation succeeds.
        /// </returns>
        /// <response code="200">
        /// OK – Returns the updated <see cref="CustomerBasketDTO"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – The basket ID format is invalid.
        /// </response>
        /// <response code="404">
        /// Not Found – The basket or item does not exist.
        /// </response>
        [HttpDelete("{basketId}/items/{productId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomerBasketDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveItemFromBasket(string basketId, Guid productId)
        {
            if (!Guid.TryParse(basketId, out _))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidBasketIdFormat"]));

            var updatedBasket = await _basketService.RemoveItemAsync(basketId, productId);

            if (updatedBasket == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["BasketOrItemNotFound"]));

            return Ok(updatedBasket);
        }

        /// <summary>
        /// Increases the quantity of a specific item in the customer's basket.
        /// </summary>
        /// <param name="basketId">The identifier of the basket.</param>
        /// <param name="productId">The identifier of the product to increase quantity for.</param>
        /// <response code="400">Returned if the basket id format is invalid.</response>
        /// <response code="404">Returned if the basket or item is not found.</response>
        /// <response code="200">Returned with the updated basket.</response>
        [HttpPatch("{basketId}/items/{productId}/increase")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomerBasketDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> IncreaseItemQuantity(string basketId, Guid productId)
        {
            if (!Guid.TryParse(basketId, out _))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidBasketIdFormat"]));

            var updatedBasket = await _basketService.IncreaseItemQuantityAsync(basketId, productId);

            if (updatedBasket == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["BasketOrItemNotFound"]));

            return Ok(updatedBasket);
        }
        /// <summary>
        /// Applies a discount code to a customer's basket.
        /// </summary>
        /// <param name="basketId">The unique identifier of the basket to which the discount will be applied.</param>
        /// <param name="code">The discount code provided by the customer.</param>
        /// <returns>
        /// Returns <see cref="CustomerBasketDTO"/> with the updated basket if the discount is successfully applied.  
        /// Returns <see cref="ApiResponse"/> with 404 status if the basket or discount code is not found.
        /// </returns>
        /// <response code="200">The basket with the discount successfully applied.</response>
        /// <response code="404">Basket or discount not found.</response>
        /// <remarks>
        /// This endpoint requires authentication.  
        /// Example request:  
        /// POST /api/basket/{basketId}/apply-discount/{code}
        /// </remarks>

        [HttpPost("{basketId}/apply-discount/{code}")]
        [ProducesResponseType(typeof(CustomerBasketDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> ApplyDiscount(string basketId, string code)
        {
            var basket = await _basketService.ApplyDiscountAsync(basketId, code);

            if (basket == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["BasketOrDiscountNotFound"]));

            return Ok(basket);
        }


    }
}
