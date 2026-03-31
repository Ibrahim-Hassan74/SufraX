using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Categories.Requests
{
    //public record UpdateCategoryDTO([Required(ErrorMessage = "{0} can't be blank")]string NameEn,
    //    [Required][StringLength(1000, ErrorMessage = "{0} must be between {1} and {2}", MinimumLength = 5)] string DescriptionEn, 
    //    [Required(ErrorMessage = "{0} can't be blank")]Guid Id);
    public class UpdateCategoryDTO
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "RequiredCategoryName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public string NameEn { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "RequiredCategoryDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        [StringLength(1000, MinimumLength = 5, ErrorMessageResourceName = "MinLengthCategoryDescription", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public string DescriptionEn { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "RequiredCategoryName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public string NameAr { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [StringLength(1000, ErrorMessage = "{0} must be between {2} and {1}", MinimumLength = 5)]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredCategoryId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Categories.CategoryValidationMessages))]
        public Guid Id { get; set; }
    }
}
