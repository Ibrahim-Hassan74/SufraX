using Domain.Entities.Common;

namespace EStoreX.Core.Domain.Entities.Orders
{
    public class DeliveryMethod : BaseEntity<Guid>
    {
        public DeliveryMethod(string nameEn, string descriptionEn, string nameAr, string descriptionAr, decimal price, string deliveryTime)
        {
            NameEn = nameEn;
            DescriptionEn = descriptionEn;
            NameAr = nameAr;
            DescriptionAr = descriptionAr;
            Price = price;
            DeliveryTime = deliveryTime;
        }

        public DeliveryMethod() { }

        public string NameEn { get; set; }
        public string DescriptionEn { get; set; }
        public string NameAr { get; set; }
        public string DescriptionAr { get; set; }
        public decimal Price { get; set; }
        public string DeliveryTime { get; set; }
    }
}