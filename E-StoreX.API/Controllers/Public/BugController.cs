using EStoreX.Core.DTO.Common;
using Microsoft.AspNetCore.Mvc;
using EStoreX.Core.Helper;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Controller for handling bug-related operations.
    /// </summary>
    public class BugController : CustomControllerBase
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        public BugController(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }
        /// <summary>
        /// Returns a 500 Internal Server Error response.
        /// </summary>
        /// <returns>A 500 status code with a message indicating an internal server error.</returns>
        [HttpGet("error")]
        public ActionResult<ApiResponse> GetError()
        {
            return ApiResponseFactory.InternalServerError(_localizer["InternalServerError"].Value);
        }

        /// <summary>
        /// Returns a 404 Not Found response.
        /// </summary>
        /// <returns>A 404 status code with a message indicating the resource was not found.</returns>
        [HttpGet("not-found")]
        public ActionResult<ApiResponse> GetNotFound()
        {
            return ApiResponseFactory.BadRequest(_localizer["ResourceNotFound"].Value);
        }
        ///  <summary>
        ///  Returns a 400 Bad Request response.
        ///  </summary>
        ///  <returns>A 400 status code with a message indicating the resource was bad-request</returns>
        [HttpGet("bad-request/{Id:guid}")]
        public ActionResult<ApiResponse> GetBadRequest(Guid Id)
        {
            return ApiResponseFactory.BadRequest(_localizer["BadRequestWithId", Id].Value);
        }
        ///  <summary>
        ///  Returns a 400 Bad Request response.
        ///  </summary>
        ///  <returns>A 400 status code with a message indicating the resource was bad-request</returns>
        [HttpGet("bad-request")]
        public ActionResult<ApiResponse> GetBadRequest()
        {
            return ApiResponseFactory.BadRequest();
        }
    }
}
