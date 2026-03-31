using AutoFixture;
using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Products.Requests;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Products;
using EStoreX.Core.Services.Products;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace E_StoreX.ServiceTests
{
    public class ProductsServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IFixture _fixture;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly IProductsService _productsService;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEntityImageManager<Product>> _imageManagerMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<IStringLocalizer<ProductsService>> _localizerMock;

        public ProductsServiceTests()
        {
            _fixture = new Fixture();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _imageManagerMock = new Mock<IEntityImageManager<Product>>();
            _imageServiceMock = new Mock<IImageService>();
            _mapperMock = new Mock<IMapper>();
            _localizerMock = new Mock<IStringLocalizer<ProductsService>>();
            _unitOfWorkMock.Setup(u => u.ProductRepository)
                           .Returns(_productRepositoryMock.Object);
            _productsService = new ProductsService(_unitOfWorkMock.Object, _mapperMock.Object,
                _imageManagerMock.Object, _imageServiceMock.Object, _localizerMock.Object);
        }
        #region Helper Methods
        private ProductAddRequest CreateValidProductAddRequest()
        {
            var photoMock = new Mock<IFormFile>();
            var photosMock = new Mock<IFormFileCollection>();
            photosMock.Setup(p => p.Count).Returns(1);
            photosMock.Setup(p => p[0]).Returns(photoMock.Object);

            return new ProductAddRequest
            {
                NameEn = "Test Product",
                DescriptionEn = "This is a test product",
                NewPrice = 100m,
                OldPrice = 150m,
                QuantityAvailable = 10,
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Photos = photosMock.Object
            };
        }
        private Product CreateProductFromValidProductRequest(ProductAddRequest r)
        {
            return new Product
            {
                NameEn = r.NameEn,
                DescriptionEn = r.DescriptionEn,
                NewPrice = r.NewPrice,
                OldPrice = r.OldPrice,
                QuantityAvailable = r.QuantityAvailable,
                BrandId = r.BrandId,
                CategoryId = r.CategoryId
            };
        }
        private ProductResponse CreateProductResponseFromProduct(Product p)
        {
            return new ProductResponse
            {
                Id = p.Id,
                Name = p.NameEn,
                Description = p.DescriptionEn,
                NewPrice = p.NewPrice,
                OldPrice = p.OldPrice,
                QuantityAvailable = p.QuantityAvailable,
                BrandName = "Test Brand",
                CategoryName = "Test Category"
            };
        }
        #endregion

        #region CreateProductAsync Tests
        [Fact]
        public async Task CreateProductAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Act
            Func<Task> act = async () => await _productsService.CreateProductAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("*Product request cannot be null*");
        }

        [Fact]
        public async Task CreateProductAsync_ShouldReturnProductResponse_WhenValidRequest()
        {
            // Arrange

            var photoMock = new Mock<IFormFile>();
            var photosMock = new Mock<IFormFileCollection>();
            photosMock.Setup(p => p.Count).Returns(1);
            photosMock.Setup(p => p[0]).Returns(photoMock.Object);

            // Request
            var request = CreateValidProductAddRequest();

            _mapperMock.Setup(m => m.Map<Product>(request)).Returns(CreateProductFromValidProductRequest);

            _productRepositoryMock
                .Setup(r => r.AddProductAsync(It.IsAny<Product>(), request.Photos))
                .ReturnsAsync((Product p, IFormFileCollection photos) => p);

            // Mapper -> ProductResponse
            _mapperMock.Setup(m => m.Map<ProductResponse>(It.IsAny<Product>()))
                .Returns(CreateProductResponseFromProduct);

            // Act
            var result = await _productsService.CreateProductAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be(request.NameEn);
            result.Description.Should().Be(request.DescriptionEn);
            result.NewPrice.Should().Be(request.NewPrice);
            result.OldPrice.Should().Be(request.OldPrice);
            result.QuantityAvailable.Should().Be(request.QuantityAvailable);
            result.BrandName.Should().Be("Test Brand");
            result.CategoryName.Should().Be("Test Category");

            _productRepositoryMock.Verify(r => r.AddProductAsync(It.IsAny<Product>(), request.Photos), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
        {
            // Arrange
            var invalidRequest = new ProductAddRequest();

            // Act
            Func<Task> act = async () => await _productsService.CreateProductAsync(invalidRequest);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        #endregion

        #region DeleteProductAsync Tests
        [Fact]
        public async Task DeleteProductAsync_ShouldReturnTrue_WhenProductExistsAndDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(product);

            _productRepositoryMock
                .Setup(r => r.DeleteAsync(product))
                .ReturnsAsync(true);

            // Act
            var result = await _productsService.DeleteProductAsync(productId);

            // Assert
            result.Should().BeTrue();
            _productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _productRepositoryMock.Verify(r => r.DeleteAsync(product), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productsService.DeleteProductAsync(productId);

            // Assert
            result.Should().BeFalse();
            _productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _productRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnFalse_WhenDeleteFails()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(product);

            _productRepositoryMock
                .Setup(r => r.DeleteAsync(product))
                .ReturnsAsync(false);

            // Act
            var result = await _productsService.DeleteProductAsync(productId);

            // Assert
            result.Should().BeFalse();
            _productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _productRepositoryMock.Verify(r => r.DeleteAsync(product), Times.Once);
        }

        #endregion

        #region GetAllProductsAsync Tests
        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnMappedProductResponses_WhenProductsExist()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), NameEn = "Product 1" },
                new Product { Id = Guid.NewGuid(), NameEn = "Product 2" }
            };

            var productResponses = new List<ProductResponse>
            {
                new ProductResponse { Id = products[0].Id, Name = "Product 1" },
                new ProductResponse { Id = products[1].Id, Name = "Product 2" }
            };

            _productRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(products);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ProductResponse>>(products))
                .Returns(productResponses);

            // Act
            var result = await _productsService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Id.Should().Be(products[0].Id);
            result.Last().Name.Should().Be("Product 2");

            _productRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductResponse>>(products), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnEmpty_WhenNoProductsExist()
        {
            // Arrange
            var products = new List<Product>();

            _productRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(products);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ProductResponse>>(products))
                .Returns(new List<ProductResponse>());

            // Act
            var result = await _productsService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _productRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductResponse>>(products), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldThrowException_WhenMapperFails()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), NameEn = "Product 1" }
            };

            _productRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(products);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ProductResponse>>(products))
                .Throws(new Exception("Mapping failed"));

            // Act
            Func<Task> act = async () => { await _productsService.GetAllProductsAsync(); };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Mapping failed");

            _productRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductResponse>>(products), Times.Once);
        }
        #endregion

        #region GetProductByIdAsync Tests
        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProductResponse_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, NameEn = "Test Product" };
            var productResponse = new ProductResponse { Id = productId, Name = "Test Product" };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(product);

            _mapperMock
                .Setup(m => m.Map<ProductResponse>(product))
                .Returns(productResponse);

            // Act
            var result = await _productsService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be("Test Product");

            _productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductResponse>(product), Times.Once);
        }
        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productsService.GetProductByIdAsync(productId);

            // Assert
            result.Should().BeNull();
            _productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductResponse>(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var productId = Guid.Empty;

            // Act
            Func<Task> act = async () => { await _productsService.GetProductByIdAsync(productId); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Product ID cannot be empty.*")
                .Where(e => e.ParamName == "id");

            _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<Product, object>>[]>()), Times.Never);
            _mapperMock.Verify(m => m.Map<ProductResponse>(It.IsAny<Product>()), Times.Never);
        }
        #endregion

        #region UpdateProductAsync Tests
        [Fact]
        public async Task UpdateProductAsync_ShouldReturnProductResponse_WhenUpdateIsSuccessful()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var photoMock = new Mock<IFormFile>();
            var photosMock = new Mock<IFormFileCollection>();
            photosMock.Setup(p => p.Count).Returns(1);
            photosMock.Setup(p => p[0]).Returns(photoMock.Object);

            var request = new ProductUpdateRequest
            {
                Id = productId,
                NameEn = "Updated Product",
                DescriptionEn = "Updated DescriptionEn",
                NewPrice = 120m,
                OldPrice = 150m,
                QuantityAvailable = 10,
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Photos = photosMock.Object
            };

            var existingProduct = new Product
            {
                Id = productId,
                NameEn = "Old Product",
                DescriptionEn = "Old DescriptionEn",
                NewPrice = 100m,
                OldPrice = 150m,
                QuantityAvailable = 10,
                BrandId = request.BrandId,
                CategoryId = request.CategoryId
            };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(existingProduct);

            _productRepositoryMock
                .Setup(r => r.UpdateProductAsync(existingProduct, request.Photos))
                .ReturnsAsync(existingProduct);

            _mapperMock
                .Setup(m => m.Map<ProductResponse>(existingProduct))
                .Returns(CreateProductResponseFromProduct);

            // Act
            var result = await _productsService.UpdateProductAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be("Updated Product");
            result.Description.Should().Be("Updated DescriptionEn");

            _productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _productRepositoryMock.Verify(r => r.UpdateProductAsync(existingProduct, request.Photos), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductResponse>(existingProduct), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Act
            Func<Task> act = async () => { await _productsService.UpdateProductAsync(null!); };

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .Where(e => e.ParamName == "productUpdateRequest");

            _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<Product, object>>[]>()), Times.Never);
            _productRepositoryMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(), It.IsAny<IFormFileCollection>()), Times.Never);
        }
        [Fact]
        public async Task UpdateProductAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var photoMock = new Mock<IFormFile>();
            var photosMock = new Mock<IFormFileCollection>();
            photosMock.Setup(p => p.Count).Returns(1);
            photosMock.Setup(p => p[0]).Returns(photoMock.Object);

            var request = new ProductUpdateRequest
            {
                Id = productId,
                NameEn = "Updated Product",
                DescriptionEn = "Updated DescriptionEn",
                NewPrice = 120m,
                OldPrice = 150m,
                QuantityAvailable = 10,
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Photos = photosMock.Object
            };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync((Product?)null);

            // Act
            Func<Task> act = async () => { await _productsService.UpdateProductAsync(request); };

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Product with ID {productId} not found.");

            _productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);
            _productRepositoryMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(), It.IsAny<IFormFileCollection>()), Times.Never);
        }
        #endregion

        #region GetFilteredProductsAsync Tests

        [Fact]
        public async Task GetFilteredProductsAsync_ShouldReturnProductResponses_WhenQueryIsValid()
        {
            // Arrange
            var query = new ProductQueryDTO { SearchString = "Test" };
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), NameEn = "Test Product 1" },
                new Product { Id = Guid.NewGuid(), NameEn = "Test Product 2" }
            };

            var productResponses = new List<ProductResponse>
            {
                new ProductResponse { Id = products[0].Id, Name = "Test Product 1" },
                new ProductResponse { Id = products[1].Id, Name = "Test Product 2" }
            };

            _productRepositoryMock
                .Setup(r => r.GetFilteredProductsAsync(query))
                .ReturnsAsync((products, 1));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ProductResponse>>(products))
                .Returns(productResponses);

            // Act
            var (result, size) = await _productsService.GetFilteredProductsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Name.Should().Be("Test Product 1");
            result.Last().Name.Should().Be("Test Product 2");

            _productRepositoryMock.Verify(r => r.GetFilteredProductsAsync(query), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductResponse>>(products), Times.Once);
        }

        [Fact]
        public async Task GetFilteredProductsAsync_ShouldThrowArgumentNullException_WhenQueryIsNull()
        {
            // Act
            Func<Task> act = async () => { await _productsService.GetFilteredProductsAsync(null!); };

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .Where(e => e.ParamName == "query");

            _productRepositoryMock.Verify(r => r.GetFilteredProductsAsync(It.IsAny<ProductQueryDTO>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductResponse>>(It.IsAny<IEnumerable<Product>>()), Times.Never);
        }

        [Fact]
        public async Task GetFilteredProductsAsync_ShouldReturnEmpty_WhenNoProductsMatchQuery()
        {
            // Arrange
            var query = new ProductQueryDTO { SearchString = "NonExisting" };
            var products = new List<Product>();

            _productRepositoryMock
                .Setup(r => r.GetFilteredProductsAsync(query))
                .ReturnsAsync((products, 1));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ProductResponse>>(products))
                .Returns(new List<ProductResponse>());

            // Act
            var (result, size) = await _productsService.GetFilteredProductsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _productRepositoryMock.Verify(r => r.GetFilteredProductsAsync(query), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductResponse>>(products), Times.Once);
        }


        #endregion

        #region CountProductsAsync Tests
        [Fact]
        public async Task CountProductsAsync_ShouldReturnCount_WhenProductsExist()
        {
            // Arrange
            var expectedCount = 5;
            _productRepositoryMock
                .Setup(r => r.CountAsync())
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _productsService.CountProductsAsync();

            // Assert
            result.Should().Be(expectedCount);
            _productRepositoryMock.Verify(r => r.CountAsync(), Times.Once);
        }

        [Fact]
        public async Task CountProductsAsync_ShouldReturnZero_WhenNoProductsExist()
        {
            // Arrange
            _productRepositoryMock
                .Setup(r => r.CountAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _productsService.CountProductsAsync();

            // Assert
            result.Should().Be(0);
            _productRepositoryMock.Verify(r => r.CountAsync(), Times.Once);
        }
        #endregion

    }
}
