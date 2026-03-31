using Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities.Product
{
    public class Brand : BaseEntity<Guid>
    {
        [Required(ErrorMessage = "Brand NameEn is required")]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;
        [Required(ErrorMessage = "Brand NameEn is required")]
        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();
        [JsonIgnore]
        public ICollection<CategoryBrand> CategoryBrands { get; set; } = new List<CategoryBrand>();
        [JsonIgnore]
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}
