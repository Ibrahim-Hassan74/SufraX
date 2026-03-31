using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Categories.Requests;
using EStoreX.Core.DTO.Categories.Responses;
using EStoreX.Core.RepositoryContracts.Categories;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.Services.Categories;
using EStoreX.Core.Services.Common;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace E_StoreX.ServiceTests
{
    public class CategoriesServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CategoriesService _categoriesService;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IEntityImageManager<Category>> _entityImageManagerMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<IStringLocalizer<CategoriesService>> _localizerMock;

        public CategoriesServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _entityImageManagerMock = new Mock<IEntityImageManager<Category>>();
            _imageServiceMock = new Mock<IImageService>();
            _localizerMock = new Mock<IStringLocalizer<CategoriesService>>();
            _unitOfWorkMock.Setup(u => u.CategoryRepository).Returns(_categoryRepositoryMock.Object);
            _categoriesService = new CategoriesService(_mapperMock.Object, _unitOfWorkMock.Object, _entityImageManagerMock.Object, _imageServiceMock.Object, _localizerMock.Object);
        }

        #region CreateCategoryAsync Tests

        [Fact]
        public async Task CreateCategoryAsync_ShouldReturnCategoryResponse_WhenRequestIsValid()
        {
            // Arrange
            var request = new CategoryRequest { NameEn = "AA", DescriptionEn = "AAAAA" };
            var category = new Category { Id = Guid.NewGuid(), NameEn = request.NameEn };
            var response = new CategoryResponse(category.Id, category.NameEn, "");

            _mapperMock.Setup(m => m.Map<Category>(request)).Returns(category);
            _categoryRepositoryMock.Setup(r => r.AddAsync(category)).ReturnsAsync(category);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<CategoryResponse>(category)).Returns(response);


            // Act
            var result = await _categoriesService.CreateCategoryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be(request.NameEn);

            _mapperMock.Verify(m => m.Map<Category>(request), Times.Once);
            _categoryRepositoryMock.Verify(r => r.AddAsync(category), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<CategoryResponse>(category), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Act
            Func<Task> act = async () => { await _categoriesService.CreateCategoryAsync(null!); };

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("categoryRequest");

            _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowArgumentException_WhenRequestIsInvalid()
        {
            // Arrange
            var invalidRequest = new CategoryRequest { NameEn = "", DescriptionEn = "" }; // invalid

            _mapperMock.Setup(m => m.Map<Category>(It.IsAny<CategoryRequest>()))
                       .Returns(new Category());

            // Act
            Func<Task> act = async () => { await _categoriesService.CreateCategoryAsync(invalidRequest); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();

            _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }




        #endregion

        #region DeleteCategoryAsync Tests

        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnTrue_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category { Id = categoryId, NameEn = "Category To Delete" };

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(r => r.DeleteAsync(categoryId)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _categoriesService.DeleteCategoryAsync(categoryId);

            // Assert
            result.Should().BeTrue();

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
            _categoryRepositoryMock.Verify(r => r.DeleteAsync(categoryId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

            // Act
            var result = await _categoriesService.DeleteCategoryAsync(categoryId);

            // Assert
            result.Should().BeFalse();

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
            _categoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var categoryId = Guid.Empty;

            // Act
            Func<Task> act = async () => { await _categoriesService.DeleteCategoryAsync(categoryId); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("id");

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _categoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        #endregion

        #region GetAllCategoriesAsync Tests

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnCategories_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), NameEn = "Category 1" },
                new Category { Id = Guid.NewGuid(), NameEn = "Category 2" }
            };

            var responses = categories
                .Select(c => new CategoryResponseWithPhotos() { Id = c.Id, Name = c.NameEn });

            _categoryRepositoryMock.Setup(r => r.GetAllAsync(x => x.Photos))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryResponseWithPhotos>>(categories))
                .Returns(responses);

            // Act
            var result = await _categoriesService.GetAllCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Name.Should().Be("Category 1");

            _categoryRepositoryMock.Verify(r => r.GetAllAsync(x => x.Photos), Times.Once);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnEmpty_WhenNoCategoriesExist()
        {
            // Arrange
            var categories = new List<Category>();

            _categoryRepositoryMock.Setup(r => r.GetAllAsync(x => x.Photos))
                .ReturnsAsync(categories);

            // Act
            var result = await _categoriesService.GetAllCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _categoryRepositoryMock.Verify(r => r.GetAllAsync(x => x.Photos), Times.Once);
            _mapperMock.Verify(m => m.Map<CategoryResponse>(It.IsAny<Category>()), Times.Never);
        }

        #endregion

        #region UpdateCategoryAsync Tests

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnUpdatedCategory_WhenValidRequest()
        {
            // Arrange
            var dto = new UpdateCategoryDTO { Id = Guid.NewGuid(), NameEn = "Updated Category", DescriptionEn = "aaaaaaaaaaaa" };
            var existingCategory = new Category { Id = dto.Id, NameEn = "Old Category" };
            var updatedCategory = new Category { Id = dto.Id, NameEn = dto.NameEn };
            var response = new CategoryResponse(dto.Id, dto.NameEn, "");

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(r => r.UpdateAsync(existingCategory)).ReturnsAsync(updatedCategory);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map(dto, existingCategory)).Callback(() => existingCategory.NameEn = dto.NameEn);
            _mapperMock.Setup(m => m.Map<CategoryResponse>(existingCategory)).Returns(response);

            // Act
            var result = await _categoriesService.UpdateCategoryAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(dto.Id);
            result.Name.Should().Be(dto.NameEn);

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(existingCategory), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map(dto, existingCategory), Times.Once);
            _mapperMock.Verify(m => m.Map<CategoryResponse>(existingCategory), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Act
            Func<Task> act = async () => { await _categoriesService.UpdateCategoryAsync(null!); };

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("updateCategoryDto");

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowValidationException_WhenDtoIsInvalid()
        {
            // Arrange
            var invalidDto = new UpdateCategoryDTO { Id = Guid.NewGuid(), NameEn = "", DescriptionEn = "aaaaaaaaaaaa" };

            // Act
            Func<Task> act = async () => { await _categoriesService.UpdateCategoryAsync(invalidDto); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var dto = new UpdateCategoryDTO { Id = Guid.NewGuid(), NameEn = "NonExisting", DescriptionEn = "aaaaaaaaaaaa" };

            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => { await _categoriesService.UpdateCategoryAsync(dto); };

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Category with ID {dto.Id} not found.");

            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        #endregion

        #region GetBrandsByCategoryIdAsync Tests

        [Fact]
        public async Task GetBrandsByCategoryIdAsync_ShouldReturnBrands_WhenBrandsExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), NameEn = "Brand 1" },
                new Brand { Id = Guid.NewGuid(), NameEn = "Brand 2" }
            };

            _categoryRepositoryMock.Setup(r => r.GetBrandsByCategoryIdAsync(categoryId))
                .ReturnsAsync(brands);

            // Act
            var result = await _categoriesService.GetBrandsByCategoryIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().NameEn.Should().Be("Brand 1");

            _categoryRepositoryMock.Verify(r => r.GetBrandsByCategoryIdAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task GetBrandsByCategoryIdAsync_ShouldReturnEmpty_WhenNoBrandsExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var brands = new List<Brand>();

            _categoryRepositoryMock.Setup(r => r.GetBrandsByCategoryIdAsync(categoryId))
                .ReturnsAsync(brands);

            // Act
            var result = await _categoriesService.GetBrandsByCategoryIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _categoryRepositoryMock.Verify(r => r.GetBrandsByCategoryIdAsync(categoryId), Times.Once);
        }

        #endregion

        #region AssignBrandToCategoryAsync Tests

        [Fact]
        public async Task AssignBrandToCategoryAsync_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            // Arrange
            var categoryBrand = new CategoryBrand { CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid() };

            _categoryRepositoryMock.Setup(r => r.AssignBrandAsync(categoryBrand))
                .ReturnsAsync(true);

            // Act
            var result = await _categoriesService.AssignBrandToCategoryAsync(categoryBrand);

            // Assert
            result.Should().BeTrue();
            _categoryRepositoryMock.Verify(r => r.AssignBrandAsync(categoryBrand), Times.Once);
        }

        [Fact]
        public async Task AssignBrandToCategoryAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            var categoryBrand = new CategoryBrand { CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid() };

            _categoryRepositoryMock.Setup(r => r.AssignBrandAsync(categoryBrand))
                .ReturnsAsync(false);

            // Act
            var result = await _categoriesService.AssignBrandToCategoryAsync(categoryBrand);

            // Assert
            result.Should().BeFalse();
            _categoryRepositoryMock.Verify(r => r.AssignBrandAsync(categoryBrand), Times.Once);
        }

        #endregion

        #region UnassignBrandFromCategoryAsync Tests

        [Fact]
        public async Task UnassignBrandFromCategoryAsync_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            // Arrange
            var categoryBrand = new CategoryBrand { CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid() };

            _categoryRepositoryMock.Setup(r => r.UnassignBrandAsync(categoryBrand))
                .ReturnsAsync(true);

            // Act
            var result = await _categoriesService.UnassignBrandFromCategoryAsync(categoryBrand);

            // Assert
            result.Should().BeTrue();
            _categoryRepositoryMock.Verify(r => r.UnassignBrandAsync(categoryBrand), Times.Once);
        }

        [Fact]
        public async Task UnassignBrandFromCategoryAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            var categoryBrand = new CategoryBrand { CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid() };

            _categoryRepositoryMock.Setup(r => r.UnassignBrandAsync(categoryBrand))
                .ReturnsAsync(false);

            // Act
            var result = await _categoriesService.UnassignBrandFromCategoryAsync(categoryBrand);

            // Assert
            result.Should().BeFalse();
            _categoryRepositoryMock.Verify(r => r.UnassignBrandAsync(categoryBrand), Times.Once);
        }

        #endregion


    }
}
