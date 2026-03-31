namespace EStoreX.Core.DTO.Products.Responses
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        // NameEn and DescriptionEn are localized values chosen at mapping time
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public string CategoryName { get; set; }
        public int QuantityAvailable { get; set; }
        public string BrandName { get; set; }
        public bool IsFeatured { get; set; }
        public IEnumerable<PhotoResponse> Photos { get; set; }

        public ProductResponse() { } 
    }
}
