using Asp.Versioning;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Brands.Response;
using EStoreX.Core.DTO.Categories.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Products;
using EStoreX.Core.Services.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Public controller for retrieving brand information in API version 1.0.
    /// </summary>
    [ApiVersion(1.0)]
    public class BrandsController : CustomControllerBase
    {
        private readonly IBrandService _brandsService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        /// <summary>
        /// Initializes a new instance of <see cref="BrandsController"/>.
        /// </summary>
        /// <param name="brandsService">Service to manage brand operations.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public BrandsController(IBrandService brandsService, IStringLocalizer<SharedResource> localizer)
        {
            _brandsService = brandsService;
            _localizer = localizer;
        }

        /// <summary>
        /// Retrieves all brands.
        /// </summary>
        /// <returns>List of all brands.</returns>
        /// <response code="200">Returns the list of brands.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BrandResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _brandsService.GetAllBrandsAsync();
            return Ok(brands);
        }

        /// <summary>
        /// Retrieves a brand by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the brand to retrieve.</param>
        /// <returns>The brand details if found.</returns>
        /// <response code="200">Brand found and returned successfully.</response>
        /// <response code="404">Brand not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var brand = await _brandsService.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["BrandNotFound"].Value));

            return Ok(brand);
        }

        /// <summary>
        /// Retrieves a brand by its name.
        /// </summary>
        /// <param name="name">The exact name of the brand to search for.</param>
        /// <returns>
        /// Returns <see cref="Brand"/> with HTTP 200 OK if the brand is found.  
        /// Returns <see cref="ApiResponse"/> with HTTP 404 Not Found if the brand does not exist.
        /// </returns>
        /// <response code="200">Brand found and returned successfully.</response>
        /// <response code="404">Brand with the specified name was not found.</response>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBrandByName(string name)
        {
            var brand = await _brandsService.GetBrandByNameAsync(name);
            if (brand == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["BrandNotFoundWithName", name].Value));
            return Ok(brand);
        }
        /// <summary>
        /// Get all categories that a brand belongs to.
        /// </summary>
        /// <param name="brandId">The unique identifier of the brand.</param>
        /// <returns>List of categories associated with the brand.</returns>
        [HttpGet("{brandId:guid}/categories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategoriesByBrand(Guid brandId)
        {
            var categories = await _brandsService.GetCategoriesByBrandIdAsync(brandId);
            return Ok(categories);
        }
        /// <summary>
        /// Retrieves all images associated with a specific brand.
        /// </summary>
        /// <param name="brandId">The unique identifier of the brand.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of brand images;  
        /// <c>404 Not Found</c> if the brand does not exist or has no images.
        /// </returns>
        /// <response code="200">Brand images retrieved successfully.</response>
        /// <response code="404">No images found for the given brand.</response>
        [HttpGet("{brandId:guid}/images")]
        [ProducesResponseType(typeof(ApiResponseWithData<List<PhotoInfo>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBrandImages(Guid brandId)
        {
            var response = await _brandsService.GetBrandImagesAsync(brandId);
            return StatusCode(response.StatusCode, response);
        }

    }
}
