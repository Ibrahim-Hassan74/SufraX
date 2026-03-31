using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Products.Requests
{
    public class ProductAddRequest
    {
        [Required(ErrorMessageResourceName = "RequiredProductName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        [MaxLength(100, ErrorMessageResourceName = "MaxLengthProductName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public string NameEn { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "RequiredProductDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        [StringLength(1000, MinimumLength = 5, ErrorMessageResourceName = "MinLengthProductDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public string DescriptionEn { get; set; }
        [Required(ErrorMessageResourceName = "RequiredProductName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        [MaxLength(100, ErrorMessageResourceName = "MaxLengthProductName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public string NameAr { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "RequiredProductDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        [StringLength(1000, MinimumLength = 5, ErrorMessageResourceName = "MinLengthProductDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public string DescriptionAr { get; set; }
        [Range(0, double.MaxValue, ErrorMessageResourceName = "InvalidPriceNonNegative", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public decimal NewPrice { get; set; }
        [Range(0, double.MaxValue, ErrorMessageResourceName = "InvalidPriceNonNegative", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public decimal OldPrice { get; set; }
        [Required(ErrorMessageResourceName = "RequiredProductName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "InvalidQuantityNonNegative", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public int QuantityAvailable { get; set; }
        [Required(ErrorMessageResourceName = "RequiredBrandId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public Guid BrandId { get; set; }
        public Guid CategoryId { get; set; }
        [MinLength(1, ErrorMessageResourceName = "RequiredPhoto", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public IFormFileCollection Photos { get; set; }
    }

}
