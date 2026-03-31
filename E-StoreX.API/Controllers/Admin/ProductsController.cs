using Asp.Versioning;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Products.Requests;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// Provides administrative operations for managing products in the E-StoreX application.
    /// </summary>
    /// <remarks>
    /// This controller is intended for admin scenarios only and includes endpoints 
    /// for creating, updating, and deleting products.
    /// </remarks>
    [ApiVersion(2.0)]
    public class ProductsController : AdminControllerBase
    {
        private readonly IProductsService _productsService;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class.
        /// </summary>
        /// <param name="productsService">Service for handling product operations.</param>
        /// <param name="exportService">Service to manage files.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public ProductsController(IProductsService productsService, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _productsService = productsService;
            _exportService = exportService;
            _localizer = localizer;
        }
        /// <summary>
        /// Creates a new product in the database.
        /// </summary>
        /// <param name="productRequest">The product details to be created.</param> 
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with the created <see cref="ProductResponse"/>.  
        /// </returns>
        /// <response code="200">Product created successfully.</response>
        /// <response code="400">Invalid product data supplied.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductResponse>> CreateProduct([FromForm] ProductAddRequest productRequest)
        {
            var createdProduct = await _productsService.CreateProductAsync(productRequest);
            return Ok(createdProduct);
        }
        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="productUpdateRequest">The updated product details.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with the updated <see cref="ProductResponse"/>.  
        /// Returns <see cref="BadRequestObjectResult"/> if the IDs do not match.
        /// </returns>
        /// <response code="200">Product updated successfully.</response>
        /// <response code="400">Product ID mismatch or invalid data supplied.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductResponse>> UpdateProduct([FromRoute] Guid id, [FromForm] ProductUpdateRequest productUpdateRequest)
        {
            if (id != productUpdateRequest.Id) return BadRequest(ApiResponseFactory.BadRequest(_localizer["IdMismatch"].Value));

            var updatedProduct = await _productsService.UpdateProductAsync(productUpdateRequest);
            return Ok(updatedProduct);
        }
        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the product to delete.</param>
        /// <returns>
        /// Returns <see cref="OkResult"/> if the product was deleted.  
        /// Returns <see cref="NotFoundResult"/> if no product was found with the given ID.  
        /// Returns <see cref="BadRequestResult"/> if the ID is invalid.
        /// </returns>
        /// <response code="200">Product deleted successfully.</response>
        /// <response code="400">Invalid product ID supplied.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidProductId"].Value));

            var res = await _productsService.DeleteProductAsync(id);

            if (!res)
                return NotFound(ApiResponseFactory.NotFound(_localizer["ProductNotFound"].Value));

            return Ok(ApiResponseFactory.Success(_localizer["DeletedSuccessfully"].Value));
        }
        /// <summary>
        /// Exports all products into the specified file format.
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
        /// <response code="200">Products exported successfully in the requested format.</response>
        /// <response code="400">Unsupported export type requested.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Export(ExportType type)
        {
            var products = await _productsService.GetAllProductsAsync();

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(products), "text/csv", "products.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(products), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(products), "application/pdf", "products.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }
        /// <summary>
        /// Deletes a specific product image by its ID.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="photoId">The unique identifier of the photo to delete.</param>
        /// <returns>
        /// <c>200 OK</c> if the image was deleted successfully;  
        /// <c>404 Not Found</c> if the product or the photo was not found;  
        /// <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">Image deleted successfully.</response>
        /// <response code="404">Product or image not found.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpDelete("{productId:guid}/images/{photoId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductImage(Guid productId, Guid photoId)
        {
            var response = await _productsService.DeleteProductImageAsync(productId, photoId);
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Adds images to the specified product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="files">The list of image files to upload.</param>
        /// <returns>
        /// <c>200 OK</c> if the images were added successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the product does not exist.
        /// </returns>
        /// <response code="200">Images added successfully.</response>
        /// <response code="400">No files were provided.</response>
        /// <response code="404">Product not found.</response>
        [HttpPost("{productId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddProductImages(Guid productId, [FromForm] List<IFormFile> files)
        {
            var response = await _productsService.AddProductImagesAsync(productId, files);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Updates all images of the specified product.
        /// </summary>
        /// <remarks>
        /// This will remove all existing images of the product and replace them with the newly uploaded ones.
        /// </remarks>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="files">The list of new image files to upload.</param>
        /// <returns>
        /// <c>200 OK</c> if the images were updated successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the product does not exist.
        /// </returns>
        /// <response code="200">Images updated successfully.</response>
        /// <response code="400">No files were provided.</response>
        /// <response code="404">Product not found.</response>
        [HttpPatch("{productId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProductImages(Guid productId, [FromForm] List<IFormFile> files)
        {
            var response = await _productsService.UpdateProductImagesAsync(productId, files);
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Updates the featured status of a product.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <param name="isFeatured">Boolean value to set whether the product is featured or not.</param>
        /// <returns>
        /// <c>200 OK</c> if the product status was updated successfully;  
        /// <c>404 Not Found</c> if the product does not exist.
        /// </returns>
        /// <response code="200">Product featured status updated successfully.</response>
        /// <response code="404">Product not found or invalid ID.</response>
        [HttpPatch("{id:guid}/featured")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetFeaturedStatus(Guid id, [FromQuery] bool isFeatured)
        {
            var result = await _productsService.SetFeaturedStatusAsync(id, isFeatured);
            if (!result)
                return NotFound(ApiResponseFactory.NotFound(_localizer["ProductNotFoundOrInvalidId"].Value));
            return Ok(ApiResponseFactory.Success(isFeatured ? _localizer["ProductMarkedAsFeatured"].Value : _localizer["ProductRemovedFromFeatured"].Value));
        }
    }
}
