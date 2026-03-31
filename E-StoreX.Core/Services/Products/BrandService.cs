using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Brands.Response;
using EStoreX.Core.DTO.Categories.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Products;
using EStoreX.Core.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Products
{
    /// <summary>
    /// Service implementation for managing <see cref="Brand"/> entities.
    /// </summary>
    public class BrandService : BaseService, IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IEntityImageManager<Brand> _imageManager;
        private readonly IImageService _imageService;
        private readonly IStringLocalizer<BrandService> _localizer;

        public BrandService(IUnitOfWork unitOfWork, IMapper mapper, IEntityImageManager<Brand> imageManager, IImageService imageService, IStringLocalizer<BrandService> localizer) : base(unitOfWork, mapper)
        {
            _brandRepository = _unitOfWork.BrandRepository;
            _imageManager = imageManager;
            _imageService = imageService;
            _localizer = localizer;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BrandResponse>> GetAllBrandsAsync()
        {

            var res = await _brandRepository.GetAllAsync(x => x.Photos);
            return _mapper.Map<IEnumerable<BrandResponse>>(res);
        }

        /// <inheritdoc/>
        public async Task<BrandResponse?> GetBrandByIdAsync(Guid brandId)
        {
            if (brandId == Guid.Empty)
            {
                throw new ArgumentException(_localizer["BrandIdRequired"].Value, nameof(brandId));
            }
            var res = await _brandRepository.GetByIdAsync(brandId, x => x.Photos);
            return _mapper.Map<BrandResponse?>(res);
        }

        /// <inheritdoc/>
        public async Task<Brand> CreateBrandAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(_localizer["BrandNameRequired"].Value, nameof(name));
            }
            if (await _brandRepository.GetByNameAsync(name) != null)
            {
                throw new InvalidOperationException(string.Format(_localizer["BrandAlreadyExists"].Value, name));
            }

            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                NameEn = name
            };

            await _brandRepository.AddAsync(brand);
            await _unitOfWork.CompleteAsync();

            return brand;
        }

        /// <inheritdoc/>
        public async Task<Brand> UpdateBrandAsync(Guid brandId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException(_localizer["NewBrandNameRequired"].Value, nameof(newName));
            }

            var existingBrand = await _brandRepository.GetByNameAsync(newName);
            if (existingBrand != null && existingBrand.Id != brandId)
            {
                throw new InvalidOperationException(string.Format(_localizer["BrandAlreadyExists"].Value, newName));
            }

            var brand = await _brandRepository.GetByIdAsync(brandId);
            if (brand == null)
            {
                throw new KeyNotFoundException(string.Format(_localizer["BrandNotFoundWithId"].Value, brandId));
            }

            brand.NameEn = newName;
            await _brandRepository.UpdateAsync(brand);
            await _unitOfWork.CompleteAsync();

            return brand;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteBrandAsync(Guid brandId)
        {
            if (brandId == Guid.Empty)
                throw new ArgumentException(_localizer["BrandIdRequired"].Value, nameof(brandId));

            var brand = await _brandRepository.GetByIdAsync(brandId);
            if (brand == null)
                return false;

            await _brandRepository.DeleteAsync(brandId);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<BrandResponse?> GetBrandByNameAsync(string name)
        {
            var res = await _brandRepository.GetByNameAsync(name);
            return _mapper.Map<BrandResponse?>(res);
        }

        public async Task<IEnumerable<CategoryResponse>> GetCategoriesByBrandIdAsync(Guid brandId)
        {
            var categories = await _brandRepository.GetCategoriesByBrandIdAsync(brandId);
            return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> GetBrandImagesAsync(Guid brandId)
        {
            return await _imageManager.GetImagesAsync(
                brandId,
                async (uow, id) => await uow.BrandRepository.GetByIdAsync(id, b => b.Photos),
                brand => brand.Photos
            );
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteBrandImageAsync(Guid brandId, Guid photoId)
        {
            return await _imageManager.DeleteImageAsync(
                brandId,
                photoId,
                async (uow, id) => await uow.BrandRepository.GetByIdAsync(id, b => b.Photos),
                brand => brand.Photos
            );
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> AddBrandImagesAsync(Guid brandId, List<IFormFile> files)
        {
            var brand = await _unitOfWork.BrandRepository.GetByIdAsync(brandId, b => b.Photos);
            if (brand == null)
                return ApiResponseFactory.NotFound(_localizer["BrandNotFound"].Value);

            var folderName = brand.NameEn.Replace(" ", "").ToLowerInvariant();

            return await _imageManager.AddImagesAsync(
                brandId,
                files,
                $"Brands/{folderName}",
                async (uow, id) => await uow.BrandRepository.GetByIdAsync(id, b => b.Photos),
                (entity, imagePaths) =>
                {
                    foreach (var path in imagePaths)
                    {
                        entity.Photos.Add(new Photo
                        {
                            BrandId = brandId,
                            ImageName = path
                        });
                    }
                }
            );
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> UpdateBrandImagesAsync(Guid brandId, List<IFormFile> files)
        {
            var brand = await _unitOfWork.BrandRepository.GetByIdAsync(brandId, b => b.Photos);
            if (brand == null)
                return ApiResponseFactory.NotFound(_localizer["BrandNotFound"].Value);

            if (files == null || files.Count == 0)
                return ApiResponseFactory.BadRequest(_localizer["NoFilesProvided"].Value);

            // Delete old images
            foreach (var photo in brand.Photos.ToList())
            {
                _imageService.DeleteImageAsync(photo.ImageName);
                brand.Photos.Remove(photo);
            }

            var folderName = brand.NameEn.Replace(" ", "").ToLowerInvariant();

            var formFileCollection = new FormFileCollection();
            foreach (var file in files)
                formFileCollection.Add(file);

            var imagePaths = await _imageService.AddImageAsync(formFileCollection, $"Brands/{folderName}");

            foreach (var path in imagePaths)
            {
                brand.Photos.Add(new Photo
                {
                    ImageName = path,
                    BrandId = brandId
                });
            }

            await _unitOfWork.CompleteAsync();
            return ApiResponseFactory.Success(_localizer["ImagesUpdatedSuccessfully"].Value);
        }

    }
}
