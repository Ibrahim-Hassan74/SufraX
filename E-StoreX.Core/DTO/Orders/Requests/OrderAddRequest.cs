using System.ComponentModel.DataAnnotations;
namespace EStoreX.Core.DTO.Orders.Requests
{
    public class OrderAddRequest
    {
        [Required(ErrorMessageResourceName = "RequiredDeliveryMethodId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public Guid DeliveryMethodId { get; set; }

        [Required(ErrorMessageResourceName = "RequiredBasketId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(100, ErrorMessageResourceName = "MaxLengthBasketId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string BasketId { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredShippingAddress", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public ShippingAddressDTO ShippingAddress { get; set; }
    }
}