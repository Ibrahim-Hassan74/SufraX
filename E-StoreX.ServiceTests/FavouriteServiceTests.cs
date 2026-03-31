using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.Domain.Entities.Favourites;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Favourites;
using EStoreX.Core.ServiceContracts.Favourites;
using EStoreX.Core.Services.Common;
using EStoreX.Core.Services.Favourites;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace E_StoreX.ServiceTests
{
    public class FavouriteServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IFavouriteService _favouriteService;
        private readonly Mock<IFavouriteRepository> _favouriteRepositoryMock;
        private readonly Mock<IStringLocalizer<FavouriteService>> _localizerMock;

        public FavouriteServiceTests()
        {
            _favouriteRepositoryMock = new Mock<IFavouriteRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _localizerMock = new Mock<IStringLocalizer<FavouriteService>>();
            _unitOfWorkMock.Setup(u => u.FavouriteRepository).Returns(_favouriteRepositoryMock.Object);
            _favouriteService = new FavouriteService(_unitOfWorkMock.Object, _mapperMock.Object, _localizerMock.Object);
        }

        #region AddToFavouriteAsync Tests

        [Fact]
        public async Task AddToFavouriteAsync_ShouldReturnConflict_WhenProductAlreadyInFavourites()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var favourite = new Favourite { UserId = userId, ProductId = productId };

            _favouriteRepositoryMock.Setup(r => r.IsFavouriteAsync(It.Is<Favourite>(
                f => f.UserId == userId && f.ProductId == productId)))
                .ReturnsAsync(true);

            // Act
            var result = await _favouriteService.AddToFavouriteAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(409);
            result.Message.Should().Be("Product already in favourites");

            _favouriteRepositoryMock.Verify(r => r.IsFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
            _favouriteRepositoryMock.Verify(r => r.AddToFavouriteAsync(It.IsAny<Favourite>()), Times.Never);
        }

        [Fact]
        public async Task AddToFavouriteAsync_ShouldReturnSuccess_WhenProductIsAdded()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var favourite = new Favourite { UserId = userId, ProductId = productId };

            _favouriteRepositoryMock.Setup(r => r.IsFavouriteAsync(It.Is<Favourite>(
                f => f.UserId == userId && f.ProductId == productId)))
                .ReturnsAsync(false);

            _favouriteRepositoryMock.Setup(r => r.AddToFavouriteAsync(It.Is<Favourite>(
                f => f.UserId == userId && f.ProductId == productId)))
                .ReturnsAsync(new Favourite() { ProductId = favourite.ProductId, UserId = favourite.UserId, CreatedAt = DateTime.UtcNow });

            // Act
            var result = await _favouriteService.AddToFavouriteAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Product added to favourites successfully");

            _favouriteRepositoryMock.Verify(r => r.IsFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
            _favouriteRepositoryMock.Verify(r => r.AddToFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
        }

        #endregion

        #region RemoveFromFavouriteAsync Tests

        [Fact]
        public async Task RemoveFromFavouriteAsync_ShouldReturnNotFound_WhenProductNotInFavourites()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var favourite = new Favourite { UserId = userId, ProductId = productId };

            _favouriteRepositoryMock.Setup(r => r.IsFavouriteAsync(It.Is<Favourite>(
                f => f.UserId == userId && f.ProductId == productId)))
                .ReturnsAsync(false);

            // Act
            var result = await _favouriteService.RemoveFromFavouriteAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Message.Should().Be("Product not found in favourites");

            _favouriteRepositoryMock.Verify(r => r.IsFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
            _favouriteRepositoryMock.Verify(r => r.RemoveFromFavouriteAsync(It.IsAny<Favourite>()), Times.Never);
        }

        [Fact]
        public async Task RemoveFromFavouriteAsync_ShouldReturnInternalServerError_WhenRemovalFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var favourite = new Favourite { UserId = userId, ProductId = productId };

            _favouriteRepositoryMock.Setup(r => r.IsFavouriteAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(true);

            _favouriteRepositoryMock.Setup(r => r.RemoveFromFavouriteAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(false);

            // Act
            var result = await _favouriteService.RemoveFromFavouriteAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(500);
            result.Message.Should().Be("Failed to remove product from favourites");

            _favouriteRepositoryMock.Verify(r => r.IsFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
            _favouriteRepositoryMock.Verify(r => r.RemoveFromFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
        }

        [Fact]
        public async Task RemoveFromFavouriteAsync_ShouldReturnSuccess_WhenProductRemoved()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var favourite = new Favourite { UserId = userId, ProductId = productId };

            _favouriteRepositoryMock.Setup(r => r.IsFavouriteAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(true);

            _favouriteRepositoryMock.Setup(r => r.RemoveFromFavouriteAsync(It.IsAny<Favourite>()))
                .ReturnsAsync(true);

            // Act
            var result = await _favouriteService.RemoveFromFavouriteAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Product removed from favourites successfully");

            _favouriteRepositoryMock.Verify(r => r.IsFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
            _favouriteRepositoryMock.Verify(r => r.RemoveFromFavouriteAsync(It.IsAny<Favourite>()), Times.Once);
        }

        #endregion

        #region GetUserFavouritesAsync Tests

        [Fact]
        public async Task GetUserFavouritesAsync_ShouldReturnMappedProducts_WhenUserHasFavourites()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var favourites = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), NameEn = "Product1" },
                new Product { Id = Guid.NewGuid(), NameEn = "Product2" }
            };

            var expectedResponse = new List<ProductResponse>
            {
                new ProductResponse { Id = favourites[0].Id, Name = "Product1" },
                new ProductResponse { Id = favourites[1].Id, Name = "Product2" }
            };

            _favouriteRepositoryMock.Setup(r => r.GetUserFavouritesAsync(userId))
                .ReturnsAsync(favourites);

            _mapperMock.Setup(m => m.Map<List<ProductResponse>>(favourites))
                .Returns(expectedResponse);

            // Act
            var result = await _favouriteService.GetUserFavouritesAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponse);

            _favouriteRepositoryMock.Verify(r => r.GetUserFavouritesAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<List<ProductResponse>>(favourites), Times.Once);
        }

        [Fact]
        public async Task GetUserFavouritesAsync_ShouldReturnEmptyList_WhenUserHasNoFavourites()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var favourites = new List<Product>();
            var expectedResponse = new List<ProductResponse>();

            _favouriteRepositoryMock.Setup(r => r.GetUserFavouritesAsync(userId))
                .ReturnsAsync(favourites);

            _mapperMock.Setup(m => m.Map<List<ProductResponse>>(favourites))
                .Returns(expectedResponse);

            // Act
            var result = await _favouriteService.GetUserFavouritesAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _favouriteRepositoryMock.Verify(r => r.GetUserFavouritesAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<List<ProductResponse>>(favourites), Times.Once);
        }

        #endregion

    }
}
