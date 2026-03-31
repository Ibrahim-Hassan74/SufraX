using Asp.Versioning;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Categories.Requests;
using EStoreX.Core.DTO.Categories.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Categories;
using EStoreX.Core.ServiceContracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// Provides administrative operations for managing product categories in the E-StoreX platform.
    /// </summary>
    /// <remarks>
    /// This controller is intended for <b>admin users only</b> and includes endpoints
    /// for creating, updating, deleting, assigning brands, and exporting categories.
    /// <para>
    /// Version: 2.0  
    /// Inherits from <see cref="AdminControllerBase"/>.
    /// </para>
    /// </remarks>
    [ApiVersion(2.0)]

    public class CategoriesController : AdminControllerBase
    {
        private readonly ICategoriesService _categoriesService;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesController"/> class.
        /// </summary>
        /// <param name="exportService">Service for exporting categories to files.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public CategoriesController(ICategoriesService categoriesService, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _categoriesService = categoriesService;
            _exportService = exportService;
            _localizer = localizer;
        }
        /// <summary>
        /// Creates a new category in the database.
        /// </summary>
        /// <param name="categoryDTO">The request object containing category details.</param>
        /// <returns>
        /// Returns <see cref="CategoryResponse"/> if the category was created successfully.  
        /// Returns <see cref="BadRequestResult"/> if the request is invalid.  
        /// Returns <see cref="StatusCodeResult"/> 500 if an unexpected error occurs.
        /// </returns>
        /// <response code="200">Category created successfully.</response>
        /// <response code="400">Invalid category data supplied.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest categoryDTO)
        {
            var category = await _categoriesService.CreateCategoryAsync(categoryDTO);

            return Ok(categoryDTO);
        }

        /// <summary>
        /// Updates an existing category in the database.
        /// </summary>
        /// <param name="Id">The unique identifier of the category to update.</param>
        /// <param name="categoryDTO">The updated category details.</param>
        /// <returns>
        /// Returns <see cref="NoContentResult"/> if the update was successful.  
        /// Returns <see cref="BadRequestResult"/> if the ID does not match the request body.  
        /// Returns <see cref="NotFoundResult"/> if no category is found with the given ID.  
        /// Returns <see cref="StatusCodeResult"/> 500 if an unexpected error occurs.
        /// </returns>
        /// <response code="204">Category updated successfully.</response>
        /// <response code="400">Invalid request or ID mismatch.</response>
        [HttpPut("{Id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid Id, [FromBody] UpdateCategoryDTO categoryDTO)
        {
            if (Id != categoryDTO.Id)
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["IdMismatch"].Value));

            var res = await _categoriesService.UpdateCategoryAsync(categoryDTO);

            return NoContent();
        }

        /// <summary>
        /// Deletes a category with the specified Id from the database.
        /// </summary>
        /// <param name="Id">The unique identifier of the category to delete.</param>
        /// <returns>
        /// Returns <see cref="NoContentResult"/> if the category was deleted successfully.  
        /// Returns <see cref="BadRequestResult"/> if deletion fails or the ID is invalid.  
        /// Returns <see cref="NotFoundResult"/> if no category exists with the given ID.  
        /// Returns <see cref="StatusCodeResult"/> 500 if an unexpected error occurs.
        /// </returns>
        /// <response code="204">Category deleted successfully.</response>
        /// <response code="400">Invalid category ID or failed to delete.</response>
        [HttpDelete("{Id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCategory(Guid Id)
        {
            var result = await _categoriesService.DeleteCategoryAsync(Id);
            if (!result)
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["CategoryDeletionFailed"].Value));

            return NoContent();
        }

        /// <summary>
        /// Assign a brand to a category.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="brandId">The unique identifier of the brand.</param>
        /// <returns>NoContent if successful, BadRequest otherwise.</returns>
        [HttpPost("{categoryId:guid}/brands/{brandId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AssignBrandToCategory(Guid categoryId, Guid brandId)
        {
            var result = await _categoriesService.AssignBrandToCategoryAsync(
                new CategoryBrand { CategoryId = categoryId, BrandId = brandId });

            if (!result)
                return NotFound(ApiResponseFactory.BadRequest(_localizer["InvalidBrandOrCategoryIds"].Value));

            return NoContent();
        }

        /// <summary>
        /// Unassign a brand from a category.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="brandId">The unique identifier of the brand.</param>
        /// <returns>NoContent if successful, BadRequest otherwise.</returns>
        [HttpDelete("{categoryId:guid}/brands/{brandId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UnassignBrandFromCategory(Guid categoryId, Guid brandId)
        {
            var result = await _categoriesService.UnassignBrandFromCategoryAsync(
                new CategoryBrand { CategoryId = categoryId, BrandId = brandId });

            if (!result)
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["BrandUnassignmentFailed"].Value));

            return NoContent();
        }

        /// <summary>
        /// Exports all categories into the specified file format.
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
        /// <response code="200">categories exported successfully in the requested format.</response>
        /// <response code="400">Unsupported export type requested.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Export(ExportType type)
        {
            var categories = await _categoriesService.GetAllCategoriesAsync();

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(categories), "text/csv", "categories.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(categories), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "categories.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(categories), "application/pdf", "categories.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }
        /// <summary>
        /// Deletes a specific category image by its ID.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="photoId">The unique identifier of the photo to delete.</param>
        /// <returns>
        /// <c>200 OK</c> if the image was deleted successfully;  
        /// <c>404 Not Found</c> if the category or the photo was not found;  
        /// <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">Image deleted successfully.</response>
        /// <response code="404">Category or image not found.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpDelete("{categoryId:guid}/images/{photoId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategoryImage(Guid categoryId, Guid photoId)
        {
            var response = await _categoriesService.DeleteCategoryImageAsync(categoryId, photoId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Adds images to the specified category.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="files">The list of image files to upload.</param>
        /// <returns>
        /// <c>200 OK</c> if the images were added successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the category does not exist.
        /// </returns>
        /// <response code="200">Images added successfully.</response>
        /// <response code="400">No files were provided.</response>
        /// <response code="404">Category not found.</response>
        [HttpPost("{categoryId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCategoryImages(Guid categoryId, [FromForm] List<IFormFile> files)
        {
            var response = await _categoriesService.AddCategoryImagesAsync(categoryId, files);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Updates all images of the specified category.
        /// </summary>
        /// <remarks>
        /// This will remove all existing images of the category and replace them with the newly uploaded ones.
        /// </remarks>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="files">The list of new image files to upload.</param>
        /// <returns>
        /// <c>200 OK</c> if the images were updated successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the category does not exist.
        /// </returns>
        /// <response code="200">Images updated successfully.</response>
        /// <response code="400">No files were provided.</response>
        /// <response code="404">Category not found.</response>
        [HttpPatch("{categoryId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategoryImages(Guid categoryId, [FromForm] List<IFormFile> files)
        {
            var response = await _categoriesService.UpdateCategoryImagesAsync(categoryId, files);
            return StatusCode(response.StatusCode, response);
        }
    }
}
