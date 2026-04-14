using Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Baskets
{
    public class BasketItem : BaseEntity<Guid>
    {
        [Required(ErrorMessageResourceName = "RequiredBasketName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        [StringLength(500, ErrorMessageResourceName = "MaxLengthBasketName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string NameEn { get; set; }

        [StringLength(2000, ErrorMessageResourceName = "MaxLengthBasketDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string DescriptionEn { get; set; }
        [Required(ErrorMessageResourceName = "RequiredBasketName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        [StringLength(500, ErrorMessageResourceName = "MaxLengthBasketName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string NameAr { get; set; }

        [StringLength(2000, ErrorMessageResourceName = "MaxLengthBasketDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string DescriptionAr { get; set; }

        [Range(1, int.MaxValue, ErrorMessageResourceName = "InvalidQuantityMin", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public int Qunatity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessageResourceName = "InvalidPriceMin", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public decimal Price { get; set; }

        [Required(ErrorMessageResourceName = "RequiredCategory", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string Category { get; set; }

        [Url(ErrorMessageResourceName = "InvalidImageUrl", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string Image { get; set; }
    }
}
