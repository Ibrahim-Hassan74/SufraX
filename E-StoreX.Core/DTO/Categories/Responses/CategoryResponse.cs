using EStoreX.Core.DTO.Products.Responses;

namespace EStoreX.Core.DTO.Categories.Responses
{
    public class CategoryResponse
    {
        public CategoryResponse(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
        public CategoryResponse()
        {
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class CategoryResponseWithPhotos
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PhotoResponse> Photos { get; set; }
    }
}
