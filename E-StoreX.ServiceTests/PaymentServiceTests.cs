using Domain.Entities.Baskets;
using Domain.Entities.Product;
using EStoreX.Core.BackgroundJobs.Interfaces;
using EStoreX.Core.BackgroundJobs.Wrapper;
using EStoreX.Core.Domain.Entities.Orders;
using EStoreX.Core.Domain.Options;
using EStoreX.Core.Enums;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Orders;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Core.Services.Common;
using EStoreX.Core.Services.Products;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using System.Linq.Expressions;

namespace E_StoreX.ServiceTests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IOptions<StripeSettings> _stripeSettingsOptions;
        private readonly Mock<PaymentIntentService> _paymentIntentServiceMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly PaymentService _paymentService;
        private readonly Mock<IBackgroundJobClientWrapper> _backgroundJobClientMock;
        private readonly Mock<IStringLocalizer<PaymentService>> _localizerMock;

        public PaymentServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            var stripeSettings = new StripeSettings { SecretKey = "sk_test_123", PublishableKey = "pk_test_123" };
            _stripeSettingsOptions = Options.Create(stripeSettings);
            // Mock ProductRepository
            _productRepoMock = new Mock<IProductRepository>();
            _unitOfWorkMock.Setup(u => u.ProductRepository).Returns(_productRepoMock.Object);


            _orderRepoMock = new Mock<IOrderRepository>();
            _unitOfWorkMock.Setup(u => u.OrderRepository).Returns(_orderRepoMock.Object);
            // Mock PaymentIntentService
            _paymentIntentServiceMock = new Mock<PaymentIntentService>();
            _paymentIntentServiceMock.Setup(s => s.CreateAsync(It.IsAny<PaymentIntentCreateOptions>(), null, default))
                .ReturnsAsync(new PaymentIntent { Id = "pi_123", ClientSecret = "secret_123" });

            _paymentIntentServiceMock.Setup(s => s.UpdateAsync(It.IsAny<string>(), It.IsAny<PaymentIntentUpdateOptions>(), null, default))
                .ReturnsAsync(new PaymentIntent { Id = "pi_123", ClientSecret = "secret_123" });

            _backgroundJobClientMock = new Mock<IBackgroundJobClientWrapper>();
            _backgroundJobClientMock.Setup(b => b.Enqueue<IEmailJob>(It.IsAny<Expression<Action<IEmailJob>>>()))
                .Verifiable();

            _localizerMock = new Mock<IStringLocalizer<PaymentService>>();

            // Inject mock service
            _paymentService = new PaymentService(_unitOfWorkMock.Object, _stripeSettingsOptions, _paymentIntentServiceMock.Object, _backgroundJobClientMock.Object, _localizerMock.Object);
        }
        #region CreateOrUpdatePaymentIntentAsync Tests
        [Fact]
        public async Task CreateOrUpdatePaymentIntentAsync_ShouldThrow_WhenBasketNotFound()
        {
            var basketId = "nonexist";
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(basketId))
                           .ReturnsAsync((CustomerBasket)null);

            Func<Task> act = async () => await _paymentService.CreateOrUpdatePaymentIntentAsync(basketId, null);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage($"Basket with ID {basketId} not found.");
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntentAsync_ShouldThrow_WhenProductNotFound()
        {
            var basket = new CustomerBasket { Id = "basket1", BasketItems = new List<BasketItem> { new BasketItem { Id = Guid.NewGuid(), Qunatity = 1 } } };
            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(basket.Id))
                           .ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(It.IsAny<Guid>()))
                           .ReturnsAsync((Domain.Entities.Product.Product?)null);

            Func<Task> act = async () => await _paymentService.CreateOrUpdatePaymentIntentAsync(basket.Id, null);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage($"Product with ID {basket.BasketItems[0].Id} not found.");
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntentAsync_ShouldThrow_WhenNotEnoughStock()
        {
            var productId = Guid.NewGuid();
            var basket = new CustomerBasket
            {
                Id = "basket1",
                BasketItems = new List<BasketItem> { new BasketItem { Id = productId, Qunatity = 5 } }
            };

            var product = new Domain.Entities.Product.Product { Id = productId, NameEn = "Prod1", QuantityAvailable = 3, NewPrice = 100 };

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(basket.Id))
                           .ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByIdAsync(productId))
                           .ReturnsAsync(product);

            Func<Task> act = async () => await _paymentService.CreateOrUpdatePaymentIntentAsync(basket.Id, null);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage($"Not enough stock for product {product.NameEn}");
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntentAsync_ShouldSetShippingPrice_WhenDeliveryMethodExists()
        {
            var basket = new CustomerBasket
            {
                Id = "basket1",
                BasketItems = new List<BasketItem>(),
                PaymentIntentId = null
            };

            var deliveryMethodId = Guid.NewGuid();
            var deliveryMethod = new DeliveryMethod { Id = deliveryMethodId, Price = 50 };

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(basket.Id))
                           .ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.OrderRepository.GetDeliveryMethodByIdAsync(deliveryMethodId))
                           .ReturnsAsync(deliveryMethod);

            // Stripe call will actually execute, so in real test we should mock PaymentIntentService.
            // For demo, we just ensure the method runs and basket.PaymentIntentId assigned
            var result = await _paymentService.CreateOrUpdatePaymentIntentAsync(basket.Id, deliveryMethodId);

            result.Should().NotBeNull();
            result.PaymentIntentId.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntentAsync_ShouldHandleNullDeliveryMethod()
        {
            var basket = new CustomerBasket
            {
                Id = "basket1",
                BasketItems = new List<BasketItem>(),
                PaymentIntentId = null
            };

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(basket.Id))
                           .ReturnsAsync(basket);
            _unitOfWorkMock.Setup(u => u.OrderRepository.GetDeliveryMethodByIdAsync(It.IsAny<Guid>()))
                           .ReturnsAsync((DeliveryMethod)null);

            var result = await _paymentService.CreateOrUpdatePaymentIntentAsync(basket.Id, Guid.NewGuid());

            result.Should().NotBeNull();
            result.PaymentIntentId.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntentAsync_ShouldUpdatePaymentIntent_WhenAlreadyExists()
        {
            var basket = new CustomerBasket
            {
                Id = "basket1",
                BasketItems = new List<BasketItem>(),
                PaymentIntentId = "pi_123"
            };

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(basket.Id))
                           .ReturnsAsync(basket);

            var result = await _paymentService.CreateOrUpdatePaymentIntentAsync(basket.Id, null);

            result.PaymentIntentId.Should().Be("pi_123");
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntentAsync_ShouldUpdateBasketInRepository()
        {
            var basket = new CustomerBasket
            {
                Id = "basket1",
                BasketItems = new List<BasketItem>(),
                PaymentIntentId = null
            };

            _unitOfWorkMock.Setup(u => u.CustomerBasketRepository.GetBasketAsync(basket.Id))
                           .ReturnsAsync(basket);

            var result = await _paymentService.CreateOrUpdatePaymentIntentAsync(basket.Id, null);

            _unitOfWorkMock.Verify(u => u.CustomerBasketRepository.UpdateBasketAsync(basket), Times.Once);
        }

        #endregion

        #region UpdateOrderPaymentFailedAsync Tests

        [Fact]
        public async Task UpdateOrderFailedAsync_ShouldReturnFalse_WhenOrderNotFound()
        {
            // Arrange
            _orderRepoMock.Setup(r => r.GetOrderByPaymentIntentIdAsync(It.IsAny<string>()))
                          .ReturnsAsync((Order?)null);

            // Act
            var result = await _paymentService.UpdateOrderFailedAsync("pi_123");

            // Assert
            Assert.False(result);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderFailedAsync_ShouldUpdateOrderStatusToPaymentFailed_WhenOrderExists()
        {
            // Arrange
            var order = new Order { Id = Guid.NewGuid(), Status = Status.Pending };
            _orderRepoMock.Setup(r => r.GetOrderByPaymentIntentIdAsync("pi_123"))
                          .ReturnsAsync(order);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            var mockBackgroundJob = new Mock<BackgroundJob>();

            // Act
            var result = await _paymentService.UpdateOrderFailedAsync("pi_123");

            // Assert
            Assert.Equal(Status.PaymentFailed, order.Status);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateOrderFailedAsync_ShouldCallCompleteAsync_WhenOrderExists()
        {
            // Arrange
            var order = new Order { Id = Guid.NewGuid(), Status = Status.Pending };
            _orderRepoMock.Setup(r => r.GetOrderByPaymentIntentIdAsync("pi_123"))
                          .ReturnsAsync(order);

            // Act
            await _paymentService.UpdateOrderFailedAsync("pi_123");

            // Assert
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        #endregion

        #region UpdateOrderPaymentSucceededAsync Tests

        [Fact]
        public async Task UpdateOrderSuccessAsync_ShouldReturnFalse_WhenOrderNotFound()
        {
            // Arrange
            _orderRepoMock.Setup(r => r.GetOrderByPaymentIntentIdAsync(It.IsAny<string>()))
                          .ReturnsAsync((Order?)null);

            // Act
            var result = await _paymentService.UpdateOrderSuccessAsync("pi_123");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateOrderSuccessAsync_ShouldThrow_WhenOrderIsNotPending()
        {
            // Arrange
            var order = new Order { Id = Guid.NewGuid(), Status = Status.PaymentReceived };
            _orderRepoMock.Setup(r => r.GetOrderByPaymentIntentIdAsync("pi_123"))
                          .ReturnsAsync(order);

            // Act
            Func<Task> act = async () => await _paymentService.UpdateOrderSuccessAsync("pi_123");

            // Assert
            await act.Should()
                     .ThrowAsync<InvalidOperationException>()
                     .WithMessage("Order is not in a pending state.");
        }


        [Fact]
        public async Task UpdateOrderSuccessAsync_ShouldThrow_WhenStockNotEnough()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Status = Status.Pending,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductItemId = Guid.NewGuid(), Quantity = 5 }
                }
            };

            var product = new Domain.Entities.Product.Product
            {
                Id = order.OrderItems.First().ProductItemId,
                NameEn = "Laptop",
                QuantityAvailable = 3
            };

            _orderRepoMock
                .Setup(r => r.GetOrderByPaymentIntentIdAsync("pi_123"))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(p => p.GetByIdAsync(order.OrderItems.First().ProductItemId))
                .ReturnsAsync(product);

            // Act
            Func<Task> act = async () => await _paymentService.UpdateOrderSuccessAsync("pi_123");

            // Assert
            await act.Should()
                     .ThrowAsync<InvalidOperationException>()
                     .WithMessage("*Not enough stock for product Laptop*");
        }

        [Fact]
        public async Task UpdateOrderSuccessAsync_ShouldSkipItem_WhenProductNotFound()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Status = Status.Pending,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductItemId = Guid.NewGuid(), Quantity = 2 }
                }
            };

            _orderRepoMock
                .Setup(r => r.GetOrderByPaymentIntentIdAsync("pi_123"))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Domain.Entities.Product.Product?)null);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _paymentService.UpdateOrderSuccessAsync("pi_123");

            // Assert
            result.Should().BeTrue();
            order.Status.Should().Be(Status.PaymentReceived);

            _productRepoMock.Verify(
                p => p.UpdateAsync(It.IsAny<Domain.Entities.Product.Product>()),
                Times.Never
            );
        }


        [Fact]
        public async Task UpdateOrderSuccessAsync_ShouldDecreaseStock_WhenStockIsEnough()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Status = Status.Pending,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductItemId = productId, Quantity = 2 }
                }
            };

            var product = new Domain.Entities.Product.Product
            {
                Id = productId,
                NameEn = "Phone",
                QuantityAvailable = 5
            };

            _orderRepoMock
                .Setup(r => r.GetOrderByPaymentIntentIdAsync("pi_123"))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(p => p.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _paymentService.UpdateOrderSuccessAsync("pi_123");

            // Assert
            result.Should().BeTrue();
            product.QuantityAvailable.Should().Be(3);
            order.Status.Should().Be(Status.PaymentReceived);

            _productRepoMock.Verify(p => p.UpdateAsync(product), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }


        #endregion
    }
}
