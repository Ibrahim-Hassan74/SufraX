using Domain.Entities.Baskets;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Basket
{
    public class BasketAddRequest
    {
        [Required(ErrorMessageResourceName = "RequiredBasketItem", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public BasketItem BasketItem { get; set; }
        [Required(ErrorMessageResourceName = "RequiredBasketId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string BasketId { get; set; } = string.Empty;
    }
}
