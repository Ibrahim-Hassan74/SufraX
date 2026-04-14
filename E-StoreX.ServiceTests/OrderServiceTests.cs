using AutoFixture;
using AutoMapper;
using Domain.Entities.Baskets;
using Domain.Entities.Product;
using EStoreX.Core.Domain.Entities.Orders;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Orders;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Orders;
using EStoreX.Core.Services.Common;
using EStoreX.Core.Services.Orders;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using System.Linq.Expressions;

namespace E_StoreX.ServiceTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IFixture _fixture;
        private readonly IOrderService _orderService;
        private readonly Mock<IStringLocalizer<OrderService>> _localizerMock;

        public OrderServiceTests()
        {
            _fixture = new Fixture();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _localizerMock = new Mock<IStringLocalizer<OrderService>>();
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock.Setup(u => u.OrderRepository).Returns(_orderRepositoryMock.Object);
            _orderService = new OrderService(_unitOfWorkMock.Object, _mapperMock.Object, _paymentServiceMock.Object, _localizerMock.Object);
        }


        #region Helper Methods
        private OrderAddRequest CreateValidOrderAddRequest()
        {
            return _fixture.Build<OrderAddRequest>()
                           .With(o => o.BasketId, Guid.NewGuid().ToString())
                           .With(o => o.DeliveryMethodId, Guid.NewGuid())
                           .With(o => o.ShippingAddress, _fixture.Build<ShippingAddressDTO>()
                                        .With(x => x.ZipCode, "1234").Create())
                           .Create();
        }
        private CustomerBasket CreateValidCustomerBasket()
        {
            return _fixture.Build<CustomerBasket>()
                           .With(b => b.BasketItems, new List<BasketItem>
                           {
                               new BasketItem
                               {
                                   Id = Guid.NewGuid(),
                                   Qunatity = 2,
                                   Image = "image1.png",
                                   NameEn = "Product 1",
                                   DescriptionEn = "DescriptionEn 1",
                                   Price = 10.0m,
                                   Category = "Category 1"

                               }
                           })
                           .With(b => b.PaymentIntentId, "pi_123456")
                           .Create();
        }

        #endregion

        #region CreateOrdersAsync Tests

        [Fact]
        public async Task CreateOrdersAsync_ShouldThrowArgumentNullException_WhenOrderIsNull()
        {
            Func<Task> act = async () => await _orderService.CreateOrdersAsync(null!, "buyer@example.com");
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateOrdersAsync_ShouldThrowArgumentException_WhenBuyerEmailIsEmpty(string buyerEmail)
        {
            var order = new OrderAddRequest { BasketId = Guid.NewGuid().ToString() };
            Func<Task> act = async () => await _orderService.CreateOrdersAsync(order, buyerEmail);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateOrdersAsync_ShouldThrowInvalidOperationException_WhenBasketNotFound()
        {
            var order = CreateValidOrderAddRequest();
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId))
                            .ReturnsAsync((CustomerBasket?)null);

            Func<Task> act = async () => await _orderService.CreateOrdersAsync(order, "buyer@example.com");

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Basket not found");
        }

        [Fact]
        public async Task CreateOrdersAsync_ShouldThrowInvalidOperationException_WhenProductNotFound()
        {
            var basket = CreateValidCustomerBasket();
            var order = CreateValidOrderAddRequest();
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId)).ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            Func<Task> act = async () => await _orderService.CreateOrdersAsync(order, "buyer@example.com");

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage($"Product with ID {basket.BasketItems[0].Id} not found");
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId), Times.Once);
            _unitOfWorkMock.Verify(u => u.ProductRepository.GetByIdAsync(basket.BasketItems[0].Id), Times.Once);
        }
        [Fact]
        public async Task CreateOrdersAsync_ShouldThrowInvalidOperationException_WhenDeliveryMethodNotFound()
        {
            var product = new Product { Id = Guid.NewGuid(), NewPrice = 100, NameEn = "Test Product" };
            var basket = CreateValidCustomerBasket();
            basket.BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Id = product.Id,
                    Qunatity = 2,
                    Image = "image1.png",
                    NameEn = "Product 1",
                    DescriptionEn = "DescriptionEn 1",
                    Price = 10.0m,
                    Category = "Category 1"
                }
            };
            var order = CreateValidOrderAddRequest();

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(It.IsAny<string>())).ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
            _orderRepositoryMock.Setup(o => o.GetDeliveryMethodByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DeliveryMethod?)null);

            Func<Task> act = async () => await _orderService.CreateOrdersAsync(order, "buyer@example.com");

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Delivery method not found");
            _orderRepositoryMock.Verify(o => o.GetDeliveryMethodByIdAsync(order.DeliveryMethodId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId), Times.Once);
            _unitOfWorkMock.Verify(u => u.ProductRepository.GetByIdAsync(product.Id), Times.Exactly(basket.BasketItems.Count()));
        }

        [Fact]
        public async Task CreateOrdersAsync_ShouldDeleteExistingOrder_AndUpdatePaymentIntent()
        {
            var product = new Product { Id = Guid.NewGuid(), NewPrice = 100, NameEn = "Product" };
            var basket = CreateValidCustomerBasket();
            basket.BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Id = product.Id,
                    Qunatity = 2,
                    Image = "image1.png",
                    NameEn = "Product 1",
                    DescriptionEn = "DescriptionEn 1",
                    Price = 10.0m,
                    Category = "Category 1"
                }
            };
            var deliveryMethod = new DeliveryMethod { Id = Guid.NewGuid() };
            var orderItems = new List<OrderItem>()
            {
                new OrderItem()
                {
                    Id = Guid.NewGuid(),
                    ProductItemId = product.Id,
                    ProductNameEn = product.NameEn
                }
            };
            var existingOrder = new Order("buyer@example.com", 100, new ShippingAddress(), deliveryMethod, orderItems, basket.PaymentIntentId);

            var order = new OrderAddRequest { BasketId = basket.Id, DeliveryMethodId = deliveryMethod.Id, ShippingAddress = new ShippingAddressDTO() };

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId)).ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _orderRepositoryMock.Setup(o => o.GetDeliveryMethodByIdAsync(deliveryMethod.Id)).ReturnsAsync(deliveryMethod);
            _orderRepositoryMock.Setup(o => o.GetOrderByPaymentIntentIdAsync(basket.PaymentIntentId)).ReturnsAsync(existingOrder);
            _orderRepositoryMock.Setup(o => o.DeleteAsync(existingOrder.Id)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _paymentServiceMock.Setup(p => p.CreateOrUpdatePaymentIntentAsync(basket.Id, deliveryMethod.Id)).ReturnsAsync(basket);
            _orderRepositoryMock.Setup(o => o.AddAsync(It.IsAny<Order>())).ReturnsAsync(existingOrder);
            _mapperMock.Setup(m => m.Map<OrderResponse>(It.IsAny<Order>())).Returns(new OrderResponse { Id = existingOrder.Id });

            // Act
            var result = await _orderService.CreateOrdersAsync(order, "buyer@example.com");

            // Assert
            result.Should().NotBeNull();
            _orderRepositoryMock.Verify(o => o.DeleteAsync(existingOrder.Id), Times.Once);
            _paymentServiceMock.Verify(p => p.CreateOrUpdatePaymentIntentAsync(basket.Id, deliveryMethod.Id), Times.Once);
            _orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId), Times.Once);
            _unitOfWorkMock.Verify(u => u.ProductRepository.GetByIdAsync(product.Id), Times.Exactly(basket.BasketItems.Count()));
            _orderRepositoryMock.Verify(o => o.GetDeliveryMethodByIdAsync(deliveryMethod.Id), Times.Once);
            _orderRepositoryMock.Verify(o => o.GetOrderByPaymentIntentIdAsync(basket.PaymentIntentId), Times.Once);
            _mapperMock.Verify(m => m.Map<OrderResponse>(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrdersAsync_ShouldReturnOrderResponse_WhenOrderCreatedSuccessfully()
        {
            var product = new Product { Id = Guid.NewGuid(), NewPrice = 50, NameEn = "Product" };
            var basket = CreateValidCustomerBasket();
            basket.BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Id = product.Id,
                    Qunatity = 2,
                    Image = "image1.png",
                    NameEn = "Product 1",
                    DescriptionEn = "DescriptionEn 1",
                    Price = 10.0m,
                    Category = "Category 1"
                }
            };
            var deliveryMethod = new DeliveryMethod { Id = Guid.NewGuid() };
            var order = new OrderAddRequest { BasketId = basket.Id, DeliveryMethodId = deliveryMethod.Id, ShippingAddress = new ShippingAddressDTO() };
            var createdOrder = new Order("buyer@example.com", 150, new ShippingAddress(), deliveryMethod, new List<OrderItem>(), basket.PaymentIntentId);

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId)).ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _orderRepositoryMock.Setup(o => o.GetDeliveryMethodByIdAsync(deliveryMethod.Id)).ReturnsAsync(deliveryMethod);
            _orderRepositoryMock.Setup(o => o.GetOrderByPaymentIntentIdAsync(basket.PaymentIntentId)).ReturnsAsync((Order?)null);
            _orderRepositoryMock.Setup(o => o.AddAsync(It.IsAny<Order>())).ReturnsAsync(createdOrder);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.DeleteBasketAsync(basket.Id)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<OrderResponse>(createdOrder)).Returns(new OrderResponse { Id = createdOrder.Id });

            // Act
            var result = await _orderService.CreateOrdersAsync(order, "buyer@example.com");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(createdOrder.Id);
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.GetBasketAsync(order.BasketId), Times.Once);
            _unitOfWorkMock.Verify(u => u.ProductRepository.GetByIdAsync(product.Id), Times.Exactly(basket.BasketItems.Count()));
            _orderRepositoryMock.Verify(o => o.GetDeliveryMethodByIdAsync(deliveryMethod.Id), Times.Once);
            _orderRepositoryMock.Verify(o => o.GetOrderByPaymentIntentIdAsync(basket.PaymentIntentId), Times.Once);
            _orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.DeleteBasketAsync(basket.Id), Times.Once);
            _mapperMock.Verify(m => m.Map<OrderResponse>(createdOrder), Times.Once);

        }

        [Fact]
        public async Task CreateOrdersAsync_ShouldThrowArgumentException_WhenOrderAddRequestIsInvalid()
        {
            // Arrange
            var invalidOrder = new OrderAddRequest
            {
                DeliveryMethodId = Guid.Empty, 
                BasketId = new string('a', 101), 
                ShippingAddress = null 
            };

            // Act
            Func<Task> act = async () => await _orderService.CreateOrdersAsync(invalidOrder, "buyer@test.com");

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>();
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.GetBasketAsync(It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.ProductRepository.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _orderRepositoryMock.Verify(o => o.GetDeliveryMethodByIdAsync(It.IsAny<Guid>()), Times.Never);
            _orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
            _mapperMock.Verify(m => m.Map<OrderResponse>(It.IsAny<Order>()), Times.Never);

        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateOrdersAsync_ShouldThrowArgumentException_WhenBasketIdIsInvalid(string basketId)
        {
            // Arrange
            var invalidOrder = new OrderAddRequest
            {
                DeliveryMethodId = Guid.NewGuid(),
                BasketId = basketId, 
                ShippingAddress = new ShippingAddressDTO
                {
                    FirstName = "Test",
                    LastName = "User",
                    Street = "123 Street",
                    City = "Cairo",
                    State = "Cairo",
                    ZipCode = "12345"
                }
            };

            // Act
            Func<Task> act = async () => await _orderService.CreateOrdersAsync(invalidOrder, "buyer@test.com");

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>();
            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.GetBasketAsync(It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.ProductRepository.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _orderRepositoryMock.Verify(o => o.GetDeliveryMethodByIdAsync(It.IsAny<Guid>()), Times.Never);
            _orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
            _mapperMock.Verify(m => m.Map<OrderResponse>(It.IsAny<Order>()), Times.Never);
        }




        #endregion

        #region GetAllOrdersAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task GetAllOrdersAsync_ShouldThrowArgumentException_WhenBuyerEmailIsInvalid(string email)
        {
            Func<Task> act = async () => await _orderService.GetAllOrdersAsync(email);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Buyer email cannot be null, empty, or whitespace*");
            _orderRepositoryMock.Verify(r => r.GetOrdersByBuyerEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var buyerEmail = "test@example.com";
            _orderRepositoryMock.Setup(r => r.GetOrdersByBuyerEmailAsync(buyerEmail))
                .ReturnsAsync(new List<Order>());

            _mapperMock.Setup(m => m.Map<IEnumerable<OrderResponse>>(It.IsAny<IEnumerable<Order>>()))
                .Returns(new List<OrderResponse>());

            // Act
            var result = await _orderService.GetAllOrdersAsync(buyerEmail);

            // Assert
            result.Should().BeEmpty();
            _orderRepositoryMock.Verify(r => r.GetOrdersByBuyerEmailAsync(buyerEmail), Times.Once);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnMappedOrders_WhenOrdersExist()
        {
            // Arrange
            var buyerEmail = "buyer@test.com";
            var orders = new List<Order> { new Order("buyer@test.com", 100, null, null, new List<OrderItem>(), "payment123") };

            _orderRepositoryMock.Setup(r => r.GetOrdersByBuyerEmailAsync(buyerEmail))
                .ReturnsAsync(orders);

            var mappedOrders = new List<OrderResponse> { new OrderResponse { BuyerEmail = buyerEmail } };
            _mapperMock.Setup(m => m.Map<IEnumerable<OrderResponse>>(orders)).Returns(mappedOrders);

            // Act
            var result = await _orderService.GetAllOrdersAsync(buyerEmail);

            // Assert
            result.Should().HaveCount(1);
            result.First().BuyerEmail.Should().Be(buyerEmail);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldCallRepositoryWithCorrectEmail()
        {
            // Arrange
            var buyerEmail = "test@example.com";
            _orderRepositoryMock.Setup(r => r.GetOrdersByBuyerEmailAsync(buyerEmail))
                .ReturnsAsync(new List<Order>());

            _mapperMock.Setup(m => m.Map<IEnumerable<OrderResponse>>(It.IsAny<IEnumerable<Order>>()))
                .Returns(new List<OrderResponse>());

            // Act
            await _orderService.GetAllOrdersAsync(buyerEmail);

            // Assert
            _orderRepositoryMock.Verify(r => r.GetOrdersByBuyerEmailAsync(buyerEmail), Times.Once);
        }



        #endregion

        #region GetOrderByIdAsync Tests

        [Fact]
        public async Task GetOrderByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var id = Guid.Empty;
            var buyerEmail = "buyer@test.com";

            // Act
            Func<Task> act = async () => await _orderService.GetOrderByIdAsync(id, buyerEmail);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Order ID cannot be empty*");
            _orderRepositoryMock.Verify(_ => _.GetOrderByIdAsync(id, buyerEmail), Times.Never);

        }
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task GetOrderByIdAsync_ShouldThrowArgumentException_WhenBuyerEmailIsInvalid(string email)
        {
            var id = Guid.NewGuid();
            Func<Task> act = async () => await _orderService.GetOrderByIdAsync(id, email);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Buyer email cannot be null, empty, or whitespace*");
            _orderRepositoryMock.Verify(_ => _.GetOrderByIdAsync(id, email), Times.Never);

        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnNull_WhenOrderNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var buyerEmail = "buyer@test.com";

            _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(id, buyerEmail))
                .ReturnsAsync((Order?)null);

            _mapperMock.Setup(m => m.Map<OrderResponse>(null))
                .Returns((OrderResponse?)null);

            // Act
            var result = await _orderService.GetOrderByIdAsync(id, buyerEmail);

            // Assert
            result.Should().BeNull();
            _orderRepositoryMock.Verify(_ => _.GetOrderByIdAsync(id, buyerEmail), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnMappedOrder_WhenOrderExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var buyerEmail = "buyer@test.com";
            var order = new Order(buyerEmail, 100, null, null, new List<OrderItem>(), "pay123");

            _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(id, buyerEmail))
                .ReturnsAsync(order);

            var mappedOrder = new OrderResponse { BuyerEmail = buyerEmail };
            _mapperMock.Setup(m => m.Map<OrderResponse>(order)).Returns(mappedOrder);

            // Act
            var result = await _orderService.GetOrderByIdAsync(id, buyerEmail);

            // Assert
            result.Should().NotBeNull();
            result.BuyerEmail.Should().Be(buyerEmail);
        }


        #endregion

        #region GetAllOrders

        [Fact]
        public async Task GetAllOrders_ShouldReturnEmpty_WhenNoOrdersExist()
        {
            // Arrange
            _orderRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Order, object>>[]>()))
                .ReturnsAsync(new List<Order>()); // no orders

            // Act
            var result = await _orderService.GetAllOrders();

            // Assert
            result.Should().BeEmpty();
            _orderRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Order, object>>[]>()), Times.Once);
        }

        [Fact]
        public async Task GetAllOrders_ShouldReturnMappedOrders_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order("buyer@test.com", 200, null, null, new List<OrderItem>(), "pay123"),
                new Order("buyer2@test.com", 300, null, null, new List<OrderItem>(), "pay456")
            };

            _orderRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Order, object>>[]>()))
                .ReturnsAsync(orders);

            var mappedOrders = new List<OrderResponse>
            {
                new OrderResponse { BuyerEmail = "buyer@test.com" },
                new OrderResponse { BuyerEmail = "buyer2@test.com" }
            };

            _mapperMock.Setup(m => m.Map<IEnumerable<OrderResponse>>(orders)).Returns(mappedOrders);

            // Act
            var result = await _orderService.GetAllOrders();

            // Assert
            result.Should().HaveCount(2);
            result.Select(r => r.BuyerEmail).Should().Contain(new[] { "buyer@test.com", "buyer2@test.com" });
            _orderRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Order, object>>[]>()), Times.Once);
        }

        [Fact]
        public async Task GetAllOrders_ShouldReturnEmpty_WhenRepositoryReturnsNull()
        {
            // Arrange
            _orderRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Order, object>>[]>()))
                .ReturnsAsync((IEnumerable<Order>?)null);

            // Act
            var result = await _orderService.GetAllOrders();

            // Assert
            result.Should().BeEmpty();
            _orderRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<Expression<Func<Order, object>>[]>()), Times.Once);
        }



        #endregion
    }
}
