using AutoFixture;
using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Brands.Response;
using EStoreX.Core.DTO.Categories.Responses;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Products;
using EStoreX.Core.Services.Common;
using EStoreX.Core.Services.Products;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace E_StoreX.ServiceTests
{
    public class BrandServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IFixture _fixture;
        private readonly Mock<IBrandRepository> _brandRepositoryMock;
        private readonly IBrandService _brandService;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEntityImageManager<Brand>> _entityImageManagerMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<IStringLocalizer<BrandService>> _localizerMock;

        public BrandServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fixture = new Fixture();
            _brandRepositoryMock = new Mock<IBrandRepository>();
            _mapperMock = new Mock<IMapper>();
            _entityImageManagerMock = new Mock<IEntityImageManager<Brand>>();
            _imageServiceMock = new Mock<IImageService>();
            _unitOfWorkMock.Setup(uow => uow.BrandRepository)
                .Returns(_brandRepositoryMock.Object);
            _localizerMock = new Mock<IStringLocalizer<BrandService>>();
            _brandService = new BrandService(_unitOfWorkMock.Object, _mapperMock.Object, _entityImageManagerMock.Object, _imageServiceMock.Object, _localizerMock.Object);
        }

        #region GetAllBrandsAsync Tests

        [Fact]
        public async Task GetAllBrandsAsync_ShouldReturnBrands_WhenBrandsExist()
        {
            // Arrange
            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), NameEn = "Brand 1" },
                new Brand { Id = Guid.NewGuid(), NameEn = "Brand 2" }
            };

            _brandRepositoryMock
                .Setup(r => r.GetAllAsync(x => x.Photos))
                .ReturnsAsync(brands);

            _mapperMock.Setup(m => m.Map<IEnumerable<BrandResponse>>(brands))
                .Returns(brands.Select(b => new BrandResponse { Id = b.Id, Name = b.NameEn }));

            // Act
            var result = await _brandService.GetAllBrandsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Name.Should().Be("Brand 1");

            _brandRepositoryMock.Verify(r => r.GetAllAsync(x => x.Photos), Times.Once);
        }

        [Fact]
        public async Task GetAllBrandsAsync_ShouldReturnEmpty_WhenNoBrandsExist()
        {
            // Arrange
            _brandRepositoryMock
                .Setup(r => r.GetAllAsync(x => x.Photos))
                .ReturnsAsync(new List<Brand>());
            _mapperMock.Setup(m => m.Map<IEnumerable<BrandResponse>>(It.IsAny<IEnumerable<Brand>>()));

            // Act
            var result = await _brandService.GetAllBrandsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _brandRepositoryMock.Verify(r => r.GetAllAsync(x => x.Photos), Times.Once);
        }


        #endregion

        #region GetBrandByIdAsync Tests

        [Fact]
        public async Task GetBrandByIdAsync_ShouldReturnBrand_WhenBrandExists()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var brand = new Brand { Id = brandId, NameEn = "Test Brand" };

            _brandRepositoryMock
                .Setup(r => r.GetByIdAsync(brandId, x => x.Photos))
                .ReturnsAsync(brand);

            _mapperMock.Setup(m => m.Map<BrandResponse?>(brand)).Returns(new BrandResponse() { Id = brand.Id, Name = brand.NameEn });

            // Act
            var result = await _brandService.GetBrandByIdAsync(brandId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(brandId);
            result.Name.Should().Be("Test Brand");

            _brandRepositoryMock.Verify(r => r.GetByIdAsync(brandId, x => x.Photos), Times.Once);
        }

        [Fact]
        public async Task GetBrandByIdAsync_ShouldReturnNull_WhenBrandDoesNotExist()
        {
            // Arrange
            var brandId = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(r => r.GetByIdAsync(brandId, x => x.Photos))
                .ReturnsAsync((Brand?)null);

            _mapperMock.Setup(m => m.Map<BrandResponse?>(It.IsAny<Brand>())).Returns((BrandResponse?)null);

            // Act
            var result = await _brandService.GetBrandByIdAsync(brandId);

            // Assert
            result.Should().BeNull();
            _brandRepositoryMock.Verify(r => r.GetByIdAsync(brandId, x => x.Photos), Times.Once);
        }

        [Fact]
        public async Task GetBrandByIdAsync_ShouldThrowArgumentException_WhenBrandIdIsEmpty()
        {
            // Arrange
            var brandId = Guid.Empty;

            // Act
            Func<Task> act = async () => { await _brandService.GetBrandByIdAsync(brandId); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();

            _brandRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }



        #endregion

        #region CreateBrandAsync Tests

        [Fact]
        public async Task CreateBrandAsync_ShouldReturnBrand_WhenNameIsValidAndNotExists()
        {
            // Arrange
            var brandName = "New Brand";

            _brandRepositoryMock
                .Setup(r => r.GetByNameAsync(brandName))
                .ReturnsAsync((Brand?)null);

            _brandRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Brand>()))
                .ReturnsAsync(new Brand() { NameEn = brandName, Id = Guid.NewGuid() });

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _brandService.CreateBrandAsync(brandName);

            // Assert
            result.Should().NotBeNull();
            result.NameEn.Should().Be(brandName);
            result.Id.Should().NotBeEmpty();

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(brandName), Times.Once);
            _brandRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateBrandAsync_ShouldThrowArgumentException_WhenNameIsNullOrEmpty(string invalidName)
        {
            // Act
            Func<Task> act = async () => { await _brandService.CreateBrandAsync(invalidName!); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .Where(e => e.ParamName == "name");

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(It.IsAny<string>()), Times.Never);
            _brandRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateBrandAsync_ShouldThrowInvalidOperationException_WhenBrandAlreadyExists()
        {
            // Arrange
            var brandName = "Existing Brand";
            var existingBrand = new Brand { Id = Guid.NewGuid(), NameEn = brandName };

            _brandRepositoryMock
                .Setup(r => r.GetByNameAsync(brandName))
                .ReturnsAsync(existingBrand);

            // Act
            Func<Task> act = async () => { await _brandService.CreateBrandAsync(brandName); };

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"A brand with the name '{brandName}' already exists.");

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(brandName), Times.Once);
            _brandRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        #endregion

        #region UpdateBrandAsync Tests

        [Fact]
        public async Task UpdateBrandAsync_ShouldReturnUpdatedBrand_WhenValidRequest()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var newName = "Updated Brand";
            var existingBrand = new Brand { Id = brandId, NameEn = "Old Brand" };

            _brandRepositoryMock.Setup(r => r.GetByNameAsync(newName)).ReturnsAsync((Brand?)null);
            _brandRepositoryMock.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync(existingBrand);
            _brandRepositoryMock.Setup(r => r.UpdateAsync(existingBrand)).ReturnsAsync(new Brand() { Id = brandId, NameEn = newName });
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _brandService.UpdateBrandAsync(brandId, newName);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(brandId);
            result.NameEn.Should().Be(newName);

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(newName), Times.Once);
            _brandRepositoryMock.Verify(r => r.GetByIdAsync(brandId), Times.Once);
            _brandRepositoryMock.Verify(r => r.UpdateAsync(existingBrand), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateBrandAsync_ShouldThrowArgumentException_WhenNewNameIsNullOrEmpty(string invalidName)
        {
            // Arrange
            var brandId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => { await _brandService.UpdateBrandAsync(brandId, invalidName!); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .Where(e => e.ParamName == "newName");

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(It.IsAny<string>()), Times.Never);
            _brandRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _brandRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Brand>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateBrandAsync_ShouldThrowInvalidOperationException_WhenNameAlreadyExists()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var newName = "Existing Brand";
            var existingBrandWithSameName = new Brand { Id = Guid.NewGuid(), NameEn = newName };

            _brandRepositoryMock.Setup(r => r.GetByNameAsync(newName)).ReturnsAsync(existingBrandWithSameName);

            // Act
            Func<Task> act = async () => { await _brandService.UpdateBrandAsync(brandId, newName); };

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"A brand with the name '{newName}' already exists.");

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(newName), Times.Once);
            _brandRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _brandRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Brand>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateBrandAsync_ShouldThrowKeyNotFoundException_WhenBrandDoesNotExist()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var newName = "New Brand";

            _brandRepositoryMock.Setup(r => r.GetByNameAsync(newName)).ReturnsAsync((Brand?)null);
            _brandRepositoryMock.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync((Brand?)null);

            // Act
            Func<Task> act = async () => { await _brandService.UpdateBrandAsync(brandId, newName); };

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Brand with ID {brandId} not found.");

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(newName), Times.Once);
            _brandRepositoryMock.Verify(r => r.GetByIdAsync(brandId), Times.Once);
            _brandRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Brand>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }



        #endregion

        #region DeleteBrandAsync Tests

        [Fact]
        public async Task DeleteBrandAsync_ShouldReturnTrue_WhenBrandExists()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var brand = new Brand { Id = brandId, NameEn = "Brand To Delete" };

            _brandRepositoryMock.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync(brand);
            _brandRepositoryMock.Setup(r => r.DeleteAsync(brandId)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _brandService.DeleteBrandAsync(brandId);

            // Assert
            result.Should().BeTrue();

            _brandRepositoryMock.Verify(r => r.GetByIdAsync(brandId), Times.Once);
            _brandRepositoryMock.Verify(r => r.DeleteAsync(brandId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteBrandAsync_ShouldReturnFalse_WhenBrandDoesNotExist()
        {
            // Arrange
            var brandId = Guid.NewGuid();

            _brandRepositoryMock.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync((Brand?)null);

            // Act
            var result = await _brandService.DeleteBrandAsync(brandId);

            // Assert
            result.Should().BeFalse();

            _brandRepositoryMock.Verify(r => r.GetByIdAsync(brandId), Times.Once);
            _brandRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteBrandAsync_ShouldThrowArgumentException_WhenBrandIdIsEmpty()
        {
            // Arrange
            var brandId = Guid.Empty;

            // Act
            Func<Task> act = async () => { await _brandService.DeleteBrandAsync(brandId); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();

            _brandRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _brandRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }


        #endregion

        #region GetCategoriesByBrandIdAsync Tests
        [Fact]
        public async Task GetCategoriesByBrandIdAsync_ShouldReturnCategories_WhenCategoriesExist()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), NameEn = "Category 1" },
                new Category { Id = Guid.NewGuid(), NameEn = "Category 2" }
            };

            _brandRepositoryMock.Setup(r => r.GetCategoriesByBrandIdAsync(brandId))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryResponse>>(categories))
                .Returns(categories.Select(c => new CategoryResponse(c.Id, c.NameEn, "")));

            // Act
            var result = await _brandService.GetCategoriesByBrandIdAsync(brandId);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Name.Should().Be("Category 1");

            _brandRepositoryMock.Verify(r => r.GetCategoriesByBrandIdAsync(brandId), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<CategoryResponse>>(categories), Times.Once);
        }

        [Fact]
        public async Task GetCategoriesByBrandIdAsync_ShouldReturnEmpty_WhenNoCategoriesExist()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var categories = new List<Category>();

            _brandRepositoryMock.Setup(r => r.GetCategoriesByBrandIdAsync(brandId))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryResponse>>(categories))
                .Returns(new List<CategoryResponse>());

            // Act
            var result = await _brandService.GetCategoriesByBrandIdAsync(brandId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _brandRepositoryMock.Verify(r => r.GetCategoriesByBrandIdAsync(brandId), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<CategoryResponse>>(categories), Times.Once);
        }



        #endregion

        #region GetBrandByNameAsync Tests

        [Fact]
        public async Task GetBrandByNameAsync_ShouldReturnBrand_WhenBrandExists()
        {
            // Arrange
            var brandName = "Test Brand";
            var brand = new Brand { Id = Guid.NewGuid(), NameEn = brandName };

            _brandRepositoryMock.Setup(r => r.GetByNameAsync(brandName))
                .ReturnsAsync(brand);
            _mapperMock.Setup(m => m.Map<BrandResponse?>(brand)).Returns(new BrandResponse() { Id = brand.Id, Name = brand.NameEn });

            // Act
            var result = await _brandService.GetBrandByNameAsync(brandName);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(brand.Id);
            result.Name.Should().Be(brandName);

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(brandName), Times.Once);
        }

        [Fact]
        public async Task GetBrandByNameAsync_ShouldReturnNull_WhenBrandDoesNotExist()
        {
            // Arrange
            var brandName = "NonExistingBrand";

            _brandRepositoryMock.Setup(r => r.GetByNameAsync(brandName))
                .ReturnsAsync((Brand?)null);

            // Act
            var result = await _brandService.GetBrandByNameAsync(brandName);

            // Assert
            result.Should().BeNull();

            _brandRepositoryMock.Verify(r => r.GetByNameAsync(brandName), Times.Once);
        }


        #endregion
    }
}
