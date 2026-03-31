using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Categories.Requests
{
    //public record CategoryRequest([Required(ErrorMessage = "{0} can't be blank")] string NameEn,
        //[Required][StringLength(1000, ErrorMessage = "{0} must be between {1} and {2}", MinimumLength = 5)]string DescriptionEn);
    public class CategoryRequest
    {
        [Required(ErrorMessageResourceName = "RequiredCategoryName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public string NameEn { get; set; }
        [Required(ErrorMessageResourceName = "RequiredCategoryDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        [StringLength(1000, MinimumLength = 5, ErrorMessageResourceName = "MinLengthCategoryDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public string DescriptionEn { get; set; }
        [Required(ErrorMessageResourceName = "RequiredCategoryName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public string NameAr { get; set; }
        [Required(ErrorMessageResourceName = "RequiredCategoryDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        [StringLength(1000, MinimumLength = 5, ErrorMessageResourceName = "MinLengthCategoryDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public string DescriptionAr { get; set; }
    }
}
