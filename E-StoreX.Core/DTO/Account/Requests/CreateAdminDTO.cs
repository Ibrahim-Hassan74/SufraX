using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class CreateAdminDTO
    {
        [Required(ErrorMessageResourceName = "RequiredUserName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [StringLength(50, MinimumLength = 3, ErrorMessageResourceName = "MinLengthDisplayName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceName = "MinLengthPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessageResourceName = "InvalidPhoneNumber", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? PhoneNumber { get; set; }
    }
}
