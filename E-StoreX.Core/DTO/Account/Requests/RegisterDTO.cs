using EStoreX.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class RegisterDTO
    {
        [Required(ErrorMessageResourceName = "RequiredUserName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? UserName { get; set; }
        [Required(ErrorMessageResourceName = "RequiredEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [Remote(action: "IsEmailAlreadyRegistered", controller: "Account", ErrorMessageResourceName = "EmailAlreadyInUse", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? Email { get; set; }
        [Required(ErrorMessageResourceName = "RequiredPhone", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? Phone { get; set; }
        [Required(ErrorMessageResourceName = "RequiredPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceName = "MinLengthPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? Password { get; set; }
        [Required(ErrorMessageResourceName = "RequiredPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceName = "PasswordsDoNotMatch", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? ConfirmPassword { get; set; }
    }
}
