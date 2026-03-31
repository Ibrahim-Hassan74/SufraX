using Domain.Entities.Common;

namespace EStoreX.Core.Domain.Entities.Orders
{
    public class OrderItem : BaseEntity<Guid>
    {
        public OrderItem(decimal price, int quantity, Guid productItemId, string mainImage, string productNameEn, string productNameAr)
        {
            Price = price;
            Quantity = quantity;
            ProductItemId = productItemId;
            MainImage = mainImage;
            ProductNameEn = productNameEn;
            ProductNameAr = productNameAr;
        }
        public OrderItem() { }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid ProductItemId { get; set; }
        public string MainImage { get; set; }
        public string ProductNameEn { get; set; } = string.Empty;
        public string ProductNameAr { get; set; } = string.Empty;

    }
}