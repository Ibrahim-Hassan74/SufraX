using Asp.Versioning;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Favourites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_StoreX.API.Controllers.Public
{
    /// <summary>
    /// Controller responsible for managing user's favourite products.
    /// Provides endpoints to add, remove, and retrieve favourites for the authenticated user.
    /// </summary>
    [Authorize]
    [ApiVersion(1.0)]
    public class FavouritesController : CustomControllerBase
    {
        private readonly IFavouriteService _favouriteService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavouritesController"/> class.
        /// </summary>
        /// <param name="favouriteService">The service that handles favourite-related operations.</param>
        public FavouritesController(IFavouriteService favouriteService)
        {
            _favouriteService = favouriteService;
        }

        /// <summary>
        /// Adds a product to the authenticated user's list of favourites.
        /// </summary>
        /// <param name="productId">The unique identifier of the product to add.</param>
        /// <returns>
        /// <c>200 OK</c> if the product was successfully added to favourites;
        /// <c>401 Unauthorized</c> if the user is not authenticated or the user ID is invalid;
        /// <c>409 Conflict</c> if the product is already in the user's favourites;
        /// <c>500 Internal Server Error</c> for unexpected server errors.
        /// </returns>
        /// <response code="200">Product successfully added to favourites.</response>
        /// <response code="401">User is not authenticated or has an invalid identifier.</response>
        /// <response code="409">The product is already in the user's favourites.</response>
        [HttpPost("{productId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddToFavourite(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseFactory.Unauthorized("User not found"));
            if(!Guid.TryParse(userId, out _))
                return Unauthorized(ApiResponseFactory.Unauthorized("Invalid user identifier"));
            var userIdGuid = Guid.Parse(userId);
            var response = await _favouriteService.AddToFavouriteAsync(userIdGuid, productId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Removes a product from the authenticated user's list of favourites.
        /// </summary>
        /// <param name="productId">The unique identifier of the product to remove.</param>
        /// <returns>
        /// <c>200 OK</c> if the product was successfully removed from favourites;
        /// <c>401 Unauthorized</c> if the user is not authenticated or the user ID is invalid;
        /// <c>404 Not Found</c> if the product is not in the user's favourites;
        /// <c>500 Internal Server Error</c> for unexpected server errors.
        /// </returns>
        /// <response code="200">Product successfully removed from favourites.</response>
        /// <response code="401">User is not authenticated or has an invalid identifier.</response>
        /// <response code="404">The product was not found in the user's favourites.</response>
        [HttpDelete("{productId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromFavourite(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseFactory.Unauthorized("User not found"));
            if (!Guid.TryParse(userId, out _))
                return Unauthorized(ApiResponseFactory.Unauthorized("Invalid user identifier"));
            var userIdGuid = Guid.Parse(userId);
            var response = await _favouriteService.RemoveFromFavouriteAsync(userIdGuid, productId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Retrieves all products in the authenticated user's favourites list.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with the list of favourites if the user is authenticated;
        /// <c>401 Unauthorized</c> if the user is not authenticated or the user ID is invalid;
        /// <c>500 Internal Server Error</c> for unexpected server errors.
        /// </returns>
        /// <response code="200">Successfully retrieved the list of favourite products.</response>
        /// <response code="401">User is not authenticated or has an invalid identifier.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserFavourites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseFactory.Unauthorized("User not found"));
            if (!Guid.TryParse(userId, out _))
                return Unauthorized(ApiResponseFactory.Unauthorized("Invalid user identifier"));
            var userIdGuid = Guid.Parse(userId);

            var response = await _favouriteService.GetUserFavouritesAsync(userIdGuid);
            return Ok(response);
        }
    }
}
