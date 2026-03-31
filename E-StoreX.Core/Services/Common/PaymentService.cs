using Domain.Entities.Baskets;
using EStoreX.Core.BackgroundJobs.Interfaces;
using EStoreX.Core.BackgroundJobs.Wrapper;
using EStoreX.Core.Domain.Options;
using EStoreX.Core.Enums;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.ServiceContracts.Common;
using Hangfire;
using Microsoft.Extensions.Options;
using Stripe;
using Microsoft.Extensions.Localization;
using MyProduct = Domain.Entities.Product.Product;

namespace EStoreX.Core.Services.Common
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        //private readonly IConfiguration _configuration;
        private readonly StripeSettings _stripeSettings;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly IBackgroundJobClientWrapper _backgroundJobClient;
        private readonly IStringLocalizer<PaymentService> _localizer;

public PaymentService(IUnitOfWork unitOfWork, IOptions<StripeSettings> options, PaymentIntentService paymentIntentService, IBackgroundJobClientWrapper backgroundJobClient, IStringLocalizer<PaymentService> localizer)
{
    _unitOfWork = unitOfWork;
    //_configuration = configuration;
    _stripeSettings = options.Value;
    _paymentIntentService = paymentIntentService;
    _backgroundJobClient = backgroundJobClient;
    _localizer = localizer;
}
        /// <inheritdoc/>
        public async Task<CustomerBasket> CreateOrUpdatePaymentIntentAsync(string basketId, Guid? deliveryMethodId)
        {
            var basket = await _unitOfWork.CustomerBasketRepository.GetBasketAsync(basketId);
            if (basket is null)
                throw new KeyNotFoundException(string.Format(_localizer["BasketNotFound"].Value, basketId));

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey; // _configuration["StripeSetting:SecretKey"];
            decimal shippingPrice = 0m;
            if (deliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.OrderRepository.GetDeliveryMethodByIdAsync(deliveryMethodId.Value);
                if (deliveryMethod is not null)
                    shippingPrice = deliveryMethod.Price;
            }
            decimal total = 0m;
            foreach (var item in basket.BasketItems)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(item.Id);
                if (product is null)
                    throw new KeyNotFoundException(string.Format(_localizer["ProductNotFound"].Value, item.Id));
                if(product.QuantityAvailable < item.Qunatity)
                    throw new InvalidOperationException(string.Format(_localizer["NotEnoughStock"].Value, product.NameEn));

                item.Price = product.NewPrice;
                var (unitPrice, discountAmount) = await GetDiscountedPriceAsync(product, item.Qunatity, basket);
                total += unitPrice * item.Qunatity;
            }

            //PaymentIntentService service = new PaymentIntentService();
            PaymentIntent _intent;
            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)((total + shippingPrice) * 100),
                    Currency = "USD",
                    PaymentMethodTypes = new List<string> { "card" },
                };
                _intent = await _paymentIntentService.CreateAsync(options);
                basket.PaymentIntentId = _intent.Id;
                basket.ClientSecret = _intent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = (long)((total + shippingPrice) * 100),
                };
                _intent = await _paymentIntentService.UpdateAsync(basket.PaymentIntentId, options);
            }
            await _unitOfWork.CustomerBasketRepository.UpdateBasketAsync(basket);
            return basket;
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateOrderFailedAsync(string? paymentIntentId)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderByPaymentIntentIdAsync(paymentIntentId);
            if (order is null) return false;

            order.Status = Status.PaymentFailed;
            var res = await _unitOfWork.CompleteAsync();
            if (res <= 0) return false;
            //BackgroundJob.Enqueue<IEmailJob>(job => job.SendPaymentFailedEmailAsync(order.Id, null));
            _backgroundJobClient.Enqueue<IEmailJob>(job => job.SendPaymentFailedEmailAsync(order.Id, null));

            return true;
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateOrderSuccessAsync(string? paymentIntentId)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderByPaymentIntentIdAsync(paymentIntentId);
            if (order is null) return false;

            if (order.Status != Status.Pending)
                throw new InvalidOperationException(_localizer["OrderNotPending"].Value);

            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(item.ProductItemId);
                if (product == null) continue;

                if (product.QuantityAvailable < item.Quantity)
                    throw new InvalidOperationException(string.Format(_localizer["NotEnoughStock"].Value, product.NameEn));

                product.QuantityAvailable -= item.Quantity;
                product.SalesCount += item.Quantity;
                await _unitOfWork.ProductRepository.UpdateAsync(product);
            }

            if (order.DiscountId.HasValue)
            {
                var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(order.DiscountId.Value);
                if (discount is not null)
                {
                    discount.CurrentUsageCount++;
                    await _unitOfWork.DiscountRepository.UpdateAsync(discount);
                }
            }

            order.Status = Status.PaymentReceived;
            var res = await _unitOfWork.CompleteAsync();
            if (res <= 0) return false;
            //BackgroundJob.Enqueue<IEmailJob>(job => job.SendOrderConfirmationEmailAsync(order.Id, null));
            _backgroundJobClient.Enqueue<IEmailJob>(job => job.SendOrderConfirmationEmailAsync(order.Id, null));

            return true;
        }

        /// <inheritdoc/>
        public async Task<(decimal unitPrice, decimal discountAmount)> GetDiscountedPriceAsync(MyProduct product, int quantity, CustomerBasket basket)
        {
            if (basket.DiscountId == null)
                return (product.NewPrice, 0m);

            var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(basket.DiscountId.Value);
            if (discount == null || discount.Status != DiscountStatus.Active)
                return (product.NewPrice, 0m);

            var now = DateTime.UtcNow;
            if (discount.StartDate > now || (discount.EndDate.HasValue && discount.EndDate.Value < now))
                return (product.NewPrice, 0m);

            bool applies = discount.DiscountType switch
            {
                DiscountType.Product => discount.ProductId == product.Id,
                DiscountType.Category => discount.CategoryId == product.CategoryId,
                DiscountType.Brand => discount.BrandId == product.BrandId,
                DiscountType.Global => true,
                _ => false
            };

            if (!applies)
                return (product.NewPrice, 0m);

            var discountedUnitPrice = product.NewPrice - (product.NewPrice * (discount.Percentage / 100m));
            var discountAmount = (product.NewPrice - discountedUnitPrice) * quantity;

            return (
                Math.Round(discountedUnitPrice, 2, MidpointRounding.AwayFromZero),
                Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero)
            );
        }



    }
}
