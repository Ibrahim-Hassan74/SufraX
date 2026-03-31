using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Orders.Requests
{
    public class ShippingAddressDTO
    {
        public Guid Id { get; set; }
        [Required(ErrorMessageResourceName = "RequiredFirstName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(50, ErrorMessageResourceName = "MaxLengthFirstName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredLastName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(50, ErrorMessageResourceName = "MaxLengthLastName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredCity", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(100, ErrorMessageResourceName = "MaxLengthCity", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredZipCode", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [RegularExpression(@"^\d{4,10}$", ErrorMessageResourceName = "InvalidZipCode", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredStreet", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(200, ErrorMessageResourceName = "MaxLengthStreet", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredState", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        [StringLength(100, ErrorMessageResourceName = "MaxLengthState", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Orders.OrderValidationMessages))]
        public string State { get; set; } = string.Empty;
    }
}
