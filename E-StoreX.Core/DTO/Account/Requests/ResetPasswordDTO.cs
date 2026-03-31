using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessageResourceName = "RequiredUserId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [Display(Name = "User ID")]
        public string? UserId { get; set; }

        [Required(ErrorMessageResourceName = "RequiredToken", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? Token { get; set; }

        [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceName = "MinLengthPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessageResourceName = "PasswordsDoNotMatch", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}
