using Asp.Versioning;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Ratings.Response;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Ratings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// API controller for managing ratings in the admin area.  
    /// Provides endpoints for administrators to review, moderate, and delete ratings.  
    /// </summary>
    [ApiVersion(2.0)]
    public class RatingsController : AdminControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingsController"/> class.
        /// </summary>
        /// <param name="ratingService">
        /// The rating service that provides operations for managing ratings.
        /// </param>
        /// <param name="localizer">localizer for shared resources.</param>
        public RatingsController(IRatingService ratingService, IStringLocalizer<SharedResource> localizer)
        {
            _ratingService = ratingService;
            _localizer = localizer;
        }

        /// <summary>
        /// Deletes a rating.  
        /// Admins can delete any rating, while normal users can only delete their own.
        /// </summary>
        /// <param name="id">The ID of the rating to delete.</param>
        /// <returns>No content if successful, otherwise 404 if not found.</returns>
        /// <response code="204">Rating successfully deleted.</response>
        /// <response code="404">Rating not found.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to delete this rating.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteRatingAsAdmin(Guid id)
        {
            var success = await _ratingService.DeleteRatingAsAdminAsync(id);

            if (!success)
                return NotFound(ApiResponseFactory.NotFound(_localizer["RatingNotFound"].Value));

            return NoContent();
        }
        /// <summary>
        /// Retrieves all ratings with detailed information for admin users.  
        /// The response includes the rating ID, product details (name, brand, category),  
        /// the user who wrote the comment, the comment content, and the score.
        /// </summary>
        /// <returns>A list of ratings with related product and user information.</returns>
        /// <response code="200">Returns the list of ratings with details.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to view ratings.</response>
        [HttpGet("ratings")]
        [ProducesResponseType(typeof(IEnumerable<AdminRatingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllRatingsForAdmin()
        {
            var result = await _ratingService.GetAllRatingsForAdminAsync();
            return Ok(result);
        }



    }
}
