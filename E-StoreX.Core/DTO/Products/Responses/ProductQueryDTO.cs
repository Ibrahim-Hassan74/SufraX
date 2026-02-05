using Domain.Entities.Product;
using EStoreX.Core.Enums;

namespace EStoreX.Core.DTO.Products.Responses
{
    public class ProductQueryDTO
    {
        public SearchByOptions SearchBy { get; set; } = SearchByOptions.None;
        public string? SearchString { get; set; } = "";

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public Guid? CategoryId { get; set; }

        //public string SortBy { get; set; } = nameof(Product.Name);
        public ProductSortBy? SortBy { get; set; }
        public Guid? BrandId { get; set; }
        public SortOrderOptions SortOrder { get; set; } = SortOrderOptions.ASC;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
