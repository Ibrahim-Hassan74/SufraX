using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Products.Requests;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Products;
using EStoreX.Core.Services.Common;
using Microsoft.AspNetCore.Http;

namespace EStoreX.Core.Services.Products
{
    public class ProductsService : BaseService, IProductsService
    {
        private readonly IProductRepository _productRepository;
        private readonly IEntityImageManager<Product> _imageManager;
        private readonly IImageService _imageService;
        public ProductsService(IUnitOfWork unitOfWork, IMapper mapper, IEntityImageManager<Product> imageManager, IImageService imageService) : base(unitOfWork, mapper)
        {
            _productRepository = unitOfWork.ProductRepository;
            _imageManager = imageManager;
            _imageService = imageService;
        }
        /// <inheritdoc/>
        public async Task<ProductResponse> CreateProductAsync(ProductAddRequest productRequest)
        {
            if (productRequest == null)
            {
                throw new ArgumentNullException(nameof(productRequest), "Product request cannot be null.");
            }
            ValidationHelper.ModelValidation(productRequest);

            var product = _mapper.Map<Product>(productRequest);
            product.Id = Guid.NewGuid();

            product = await _productRepository.AddProductAsync(product, productRequest.Photos);

            return _mapper.Map<ProductResponse>(product);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id, x => x.Category, y => y.Photos);

            if (product == null)
                return false;

            return await _productRepository.DeleteAsync(product);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync(x => x.Category, y => y.Photos, b => b.Brand);
            var productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products);
            return productResponses;
        }

        /// <inheritdoc/>
        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Product ID cannot be empty.", nameof(id));
            }

            var product = await _productRepository.GetByIdAsync(id, x => x.Category, y => y.Photos, b => b.Brand);

            if (product == null)
                return null;

            var productResponse = _mapper.Map<ProductResponse>(product);
            return productResponse;
        }

        /// <inheritdoc/>
        public async Task<ProductResponse> UpdateProductAsync(ProductUpdateRequest productUpdateRequest)
        {
            if (productUpdateRequest == null)
            {
                throw new ArgumentNullException(paramName: nameof(productUpdateRequest), "Product update request cannot be null.");
            }

            ValidationHelper.ModelValidation(productUpdateRequest);

            var findProduct = await _productRepository.GetByIdAsync(productUpdateRequest.Id, x => x.Category, x => x.Photos, b => b.Brand);

            if (findProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {productUpdateRequest.Id} not found.");
            }

            findProduct.Id = productUpdateRequest.Id;
            findProduct.Name = productUpdateRequest.Name;
            findProduct.Description = productUpdateRequest.Description;
            findProduct.OldPrice = productUpdateRequest.OldPrice;
            findProduct.NewPrice = productUpdateRequest.NewPrice;
            findProduct.CategoryId = productUpdateRequest.CategoryId;
            findProduct.BrandId = productUpdateRequest.BrandId;

            var productResponse = await _productRepository.UpdateProductAsync(findProduct, productUpdateRequest.Photos);

            return _mapper.Map<ProductResponse>(findProduct);
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ProductResponse>, int size)> GetFilteredProductsAsync(ProductQueryDTO query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            var (products, size) = await _productRepository.GetFilteredProductsAsync(query);
            var productsResponse = _mapper.Map<IEnumerable<ProductResponse>>(products);
            return (productsResponse, size);
        }

        /// <inheritdoc/>
        public async Task<int> CountProductsAsync()
        {
            return await _productRepository.CountAsync();
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> GetProductImagesAsync(Guid productId)
        {
            return await _imageManager.GetImagesAsync(
                productId,
                async (uow, id) => await uow.ProductRepository.GetByIdAsync(id, p => p.Photos),
                product => product.Photos
            );
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteProductImageAsync(Guid productId, Guid photoId)
        {
            return await _imageManager.DeleteImageAsync(
                productId,
                photoId,
                async (uow, id) => await uow.ProductRepository.GetByIdAsync(id, p => p.Photos),
                product => product.Photos
            );
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> AddProductImagesAsync(Guid productId, List<IFormFile> files)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId, p => p.Photos);
            if (product == null)
                return ApiResponseFactory.NotFound("Product not found.");

            var folderName = product.Name.Replace(" ", "");

            return await _imageManager.AddImagesAsync(
                productId,
                files,
                folderName,
                async (uow, id) => await uow.ProductRepository.GetByIdAsync(id, p => p.Photos),
                (entity, imagePaths) =>
                {
                    foreach (var path in imagePaths)
                    {
                        entity.Photos.Add(new Photo
                        {
                            ProductId = productId,
                            ImageName = path
                        });
                    }
                }
            );
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> UpdateProductImagesAsync(Guid productId, List<IFormFile> files)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId, p => p.Photos);
            if (product == null)
                return ApiResponseFactory.NotFound("Product not found.");

            if (files == null || files.Count == 0)
                return ApiResponseFactory.BadRequest("No files provided.");

            foreach (var photo in product.Photos.ToList())
            {
                _imageService.DeleteImageAsync(photo.ImageName);
                product.Photos.Remove(photo);
            }

            var folderName = product.Name.Replace(" ", "").ToLowerInvariant();

            var formFileCollection = new FormFileCollection();
            foreach (var file in files)
                formFileCollection.Add(file);

            var imagePaths = await _imageService.AddImageAsync(formFileCollection, $"Products/{folderName}");

            foreach (var path in imagePaths)
            {
                product.Photos.Add(new Photo
                {
                    ImageName = path,
                    ProductId = productId
                });
            }

            await _unitOfWork.CompleteAsync();
            return ApiResponseFactory.Success("Images updated successfully.");
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> GetBestSellersAsync(int count)
        {
            if (count <= 0)
                return ApiResponseFactory.BadRequest("Count must be greater than zero.");
            var filter = new ProductQueryDTO
            {
                PageNumber = 1,
                PageSize = count,
                SortBy = ProductSortBy.SalesCount, // nameof(Product.SalesCount),
                SortOrder = SortOrderOptions.DESC
            };

            var (bestSellers, totalCount) = await _unitOfWork.ProductRepository.GetFilteredProductsAsync(filter);

            if (!bestSellers.Any())
                return ApiResponseFactory.NotFound("No products found.");

            var response = _mapper.Map<List<ProductResponse>>(bestSellers);
            return ApiResponseFactory.Success("Best sellers retrieved successfully.", response);
        }
        public async Task<IEnumerable<ProductResponse>> GetFeaturedProductsAsync()
        {
            var products = await _productRepository.GetFeaturedProductsAsync();
            return _mapper.Map<IEnumerable<ProductResponse>>(products);
        }

        public async Task<bool> SetFeaturedStatusAsync(Guid productId, bool isFeatured)
        {
            return await _productRepository.SetFeaturedStatusAsync(productId, isFeatured);
        }
    }
}
