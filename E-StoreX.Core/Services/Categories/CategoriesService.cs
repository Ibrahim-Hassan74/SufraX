using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Brands.Response;
using EStoreX.Core.DTO.Categories.Requests;
using EStoreX.Core.DTO.Categories.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Categories;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Core.ServiceContracts.Categories;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Categories
{
    public class CategoriesService : BaseService, ICategoriesService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IEntityImageManager<Category> _imageManager;
        private readonly IImageService _imageService;
        private readonly IStringLocalizer<CategoriesService> _localizer;
        public CategoriesService(IMapper mapper, IUnitOfWork unitOfWork, IEntityImageManager<Category> imageManager, IImageService imageService, IStringLocalizer<CategoriesService> localizer) : base(unitOfWork, mapper)
        {
            _categoryRepository = _unitOfWork.CategoryRepository;
            _imageManager = imageManager;
            _imageService = imageService;
            _localizer = localizer;
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest categoryRequest)
        {
            if (categoryRequest == null)
                throw new ArgumentNullException(nameof(categoryRequest), _localizer["CategoryRequired"].Value);

            ValidationHelper.ModelValidation(categoryRequest);

            var category = _mapper.Map<Category>(categoryRequest);

            category.Id = Guid.NewGuid(); // Ensure a new ID is generated for the category

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(_localizer["CategoryIdRequired"].Value, nameof(id));

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            var res = await _categoryRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return res;
        }

        public async Task<IEnumerable<CategoryResponseWithPhotos>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync(x => x.Photos);
            var res = _mapper.Map<IEnumerable<CategoryResponseWithPhotos>>(categories);
            return res;
        }

        public async Task<IEnumerable<CategoryBrandResponse>> GetCategoriesBrandsAsync()
        {
            var categories = await _categoryRepository.GetAllAsync(x => x.Photos,x => x.CategoryBrands);
            var brands = await _unitOfWork.BrandRepository.GetAllAsync(x => x.Photos,x => x.CategoryBrands);
            var brandResponse = _mapper.Map<List<BrandResponse>>(brands);
            var result = categories.Select(category =>
            {
                var categoryResponse = _mapper.Map<CategoryResponseWithPhotos>(category);
                var relatedBrands = brandResponse.Where(b => category.CategoryBrands.Any(cb => cb.BrandId == b.Id)).ToList();
                return new CategoryBrandResponse
                {
                    Categories = categoryResponse,
                    BrandResponse = relatedBrands
                };
            }).ToList();
            return result;
        }

        public async Task<CategoryResponseWithPhotos?> GetCategoryByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(_localizer["CategoryIdRequired"].Value, nameof(id));
            var category = await _categoryRepository.GetByIdAsync(id, x  => x.Photos);
            if (category == null)
                return null;

            return _mapper.Map<CategoryResponseWithPhotos>(category);
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(UpdateCategoryDTO updateCategoryDto)
        {
            if(updateCategoryDto is null)
                throw new ArgumentNullException(nameof(updateCategoryDto), _localizer["CategoryRequired"].Value);

            ValidationHelper.ModelValidation(updateCategoryDto);

            var category = await _categoryRepository.GetByIdAsync(updateCategoryDto.Id);
            if (category == null)
                throw new KeyNotFoundException(string.Format(_localizer["CategoryNotFoundWithId"].Value, updateCategoryDto.Id));

            _mapper.Map(updateCategoryDto, category);

            var res = await _categoryRepository.UpdateAsync(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CategoryResponse>(category);
        }
        public async Task<IEnumerable<Brand>> GetBrandsByCategoryIdAsync(Guid categoryId)
        {
            var brands = await _categoryRepository.GetBrandsByCategoryIdAsync(categoryId);
            return brands;
        }

        public async Task<bool> AssignBrandToCategoryAsync(CategoryBrand cb)
        {
            if (_unitOfWork.CategoryRepository.GetByIdAsync(cb.CategoryId) is null || 
                _unitOfWork.BrandRepository.GetByIdAsync(cb.BrandId) is null)
                return false;

            var res = await _categoryRepository.AssignBrandAsync(cb);
            await _unitOfWork.CompleteAsync();
            return res;
        }

        public async Task<bool> UnassignBrandFromCategoryAsync(CategoryBrand cb)
        {
            if (_unitOfWork.CategoryRepository.GetByIdAsync(cb.CategoryId) is null ||
                _unitOfWork.BrandRepository.GetByIdAsync(cb.BrandId) is null)
                return false;
            var res = await _categoryRepository.UnassignBrandAsync(cb);
            await _unitOfWork.CompleteAsync();
            return res;
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> GetCategoryImagesAsync(Guid categoryId)
        {
            return await _imageManager.GetImagesAsync(
                categoryId,
                async (uow, id) => await uow.CategoryRepository.GetByIdAsync(id, c => c.Photos),
                category => category.Photos
            );
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteCategoryImageAsync(Guid categoryId, Guid photoId)
        {
            return await _imageManager.DeleteImageAsync(
                categoryId,
                photoId,
                async (uow, id) => await uow.CategoryRepository.GetByIdAsync(id, c => c.Photos),
                category => category.Photos
            );
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> AddCategoryImagesAsync(Guid categoryId, List<IFormFile> files)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId, c => c.Photos);
            if (category == null)
                return ApiResponseFactory.NotFound(_localizer["CategoryNotFound"].Value);

            var folderName = category.NameEn.Replace(" ", "").ToLowerInvariant();

            return await _imageManager.AddImagesAsync(
                categoryId,
                files,
                $"Categories/folderName",
                async (uow, id) => await uow.CategoryRepository.GetByIdAsync(id, c => c.Photos),
                (entity, imagePaths) =>
                {
                    foreach (var path in imagePaths)
                    {
                        entity.Photos.Add(new Photo
                        {
                            CategoryId = categoryId,
                            ImageName = path
                        });
                    }
                }
            );
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> UpdateCategoryImagesAsync(Guid categoryId, List<IFormFile> files)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId, c => c.Photos);
            if (category == null)
                return ApiResponseFactory.NotFound(_localizer["CategoryNotFound"].Value);

            if (files == null || files.Count == 0)
                return ApiResponseFactory.BadRequest(_localizer["NoFilesProvided"].Value);

            // Delete old images
            foreach (var photo in category.Photos.ToList())
            {
                _imageService.DeleteImageAsync(photo.ImageName);
                category.Photos.Remove(photo);
            }

            var folderName = category.NameEn.Replace(" ", "").ToLowerInvariant();

            var formFileCollection = new FormFileCollection();
            foreach (var file in files)
                formFileCollection.Add(file);

            var imagePaths = await _imageService.AddImageAsync(formFileCollection, $"Categories/{folderName}");

            foreach (var path in imagePaths)
            {
                category.Photos.Add(new Photo
                {
                    ImageName = path,
                    CategoryId = categoryId
                });
            }

            await _unitOfWork.CompleteAsync();
            return ApiResponseFactory.Success(_localizer["ImagesUpdatedSuccessfully"].Value);
        }

    }
}
