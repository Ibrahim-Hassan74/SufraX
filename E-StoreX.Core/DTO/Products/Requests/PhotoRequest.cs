using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Products.Requests
{
    public record PhotoRequest(
        [Required(ErrorMessageResourceName = "RequiredProductName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
    [MaxLength(200, ErrorMessageResourceName = "MaxLengthImageName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
    string ImageName);

}