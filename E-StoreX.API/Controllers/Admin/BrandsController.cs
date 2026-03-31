using Asp.Versioning;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Products;
using EStoreX.Core.Services.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// Provides administrative operations for managing product brands in the E-StoreX platform.
    /// </summary>
    /// <remarks>
    /// This controller is restricted to <b>Admin users</b> only and supports API version 2.0.  
    /// It allows creating, updating, deleting, and retrieving brands, as well as assigning them to categories.  
    /// <para>
    /// Inherits from <see cref="AdminControllerBase"/>.  
    /// </para>
    /// </remarks>
    [ApiVersion(2.0)]
    public class BrandsController : AdminControllerBase
    {
        private readonly IBrandService _brandsService;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        /// <summary>
        /// Initializes a new instance of <see cref="BrandsController"/>.
        /// </summary>
        /// <param name="brandsService">Service to manage brand operations.</param>
        /// <param name="exportService">Service to manage files.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public BrandsController(IBrandService brandsService, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _brandsService = brandsService;
            _exportService = exportService;
            _localizer = localizer;
        }

        /// <summary>
        /// Creates a new brand.
        /// </summary>
        /// <param name="name">The name of the brand.</param>
        /// <returns>The created brand.</returns>
        /// <response code="200">Brand created successfully.</response>
        /// <response code="400">Brand name is empty or invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Brand), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(ApiResponseFactory.NotFound(_localizer["BrandNameRequired"].Value));

            var brand = await _brandsService.CreateBrandAsync(name);
            return Ok(brand);
        }

        /// <summary>
        /// Updates an existing brand.
        /// </summary>
        /// <param name="id">The ID of the brand to update.</param>
        /// <param name="newName">The new name for the brand.</param>
        /// <returns>The updated brand.</returns>
        /// <response code="200">Brand updated successfully.</response>
        /// <response code="400">New name is empty or invalid.</response>
        /// <response code="404">Brand not found.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Brand), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Guid id, [FromBody] string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return BadRequest(ApiResponseFactory.NotFound("New name cannot be empty."));

            var updatedBrand = await _brandsService.UpdateBrandAsync(id, newName);

            if (updatedBrand == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["BrandNotFound"].Value));

            return Ok(updatedBrand);
        }

        /// <summary>
        /// Deletes a brand by ID.
        /// </summary>
        /// <param name="id">The ID of the brand to delete.</param>
        /// <response code="204">Brand deleted successfully.</response>
        /// <response code="404">Brand not found.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _brandsService.DeleteBrandAsync(id);
            if (!deleted)
                return NotFound(ApiResponseFactory.NotFound("Brand not found."));

            return NoContent();
        }

        /// <summary>
        /// Exports all brands into the specified file format.
        /// </summary>
        /// <param name="type">
        /// The type of export format.  
        /// Supported values are:  
        /// <list type="bullet">
        ///   <item><description><see cref="ExportType.Csv"/> → Comma Separated Values file (.csv)</description></item>
        ///   <item><description><see cref="ExportType.Excel"/> → Microsoft Excel file (.xlsx)</description></item>
        ///   <item><description><see cref="ExportType.Pdf"/> → Portable Document Format file (.pdf)</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// A downloadable file in the selected export format.  
        /// Returns <see cref="BadRequestResult"/> if the format is not supported.
        /// </returns>
        /// <response code="200">brands exported successfully in the requested format.</response>
        /// <response code="400">Unsupported export type requested.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Export(ExportType type)
        {
            var brands = await _brandsService.GetAllBrandsAsync();

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(brands), "text/csv", "brands.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(brands), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "brands.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(brands), "application/pdf", "brands.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }
        /// <summary>
        /// Deletes a specific brand image by its ID.
        /// </summary>
        /// <param name="brandId">The unique identifier of the brand.</param>
        /// <param name="photoId">The unique identifier of the photo to delete.</param>
        /// <returns>
        /// <c>200 OK</c> if the image was deleted successfully;  
        /// <c>404 Not Found</c> if the brand or the photo was not found;  
        /// <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">Image deleted successfully.</response>
        /// <response code="404">Brand or image not found.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpDelete("{brandId:guid}/images/{photoId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBrandImage(Guid brandId, Guid photoId)
        {
            var response = await _brandsService.DeleteBrandImageAsync(brandId, photoId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Adds images to the specified brand.
        /// </summary>
        /// <param name="brandId">The unique identifier of the brand.</param>
        /// <param name="files">The list of image files to upload.</param>
        /// <returns>
        /// <c>200 OK</c> if the images were added successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the brand does not exist.
        /// </returns>
        /// <response code="200">Images added successfully.</response>
        /// <response code="400">No files were provided.</response>
        /// <response code="404">Brand not found.</response>
        [HttpPost("{brandId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddBrandImages(Guid brandId, [FromForm] List<IFormFile> files)
        {
            var response = await _brandsService.AddBrandImagesAsync(brandId, files);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Updates all images of the specified brand.
        /// </summary>
        /// <remarks>
        /// This will remove all existing images of the brand and replace them with the newly uploaded ones.
        /// </remarks>
        /// <param name="brandId">The unique identifier of the brand.</param>
        /// <param name="files">The list of new image files to upload.</param>
        /// <returns>
        /// <c>200 OK</c> if the images were updated successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the brand does not exist.
        /// </returns>
        /// <response code="200">Images updated successfully.</response>
        /// <response code="400">No files were provided.</response>
        /// <response code="404">Brand not found.</response>
        [HttpPatch("{brandId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBrandImages(Guid brandId, [FromForm] List<IFormFile> files)
        {
            var response = await _brandsService.UpdateBrandImagesAsync(brandId, files);
            return StatusCode(response.StatusCode, response);
        }
    }
}
