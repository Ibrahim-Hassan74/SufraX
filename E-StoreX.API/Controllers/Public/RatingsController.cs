using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Ratings.Requests;
using EStoreX.Core.DTO.Ratings.Response;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Ratings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;
using System.Security.Claims;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Handles all operations related to product ratings,
    /// including creating, updating, deleting, and retrieving ratings and summaries.
    /// </summary>
    public class RatingsController : CustomControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        /// <summary>
        /// Initializes a new instance of <see cref="RatingsController"/>.
        /// </summary>
        /// <param name="ratingService">The service responsible for rating operations.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public RatingsController(IRatingService ratingService, IStringLocalizer<SharedResource> localizer)
        {
            _ratingService = ratingService;
            _localizer = localizer;
        }

        /// <summary>
        /// Adds a new rating for a given product by the logged-in user.
        /// </summary>
        /// <param name="request">The rating details including score, comment, and product ID.</param>
        /// <returns>The created rating as a <see cref="RatingResponse"/>.</returns>
        /// <response code="200">Successfully added the rating.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(RatingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RatingResponse>> AddRating([FromBody] RatingAddRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _ratingService.AddRatingAsync(request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing rating owned by the logged-in user.
        /// </summary>
        /// <param name="id">The ID of the rating to update.</param>
        /// <param name="request">The updated rating details.</param>
        /// <returns>The updated rating as a <see cref="RatingResponse"/> or 404 if not found.</returns>
        /// <response code="200">Successfully updated the rating.</response>
        /// <response code="404">Rating not found or not owned by user.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(RatingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RatingResponse>> UpdateRating(Guid id, [FromBody] RatingUpdateRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _ratingService.UpdateRatingAsync(id, request, userId);
            if (result == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["RatingNotFoundOrNotOwned"].Value));
            return Ok(result);
        }

        /// <summary>
        /// Deletes a rating owned by the logged-in user.
        /// </summary>
        /// <param name="id">The ID of the rating to delete.</param>
        /// <returns>No content if successful, otherwise 404 if not found.</returns>
        /// <response code="204">Rating successfully deleted.</response>
        /// <response code="404">Rating not found or not owned by user.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteRating(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _ratingService.DeleteRatingAsync(id, userId);
            if (!success)
                return NotFound(ApiResponseFactory.NotFound(_localizer["RatingNotFoundOrNotOwned"].Value));
            return NoContent();
        }

        /// <summary>
        /// Retrieves all ratings for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>A list of ratings as <see cref="RatingResponse"/>.</returns>
        /// <response code="200">Successfully retrieved product ratings.</response>
        [HttpGet("product/{productId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RatingResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RatingResponse>>> GetRatingsForProduct(Guid productId)
        {
            var result = await _ratingService.GetRatingsForProductAsync(productId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a summary of ratings for a specific product,
        /// including average score and total number of ratings.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>A <see cref="ProductRatingResponse"/> containing rating summary.</returns>
        /// <response code="200">Successfully retrieved product rating summary.</response>
        [HttpGet("product/{productId:guid}/summary")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductRatingResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductRatingResponse>> GetProductRatingSummary(Guid productId)
        {
            var result = await _ratingService.GetProductRatingSummaryAsync(productId);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves the current logged-in user's rating for a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>
        /// An <see cref="ActionResult{RatingResponse}"/> containing the user's rating,
        /// or a 404 Not Found response if the user hasn't rated the product yet.
        /// </returns>
        /// <response code="200">Successfully retrieved the user's rating.</response>
        /// <response code="404">No rating found for this product by the user.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("product/{productId:guid}/my-rating")]
        [Authorize]
        [ProducesResponseType(typeof(RatingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RatingResponse>> GetMyRatingForProduct(Guid productId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _ratingService.GetUserRatingForProductAsync(productId, userId);

            //if (result == null)
            //    return NotFound(ApiResponseFactory.NotFound("No rating found for this product by the user."));

            return Ok(result);
        }


    }
}
