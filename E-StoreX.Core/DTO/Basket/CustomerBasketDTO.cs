using Domain.Entities.Baskets;
using Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Basket
{
    public class CustomerBasketDTO : BaseEntity<string>
    {
        public List<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
        public decimal DiscountValue { get; set; } = 0m;
        public decimal Percentage { get; set; } = 0m;
        public decimal Total { get; set; } = 0m;
        public CustomerBasketDTO()
        {

        }
        public CustomerBasketDTO(string id)
        {
            Id = id;
        }
    }
    public class BasketAddRequest
    {
        [Required(ErrorMessageResourceName = "RequiredBasketItem", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public BasketItem BasketItem { get; set; }
        [Required(ErrorMessageResourceName = "RequiredBasketId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Basket.BasketValidationMessages))]
        public string BasketId { get; set; } = string.Empty;
    }
}
