using AutoMapper;
using EStoreX.Core.Domain.Entities.Orders;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Orders;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Orders;
using EStoreX.Core.Services.Common;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Orders
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentService _paymentService;
        private readonly IStringLocalizer<OrderService> _localizer;
        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IPaymentService paymentService, IStringLocalizer<OrderService> localizer) : base(unitOfWork, mapper)
        {
            _orderRepository = _unitOfWork.OrderRepository;
            _paymentService = paymentService;
            _localizer = localizer;
        }
        /// <inheritdoc/>
        public async Task<OrderResponse> CreateOrdersAsync(OrderAddRequest order, string buyerEmail)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), _localizer["OrderRequired"].Value);
            }
            if (string.IsNullOrEmpty(buyerEmail))
            {
                throw new ArgumentException(_localizer["BuyerEmailRequired"].Value, nameof(buyerEmail));
            }

            ValidationHelper.ModelValidation(order);

            var basket = await _unitOfWork.CustomerBasketRepository.GetBasketAsync(order.BasketId);
            if (basket == null)
            {
                throw new InvalidOperationException(_localizer["BasketNotFound"].Value);
            }
            var orderItems = new List<OrderItem>();
            var subTotal = 0m;
            decimal totalDiscount = 0m;
            foreach (var item in basket.BasketItems)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(item.Id);
                if(product is null)
                    throw new InvalidOperationException(string.Format(_localizer["ProductNotFoundWithId"].Value, item.Id));

                var (unitPrice, discountAmount) = await _paymentService.GetDiscountedPriceAsync(product, item.Qunatity, basket);

                var orderItem = new OrderItem(
                    unitPrice,
                    item.Qunatity,
                    item.Id,
                    item.Image,
                    product.NameEn ?? _localizer["UnknownProduct"].Value,
                    product.NameAr ?? product.NameEn ?? _localizer["UnknownProduct"].Value
                );

                orderItems.Add(orderItem);
                subTotal += unitPrice * item.Qunatity;
                totalDiscount += discountAmount;
            }

            var deliveryMethod = await _orderRepository.GetDeliveryMethodByIdAsync(order.DeliveryMethodId);
            if (deliveryMethod == null)
                throw new InvalidOperationException(_localizer["DeliveryMethodNotFound"].Value);

            //var subTotal = orderItems.Sum(item => item.Price * item.Quantity);
            var shippingAddress = _mapper.Map<ShippingAddress>(order.ShippingAddress);

            var existingOrder = await _orderRepository.GetOrderByPaymentIntentIdAsync(basket.PaymentIntentId);

            if (existingOrder is not null)
            {
                await _orderRepository.DeleteAsync(existingOrder.Id);
                await _unitOfWork.CompleteAsync();
                await _paymentService.CreateOrUpdatePaymentIntentAsync(basket.Id, deliveryMethod.Id);
            }

            var orderEntity = new Order(
                buyerEmail,
                subTotal,
                shippingAddress,
                deliveryMethod,
                orderItems,
                basket.PaymentIntentId,
                basket.DiscountCode,
                basket.DiscountId,
                totalDiscount
            ); 

            var createdOrder = await _orderRepository.AddAsync(orderEntity);
            await _unitOfWork.CompleteAsync();

            await _unitOfWork.CustomerBasketRepository.DeleteBasketAsync(order.BasketId);

            return _mapper.Map<OrderResponse>(createdOrder);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(string buyerEmail)
        {
            if (string.IsNullOrWhiteSpace(buyerEmail))
            {
                throw new ArgumentException(_localizer["BuyerEmailRequired"].Value, nameof(buyerEmail));
            }

            var orders = await _orderRepository.GetOrdersByBuyerEmailAsync(buyerEmail);
            return _mapper.Map<IEnumerable<OrderResponse>>(orders);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeliveryMethodResponse>> GetDeliveryMethodAsync()
        {
            var deliveryMethods = await _orderRepository.GetAllDeliveryMethodsAsync();
            return _mapper.Map<IEnumerable<DeliveryMethodResponse>>(deliveryMethods);
        }

        /// <inheritdoc/>
        public async Task<OrderResponse> GetOrderByIdAsync(Guid Id, string buyerEmail)
        {
            if (Id == Guid.Empty)
            {
                throw new ArgumentException(_localizer["OrderIdRequired"].Value, nameof(Id));
            }
            if (string.IsNullOrWhiteSpace(buyerEmail))
            {
                throw new ArgumentException(_localizer["BuyerEmailRequired"].Value, nameof(buyerEmail));
            }
            var order = await _orderRepository.GetOrderByIdAsync(Id, buyerEmail);
            return _mapper.Map<OrderResponse>(order);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<OrderResponse>> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllAsync(sh => sh.ShippingAddress, oi => oi.OrderItems, dm => dm.DeliveryMethod);
            if (orders == null || !orders.Any())
            {
                return Enumerable.Empty<OrderResponse>();
            }
            return _mapper.Map<IEnumerable<OrderResponse>>(orders);
        }
        /// <inheritdoc/>
        public async Task<SalesReportResponse?> GetSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate);

            if (orders == null || !orders.Any())
                return null;

            var report = new SalesReportResponse
            {
                TotalRevenue = orders.Sum(o => o.GetTotal()),
                TotalOrders = orders.Count(),
                TotalCustomers = orders.Select(o => o.BuyerEmail).Distinct().Count(),
                TopProducts = orders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => new { oi.ProductItemId, ProductName = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? oi.ProductNameAr : oi.ProductNameEn })
                    .Select(g => new TopProductResponse
                    {
                        ProductId = g.Key.ProductItemId,
                        ProductName = g.Key.ProductName,
                        QuantitySold = g.Sum(x => x.Quantity),
                        RevenueGenerated = g.Sum(x => x.Quantity * x.Price)
                    })
                    .OrderByDescending(tp => tp.QuantitySold)
                    .Take(10)
                    .ToList()
            };

            return report;
        }

    }
}
