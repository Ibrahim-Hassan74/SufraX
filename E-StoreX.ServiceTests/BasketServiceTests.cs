using AutoMapper;
using Domain.Entities.Baskets;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Basket;
using EStoreX.Core.RepositoryContracts.Basket;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.Services.Basket;
using EStoreX.Core.Services.Common;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

namespace E_StoreX.ServiceTests
{
    public class BasketServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly BasketService _basketService;
        private readonly Mock<ICustomerBasketRepository> _customerBasketRepositoryMock;
        private readonly Mock<IStringLocalizer<BasketService>> _localizerMock;

        public BasketServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _customerBasketRepositoryMock = new Mock<ICustomerBasketRepository>();
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository).Returns(_customerBasketRepositoryMock.Object);
            _localizerMock = new Mock<IStringLocalizer<BasketService>>();
            _basketService = new BasketService(_unitOfWorkMock.Object, _mapperMock.Object, _localizerMock.Object);
        }

        #region GetBasketAsync Tests
        [Fact]
        public async Task GetBasketAsync_ShouldReturnBasketDto_WhenBasketExists()
        {
            // Arrange
            var basketId = "basket-123";
            var basket = new CustomerBasket(basketId);
            var basketDto = new CustomerBasketDTO { Id = basketId };

            _customerBasketRepositoryMock
                .Setup(r => r.GetBasketAsync(basketId))
                .ReturnsAsync(basket);

            _mapperMock
                .Setup(m => m.Map<CustomerBasketDTO>(basket))
                .Returns(basketDto);

            // Act
            var result = await _basketService.GetBasketAsync(basketId);

            // Assert
            result.Should().BeEquivalentTo(basketDto);
            _customerBasketRepositoryMock.Verify(r => r.GetBasketAsync(basketId), Times.Once);
            _mapperMock.Verify(m => m.Map<CustomerBasketDTO>(basket), Times.Once);
        }

        [Fact]
        public async Task GetBasketAsync_ShouldReturnNull_WhenBasketDoesNotExist()
        {
            // Arrange
            var basketId = "basket-456";
            _customerBasketRepositoryMock
                .Setup(r => r.GetBasketAsync(basketId))
                .ReturnsAsync((CustomerBasket)null);

            _mapperMock
                .Setup(m => m.Map<CustomerBasketDTO>(null))
                .Returns((CustomerBasketDTO)null);

            // Act
            var result = await _basketService.GetBasketAsync(basketId);

            // Assert
            result.Should().BeNull();
            _customerBasketRepositoryMock.Verify(r => r.GetBasketAsync(basketId), Times.Once);
        }

        #endregion

        #region UpdateBasketAsync Tests
        [Fact]
        public async Task UpdateBasketAsync_ShouldReturnNull_WhenAllItemsInvalid()
        {
            // Arrange
            var basketDto = new BasketAddRequest
            {
                BasketId = "basket-1",
                BasketItem = new BasketItem { Id = Guid.NewGuid(), Qunatity = -1 }
            };

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _basketService.AddItemToBasketAsync(basketDto);

            // Assert
            result.Should().BeNull();
            _unitOfWorkMock.Verify(u => u.ProductRepository.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
            _customerBasketRepositoryMock.Verify(r => r.UpdateBasketAsync(It.IsAny<CustomerBasket>()), Times.Never);
            _customerBasketRepositoryMock.Verify(r => r.GetBasketAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBasketAsync_ShouldUpdateExistingBasket_WhenBasketExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, NameEn = "Laptop", QuantityAvailable = 10, NewPrice = 1000 };

            var basketDto = new BasketAddRequest
            {
                BasketId = "basket-1",
                BasketItem = new BasketItem { Id = productId, Qunatity = 2, Category = "Electronics", Image = "img.png" }
            };

            var existingBasket = new CustomerBasket("basket-1")
            {
                BasketItems = new List<BasketItem>
                {
                    new BasketItem { Id = productId, Qunatity = 1, Price = 900 }
                }
            };

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket-1"))
                .ReturnsAsync(existingBasket);

            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(existingBasket))
                .ReturnsAsync(existingBasket);

            var expectedDto = new CustomerBasketDTO { Id = "basket-1" };
            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(existingBasket))
                .Returns(expectedDto);

            // Act
            var result = await _basketService.AddItemToBasketAsync(basketDto);

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
            existingBasket.BasketItems.First().Qunatity.Should().Be(3); // 1 old + 2 new
            existingBasket.BasketItems.First().Price.Should().Be(1000); // updated price
            _customerBasketRepositoryMock.Verify(b => b.GetBasketAsync(It.IsAny<string>()), Times.Once);
            _customerBasketRepositoryMock.Verify(b => b.UpdateBasketAsync(existingBasket), Times.Once);
            _mapperMock.Verify(b => b.Map<CustomerBasketDTO>(existingBasket), Times.Once);
            _unitOfWorkMock.Verify(p => p.ProductRepository.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBasketAsync_ShouldCreateNewBasket_WhenBasketDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, NameEn = "Phone", QuantityAvailable = 5, NewPrice = 500 };

            var basketDto = new BasketAddRequest
            {
                BasketId = "basket-2",
                BasketItem = new BasketItem { Id = productId, Qunatity = 1, Category = "Mobiles", Image = "img.png" }
            };

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket-2"))
                .ReturnsAsync((CustomerBasket?)null);

            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
                .ReturnsAsync((CustomerBasket basket) => basket);

            var expectedDto = new CustomerBasketDTO { Id = "basket-2" };
            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(It.IsAny<CustomerBasket>()))
                .Returns(expectedDto);

            // Act
            var result = await _basketService.AddItemToBasketAsync(basketDto);

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
            _customerBasketRepositoryMock.Verify(r => r.UpdateBasketAsync(It.IsAny<CustomerBasket>()), Times.Once);
            _mapperMock.Verify(b => b.Map<CustomerBasketDTO>(It.IsAny<CustomerBasket>()), Times.Once);
            _unitOfWorkMock.Verify(p => p.ProductRepository.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
            _customerBasketRepositoryMock.Verify(b => b.GetBasketAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBasketAsync_ShouldAddNewItems_WhenBasketExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basketDto = new BasketAddRequest
            {
                BasketId = "basket1",
                BasketItem =
                    new BasketItem { Id = productId, Qunatity = 2 }

            };

            var product = new Product { Id = productId, NameEn = "Test", NewPrice = 50, QuantityAvailable = 10, DescriptionEn = "desc" };
            var existingBasket = new CustomerBasket("basket1");

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync("basket1"))
                .ReturnsAsync(existingBasket);
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
                .ReturnsAsync(existingBasket);
            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(It.IsAny<CustomerBasket>()))
                .Returns(new CustomerBasketDTO(basketDto.BasketId)
                {
                    BasketItems = new List<BasketItem>
                    {
                        basketDto.BasketItem
                    }
                });

            // Act
            var result = await _basketService.AddItemToBasketAsync(basketDto);

            // Assert
            result.Should().NotBeNull();
            result!.BasketItems.Should().ContainSingle(i => i.Id == productId);
        }


        [Fact]
        public async Task UpdateBasketAsync_ShouldUpdateQuantityAndPrice_WhenItemAlreadyExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basketDto = new BasketAddRequest
            {
                BasketId = "basket1",
                BasketItem =
                    new BasketItem { Id = productId, Qunatity = 3 }

            };

            var product = new Product { Id = productId, NameEn = "Test", NewPrice = 100, QuantityAvailable = 10, DescriptionEn = "desc" };
            var existingBasket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem> { new BasketItem { Id = productId, Qunatity = 1, Price = 50 } }
            };

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync("basket1"))
                .ReturnsAsync(existingBasket);
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
                .ReturnsAsync(existingBasket);

            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(It.IsAny<CustomerBasket>()))
                .Returns(new CustomerBasketDTO(existingBasket.Id)
                {
                    BasketItems = new List<BasketItem>
                    {
                        new BasketItem { Id = productId, Qunatity = 4, Price = 100 } // Updated quantity and price
                    }
                });

            // Act
            var result = await _basketService.AddItemToBasketAsync(basketDto);

            // Assert
            result.Should().NotBeNull();
            var updatedItem = result!.BasketItems.First(i => i.Id == productId);
            updatedItem.Qunatity.Should().Be(4); // old 1 + new 3
            updatedItem.Price.Should().Be(100); // updated price
        }



        #endregion

        #region DeleteBasketAsync Tests

        [Fact]
        public async Task DeleteBasketAsync_ShouldReturnTrue_WhenBasketDeletedSuccessfully()
        {
            // Arrange
            var basketId = "basket1";
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.DeleteBasketAsync(basketId))
                .ReturnsAsync(true);

            // Act
            var result = await _basketService.DeleteBasketAsync(basketId);

            // Assert
            result.Should().BeTrue();
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.DeleteBasketAsync(basketId), Times.Once);
        }
        [Fact]
        public async Task DeleteBasketAsync_ShouldReturnFalse_WhenBasketDoesNotExist()
        {
            // Arrange
            var basketId = "basketX";
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.DeleteBasketAsync(basketId))
                .ReturnsAsync(false);

            // Act
            var result = await _basketService.DeleteBasketAsync(basketId);

            // Assert
            result.Should().BeFalse();
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.DeleteBasketAsync(basketId), Times.Once);
        }
        [Fact]
        public async Task DeleteBasketAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var basketId = "basketError";
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.DeleteBasketAsync(basketId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var act = async () => await _basketService.DeleteBasketAsync(basketId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("DB error");
        }

        #endregion

        #region MergeBasketsAsync Tests

        [Fact]
        public async Task MergeBasketsAsync_ShouldReturnNull_WhenBothGuestAndUserBasketsAreNull()
        {
            // Arrange
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("guest")).ReturnsAsync((CustomerBasket?)null);
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("user")).ReturnsAsync((CustomerBasket?)null);

            // Act
            var result = await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task MergeBasketsAsync_ShouldMoveGuestBasketToUser_WhenUserBasketIsNull()
        {
            // Arrange
            var guestBasket = new CustomerBasket("guest") { BasketItems = new List<BasketItem>() };
            var updatedBasket = new CustomerBasket("user") { BasketItems = new List<BasketItem>() };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("guest")).ReturnsAsync(guestBasket);
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("user")).ReturnsAsync((CustomerBasket?)null);
            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(It.IsAny<CustomerBasket>())).ReturnsAsync(updatedBasket);

            var mappedBasket = new CustomerBasketDTO { Id = "user" };
            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(updatedBasket)).Returns(mappedBasket);

            // Act
            var result = await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            result.Id.Should().Be("user");
            _customerBasketRepositoryMock.Verify(r => r.DeleteBasketAsync("guest"), Times.Once);
            _customerBasketRepositoryMock.Verify(r => r.UpdateBasketAsync(It.Is<CustomerBasket>(b => b.Id == "user")), Times.Once);
        }

        [Fact]
        public async Task MergeBasketsAsync_ShouldReturnUserBasket_WhenGuestBasketIsNull()
        {
            // Arrange
            var userBasket = new CustomerBasket("user") { BasketItems = new List<BasketItem>() };
            var mappedUserBasket = new CustomerBasketDTO { Id = "user" };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("guest")).ReturnsAsync((CustomerBasket?)null);
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("user")).ReturnsAsync(userBasket);
            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(userBasket)).Returns(mappedUserBasket);

            // Act
            var result = await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            result.Id.Should().Be("user");
        }

        [Fact]
        public async Task MergeBasketsAsync_ShouldIncreaseQuantity_WhenGuestAndUserHaveSameItem()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var guestBasket = new CustomerBasket("guest")
            {
                BasketItems = new List<BasketItem> { new BasketItem { Id = itemId, Qunatity = 2 } }
            };
            var userBasket = new CustomerBasket("user")
            {
                BasketItems = new List<BasketItem> { new BasketItem { Id = itemId, Qunatity = 3 } }
            };
            var updatedBasket = userBasket; // بعد الدمج
            var mappedBasket = new CustomerBasketDTO { Id = "user" };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("guest")).ReturnsAsync(guestBasket);
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("user")).ReturnsAsync(userBasket);
            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(userBasket)).ReturnsAsync(updatedBasket);
            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(updatedBasket)).Returns(mappedBasket);

            // Act
            var result = await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            userBasket.BasketItems.First().Qunatity.Should().Be(5); // 3+2
            result.Id.Should().Be("user");
            _customerBasketRepositoryMock.Verify(r => r.DeleteBasketAsync("guest"), Times.Once);
        }

        [Fact]
        public async Task MergeBasketsAsync_ShouldAddNewItemsFromGuest_WhenUserDoesNotHaveThem()
        {
            // Arrange
            var guestItemId = Guid.NewGuid();
            var guestBasket = new CustomerBasket("guest")
            {
                BasketItems = new List<BasketItem> { new BasketItem { Id = guestItemId, Qunatity = 1 } }
            };
            var userBasket = new CustomerBasket("user") { BasketItems = new List<BasketItem>() };
            var updatedBasket = userBasket;
            var mappedBasket = new CustomerBasketDTO { Id = "user" };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("guest")).ReturnsAsync(guestBasket);
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("user")).ReturnsAsync(userBasket);
            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(userBasket)).ReturnsAsync(updatedBasket);
            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(updatedBasket)).Returns(mappedBasket);

            // Act
            var result = await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            userBasket.BasketItems.Should().ContainSingle(i => i.Id == guestItemId);
            result.Id.Should().Be("user");
        }

        [Fact]
        public async Task MergeBasketsAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _customerBasketRepositoryMock
                .Setup(r => r.GetBasketAsync("guest"))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            Func<Task> act = async () => await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
        }

        [Fact]
        public async Task MergeBasketsAsync_ShouldThrowException_WhenUpdateFails()
        {
            // Arrange
            var guestBasket = new CustomerBasket("guest") { BasketItems = new List<BasketItem>() };
            var userBasket = new CustomerBasket("user") { BasketItems = new List<BasketItem>() };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("guest")).ReturnsAsync(guestBasket);
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("user")).ReturnsAsync(userBasket);
            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(userBasket))
                .ThrowsAsync(new Exception("Update failed"));

            // Act
            Func<Task> act = async () => await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Update failed");
        }

        [Fact]
        public async Task MergeBasketsAsync_ShouldThrowException_WhenDeleteFails()
        {
            // Arrange
            var guestBasket = new CustomerBasket("guest") { BasketItems = new List<BasketItem>() };
            var userBasket = new CustomerBasket("user") { BasketItems = new List<BasketItem>() };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("guest")).ReturnsAsync(guestBasket);
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("user")).ReturnsAsync((CustomerBasket?)null);
            _customerBasketRepositoryMock.Setup(r => r.DeleteBasketAsync("guest"))
                .ThrowsAsync(new Exception("Delete failed"));

            // Act
            Func<Task> act = async () => await _basketService.MergeBasketsAsync("guest", "user");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Delete failed");
        }


        #endregion

        #region DecrementItemQuantityAsync Tests

        [Fact]
        public async Task DecreaseItemQuantityAsync_ShouldReturnNull_WhenBasketNotFound()
        {
            // Arrange
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync((CustomerBasket?)null);

            // Act
            var result = await _basketService.DecreaseItemQuantityAsync("basket1", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DecreaseItemQuantityAsync_ShouldReturnNull_WhenItemNotFound()
        {
            // Arrange
            var basket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem>()
            };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            // Act
            var result = await _basketService.DecreaseItemQuantityAsync("basket1", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task DecreaseItemQuantityAsync_ShouldDecreaseQuantity_WhenItemExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem>
        {
            new BasketItem { Id = productId, Qunatity = 3 }
        }
            };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(basket))
                .ReturnsAsync(basket);

            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(basket))
                .Returns(new CustomerBasketDTO { Id = "basket1" });

            // Act
            var result = await _basketService.DecreaseItemQuantityAsync("basket1", productId);

            // Assert
            basket.BasketItems.First().Qunatity.Should().Be(2);
            result.Should().NotBeNull();
            result!.Id.Should().Be("basket1");
        }
        [Fact]
        public async Task DecreaseItemQuantityAsync_ShouldRemoveItem_WhenQuantityReachesZero()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem>
        {
            new BasketItem { Id = productId, Qunatity = 1 }
        }
            };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(basket))
                .ReturnsAsync(basket);

            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(basket))
                .Returns(new CustomerBasketDTO { Id = "basket1" });

            // Act
            var result = await _basketService.DecreaseItemQuantityAsync("basket1", productId);

            // Assert
            basket.BasketItems.Should().BeEmpty();
            result.Should().NotBeNull();
            result!.Id.Should().Be("basket1");
        }
        [Fact]
        public async Task DecreaseItemQuantityAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _basketService.DecreaseItemQuantityAsync("basket1", Guid.NewGuid());

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("DB error");
        }


        #endregion

        #region IncreaseItemQuantityAsync Tests

        [Fact]
        public async Task IncreaseItemQuantityAsync_ShouldReturnNull_WhenBasketNotFound()
        {
            // Arrange
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync((CustomerBasket?)null);

            // Act
            var result = await _basketService.IncreaseItemQuantityAsync("basket1", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task IncreaseItemQuantityAsync_ShouldReturnNull_WhenItemNotFound()
        {
            // Arrange
            var basket = new CustomerBasket("basket1");
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            // Act
            var result = await _basketService.IncreaseItemQuantityAsync("basket1", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task IncreaseItemQuantityAsync_ShouldReturnNull_WhenProductNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem>
                {
                    new BasketItem { Id = productId, Qunatity = 1 }
                }
            };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            _unitOfWorkMock.Setup(r => r.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _basketService.IncreaseItemQuantityAsync("basket1", productId);

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task IncreaseItemQuantityAsync_ShouldIncreaseQuantity_WhenStockAvailable()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem>
                {
                    new BasketItem { Id = productId, Qunatity = 1 }
                }
            };

            var product = new Product { Id = productId, QuantityAvailable = 5 };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            _unitOfWorkMock.Setup(r => r.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(basket))
                .ReturnsAsync(basket);

            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(basket))
                .Returns(new CustomerBasketDTO { Id = "basket1" });

            // Act
            var result = await _basketService.IncreaseItemQuantityAsync("basket1", productId);

            // Assert
            basket.BasketItems.First().Qunatity.Should().Be(2);
            result.Should().NotBeNull();
            result!.Id.Should().Be("basket1");
        }
        [Fact]
        public async Task IncreaseItemQuantityAsync_ShouldNotIncrease_WhenQuantityEqualsStock()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem>
                {
                    new BasketItem { Id = productId, Qunatity = 5 }
                }
            };

            var product = new Product { Id = productId, QuantityAvailable = 5 };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            _unitOfWorkMock.Setup(r => r.ProductRepository.GetByIdAsync(productId))
                 .ReturnsAsync(product);

            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(basket))
                .Returns(new CustomerBasketDTO { Id = "basket1" });

            // Act
            var result = await _basketService.IncreaseItemQuantityAsync("basket1", productId);

            // Assert
            basket.BasketItems.First().Qunatity.Should().Be(5); // unchanged
            result.Should().NotBeNull();
            result!.Id.Should().Be("basket1");
        }

        [Fact]
        public async Task IncreaseItemQuantityAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _basketService.IncreaseItemQuantityAsync("basket1", Guid.NewGuid());

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("DB error");
        }


        #endregion

        #region RemoveItemAsync Tests
        [Fact]
        public async Task RemoveItemAsync_ShouldReturnNull_WhenBasketNotFound()
        {
            // Arrange
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync((CustomerBasket?)null);

            // Act
            var result = await _basketService.RemoveItemAsync("basket1", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task RemoveItemAsync_ShouldReturnNull_WhenItemNotFound()
        {
            // Arrange
            var basket = new CustomerBasket("basket1");
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            // Act
            var result = await _basketService.RemoveItemAsync("basket1", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task RemoveItemAsync_ShouldRemoveItem_WhenItemExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket("basket1")
            {
                BasketItems = new List<BasketItem>
                {
                    new BasketItem { Id = productId, Qunatity = 2 }
                }
            };

            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ReturnsAsync(basket);

            _customerBasketRepositoryMock.Setup(r => r.UpdateBasketAsync(basket))
                .ReturnsAsync(basket);

            _mapperMock.Setup(m => m.Map<CustomerBasketDTO>(basket))
                .Returns(new CustomerBasketDTO { Id = "basket1" });

            // Act
            var result = await _basketService.RemoveItemAsync("basket1", productId);

            // Assert
            basket.BasketItems.Should().BeEmpty();
            result.Should().NotBeNull();
            result!.Id.Should().Be("basket1");
        }

        [Fact]
        public async Task RemoveItemAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _customerBasketRepositoryMock.Setup(r => r.GetBasketAsync("basket1"))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _basketService.RemoveItemAsync("basket1", Guid.NewGuid());

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("DB error");
        }


        #endregion
    }
}
