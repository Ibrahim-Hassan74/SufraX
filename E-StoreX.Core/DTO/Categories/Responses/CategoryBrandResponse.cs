using EStoreX.Core.DTO.Brands.Response;

namespace EStoreX.Core.DTO.Categories.Responses
{
    public class CategoryBrandResponse
    {
        public CategoryResponseWithPhotos Categories { get; set; }
        public List<BrandResponse> BrandResponse { get; set; }

    }
}
