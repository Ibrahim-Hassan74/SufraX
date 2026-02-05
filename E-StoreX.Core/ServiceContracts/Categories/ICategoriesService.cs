using Domain.Entities.Product;
using EStoreX.Core.DTO.Categories.Requests;
using EStoreX.Core.DTO.Categories.Responses;
using EStoreX.Core.DTO.Common;
using Microsoft.AspNetCore.Http;

namespace EStoreX.Core.ServiceContracts.Categories
{
    /// <summary>
    /// Service contract for managing <see cref="Category"/> entities.
    /// Provides operations for creating, updating, deleting, and retrieving categories,
    /// as well as managing their associated brands.
    /// </summary>
    public interface ICategoriesService
    {
        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing a collection of <see cref="CategoryResponse"/>.
        /// </returns>
        Task<IEnumerable<CategoryResponseWithPhotos>> GetAllCategoriesAsync();

        /// <summary>
        /// Retrieves a category by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing the <see cref="CategoryResponse"/> if found; otherwise, <c>null</c>.
        /// </returns>
        Task<CategoryResponseWithPhotos?> GetCategoryByIdAsync(Guid id);

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryRequest">The category data transfer object containing input details.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing the newly created <see cref="CategoryResponse"/>.
        /// </returns>
        Task<CategoryResponse> CreateCategoryAsync(CategoryRequest categoryRequest);

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="updateCategoryDto">The DTO containing updated category information.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing the updated <see cref="CategoryResponse"/>.
        /// </returns>
        Task<CategoryResponse> UpdateCategoryAsync(UpdateCategoryDTO updateCategoryDto);

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>
        /// <c>true</c> if the category was successfully deleted; 
        /// otherwise, <c>false</c> if it does not exist.
        /// </returns>
        Task<bool> DeleteCategoryAsync(Guid id);

        /// <summary>
        /// Retrieves all brands assigned to a specific category.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing a collection of <see cref="Brand"/> entities linked to the category.
        /// </returns>
        Task<IEnumerable<Brand>> GetBrandsByCategoryIdAsync(Guid categoryId);

        /// <summary>
        /// Assigns a brand to a category.
        /// </summary>
        /// <param name="cb">The linking entity representing the relationship between category and brand.</param>
        /// <returns>
        /// <c>true</c> if the assignment was successful; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> AssignBrandToCategoryAsync(CategoryBrand cb);

        /// <summary>
        /// Removes an existing brand assignment from a category.
        /// </summary>
        /// <param name="cb">The linking entity representing the relationship between category and brand.</param>
        /// <returns>
        /// <c>true</c> if the unassignment was successful; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> UnassignBrandFromCategoryAsync(CategoryBrand cb);
        /// <summary>
        /// Retrieves all images associated with a specific category.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> containing the list of category images if found;  
        /// otherwise, a <c>404 Not Found</c> response.
        /// </returns>
        Task<ApiResponse> GetCategoryImagesAsync(Guid categoryId);

        /// <summary>
        /// Deletes a specific category image by its ID.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="photoId">The unique identifier of the photo to delete.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating whether the image was deleted successfully;  
        /// <c>404 Not Found</c> if the category or image was not found.
        /// </returns>
        Task<ApiResponse> DeleteCategoryImageAsync(Guid categoryId, Guid photoId);

        /// <summary>
        /// Adds images to the specified category.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="files">The list of image files to upload.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating whether the images were added successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the category does not exist.
        /// </returns>
        Task<ApiResponse> AddCategoryImagesAsync(Guid categoryId, List<IFormFile> files);

        /// <summary>
        /// Updates all images of the specified category.
        /// </summary>
        /// <remarks>
        /// This will remove all existing images of the category and replace them with the newly uploaded ones.
        /// </remarks>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <param name="files">The list of new image files to upload.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating whether the images were updated successfully;  
        /// <c>400 Bad Request</c> if no files were provided;  
        /// <c>404 Not Found</c> if the category does not exist.
        /// </returns>
        Task<ApiResponse> UpdateCategoryImagesAsync(Guid categoryId, List<IFormFile> files);
        /// <summary>
        /// Retrieves all categories along with their associated brands.
        /// </summary>
        /// <remarks>
        /// Each category will appear once in the result, and its <see cref="CategoryBrandResponse.BrandResponse"/> 
        /// will contain a list of brands linked to that category. 
        /// Photos for both categories and brands are included.
        /// </remarks>
        /// <returns>
        /// A collection of <see cref="CategoryBrandResponse"/> objects, 
        /// each representing a category and its related brands.
        /// </returns>
        Task<IEnumerable<CategoryBrandResponse>> GetCategoriesBrandsAsync();
    }
}
