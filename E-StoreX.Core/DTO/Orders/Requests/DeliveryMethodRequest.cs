using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Orders.Requests
{
    public class DeliveryMethodRequest
    {
        [Required(ErrorMessageResourceName = "RequiredName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(100, ErrorMessageResourceName = "MaxLengthName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string NameEn { get; set; } = string.Empty;

        [StringLength(500, ErrorMessageResourceName = "MaxLengthDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string DescriptionEn { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "RequiredName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(100, ErrorMessageResourceName = "MaxLengthName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(500, ErrorMessageResourceName = "MaxLengthDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string DescriptionAr { get; set; } = string.Empty;

        [Range(0, 10000, ErrorMessageResourceName = "InvalidPriceRange", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public decimal Price { get; set; }

        [Required(ErrorMessageResourceName = "RequiredDeliveryTime", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(50, ErrorMessageResourceName = "MaxLengthDeliveryTime", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string DeliveryTime { get; set; } = string.Empty;
    }
}
