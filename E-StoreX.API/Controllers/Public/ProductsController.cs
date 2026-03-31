using E_StoreX.API.Helper;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Handles product-related operations such as retrieving all products or fetching product details by ID.
    /// </summary>
    public class ProductsController : CustomControllerBase
    {
        private readonly IProductsService _productsService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class.
        /// </summary>
        /// <param name="productsService">Service for managing product operations.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public ProductsController(IProductsService productsService, IStringLocalizer<SharedResource> localizer)
        {
            _productsService = productsService;
            _localizer = localizer;
        }

        /// <summary>
        /// Retrieves all products with optional filtering and pagination.
        /// </summary>
        /// <param name="query">Query parameters for filtering, searching, and pagination.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with a paginated list of <see cref="ProductResponse"/> objects if products exist.  
        /// Returns <see cref="NoContentResult"/> (204) if no products match the query.
        /// </returns>
        /// <response code="200">Successfully retrieved the list of products.</response>
        /// <response code="204">No products found for the given query.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [ProducesResponseType(typeof(Pagination<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts([FromQuery] ProductQueryDTO query)
        {
            var (products, totalCount) = await _productsService.GetFilteredProductsAsync(query);

            //var totalCount = await _productsService.CountProductsAsync();

            if (!products.Any())
                return NoContent();
            var result = new Pagination<ProductResponse>(query.PageNumber, query.PageSize, totalCount, products);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="Id">The unique identifier of the product.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with the <see cref="ProductResponse"/> if found.  
        /// Returns <see cref="NotFoundObjectResult"/> if the product does not exist.
        /// </returns>
        /// <response code="200">Successfully retrieved the product.</response>
        /// <response code="404">Product not found or invalid ID.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{Id:guid}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductResponse>> GetProductById(Guid Id)
        {
            var product = await _productsService.GetProductByIdAsync(Id);
            return product is not null ? Ok(product) : NotFound(ApiResponseFactory.NotFound(_localizer["ProductNotFoundOrInvalidId"].Value));
        }
        /// <summary>
        /// Retrieves all images associated with a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of product images;  
        /// <c>404 Not Found</c> if the product does not exist or has no images.
        /// </returns>
        /// <response code="200">Product images retrieved successfully.</response>
        /// <response code="404">No images found for the given product.</response>
        [HttpGet("{productId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponseWithData<List<PhotoInfo>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductImages(Guid productId)
        {
            var response = await _productsService.GetProductImagesAsync(productId);
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Retrieves the top N best-selling products.
        /// </summary>
        /// <param name="count">The number of best-selling products to return (e.g., 5, 10, 20).</param>
        /// <returns>
        /// <c>200 OK</c> with the best-selling products;  
        /// <c>400 Bad Request</c> if the count is invalid;  
        /// <c>404 Not Found</c> if no products exist.
        /// </returns>
        /// <response code="200">Best sellers retrieved successfully.</response>
        /// <response code="400">Invalid count provided.</response>
        /// <response code="404">No products found.</response>
        [HttpGet("best-sellers")]
        [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<ProductResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBestSellers([FromQuery] int count = 10)
        {
            var response = await _productsService.GetBestSellersAsync(count);
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Retrieves all featured products.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with the featured products;  
        /// <c>404 Not Found</c> if no featured products exist.
        /// </returns>
        /// <response code="200">Featured products retrieved successfully.</response>
        /// <response code="404">No featured products found.</response>
        [HttpGet("featured")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            var response = await _productsService.GetFeaturedProductsAsync();
            if (!response.Any())
                return NotFound(ApiResponseFactory.NotFound(_localizer["NoFeaturedProducts"].Value));
            return Ok(response);
        }
    }
}
