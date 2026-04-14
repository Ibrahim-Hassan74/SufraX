using Domain.Entities.Common;

namespace EStoreX.Core.DTO.Basket
{
    public class CustomerBasketDTO : BaseEntity<string>
    {
        public List<BasketItemResponse> BasketItems { get; set; } = new();
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
}
