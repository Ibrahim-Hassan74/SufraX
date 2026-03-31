using Stripe;
using EStoreX.Core.DTO.Common;
using Microsoft.AspNetCore.Mvc;
using EStoreX.Core.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Domain.Entities.Baskets;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.Helper;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Handles payment-related operations such as creating or updating Stripe payment intents.
    /// </summary>
    /// <remarks>
    /// This controller is secured with [Authorize], meaning all endpoints require authenticated users.
    /// Typically called before placing an order to prepare or update the payment.
    /// </remarks>
    public class PaymentsController : CustomControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _signingSecret;
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentsController"/> class.
        /// Handles payment operations including creating payment intents and processing webhooks.
        /// </summary>
        /// <param name="paymentService">Service for handling payment-related business logic.</param>
        /// <param name="options">Stripe configuration options (e.g., signing secret).</param>
        /// <param name="logger">Logger instance for logging payment events and errors.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        /// <exception cref="ArgumentNullException">Thrown if Stripe signing secret is not provided in configuration.</exception>
        public PaymentsController(IPaymentService paymentService, IOptions<StripeSettings> options, ILogger<PaymentsController> logger, IStringLocalizer<SharedResource> localizer)
        {
            _paymentService = paymentService;
            _signingSecret = options.Value.SigningSecret ?? throw new ArgumentNullException(nameof(options), "Stripe signing secret cannot be null.");
            _logger = logger;
            _localizer = localizer;
        }


        /// <summary>
        /// Creates or updates a Stripe payment intent for the specified basket and delivery method.
        /// </summary>
        /// <param name="basketId">The ID of the customer's basket. Cannot be null or empty.</param>
        /// <param name="deliveryMethodId">The ID of the selected delivery method.</param>
        /// <returns>
        /// Returns the updated <see cref="PaymentIntentDTO"/> containing the PaymentIntentId and ClientSecret.
        /// </returns>
        /// <response code="200">Successfully created or updated the payment intent.</response>
        /// <response code="400">Invalid basketId or delivery method ID.</response>
        /// <response code="404">Basket not found.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PaymentIntentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaymentIntentDTO>> CreateOrUpdatePaymentIntent(string basketId, Guid deliveryMethodId)
        {
            if (string.IsNullOrEmpty(basketId))
            {
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["BasketIdRequired"].Value));
            }
            if (deliveryMethodId == Guid.Empty)
            {
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidDeliveryMethodId"].Value));
            }
            var basket = await _paymentService.CreateOrUpdatePaymentIntentAsync(basketId, deliveryMethodId);
            if (basket == null)
            {
                return NotFound(ApiResponseFactory.NotFound(_localizer["BasketNotFound"].Value));
            }
            var paymentIntentDto = new PaymentIntentDTO
            {
                PaymentIntentId = basket.PaymentIntentId,
                ClientSecret = basket.ClientSecret,
            };
            return Ok(paymentIntentDto);
        }

        /// <summary>
        /// Handles incoming Stripe webhook events to update the order status based on the payment result.
        /// </summary>
        /// <remarks>
        /// This endpoint is called by Stripe to notify about payment intent status changes such as success or failure.
        /// It validates the Stripe signature and updates the related order accordingly.
        ///
        /// Expected events:
        /// <list type="bullet">
        ///   <item>
        ///     <term>payment_intent.succeeded</term>
        ///     <description>Updates the order status to PaymentReceived.</description>
        ///   </item>
        ///   <item>
        ///     <term>payment_intent.payment_failed</term>
        ///     <description>Updates the order status to PaymentFailed.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// <see cref="OkResult"/> if the webhook is processed successfully.
        /// <see cref="BadRequestResult"/> if validation fails.
        /// </returns>
        /// <response code="200">Webhook processed successfully.</response>
        /// <response code="400">Invalid Stripe signature or processing error.</response>
        [HttpPost("webhook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatusWithStripe()

        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _signingSecret,
                    throwOnApiVersionMismatch: false
                );

                if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    var intent = stripeEvent.Data.Object as PaymentIntent;
                    var result = await _paymentService.UpdateOrderFailedAsync(intent?.Id);

                    if (!result)
                    {
                        _logger.LogWarning("Failed to update order as PaymentFailed for PaymentIntent: {PaymentIntentId}", intent?.Id);
                    }
                }
                else if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var intent = stripeEvent.Data.Object as PaymentIntent;
                    var result = await _paymentService.UpdateOrderSuccessAsync(intent?.Id);

                    if (!result)
                    {
                        _logger.LogWarning("Failed to update order as PaymentReceived for PaymentIntent: {PaymentIntentId}", intent?.Id);
                    }
                }
                else
                {
                    Console.WriteLine($"Unhandled Stripe event type: {stripeEvent.Type}");
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook error.");
                return BadRequest(ApiResponseFactory.BadRequest());
            }
        }

    }
}
