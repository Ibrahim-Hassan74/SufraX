using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class UpdateUserDTO : IValidatableObject
    {
        [Required(ErrorMessageResourceName = "RequiredUserId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string UserId { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Display NameEn is required.")]
        public string? DisplayName { get; set; } = string.Empty;

        //[Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; } = string.Empty;

        public string? CurrentPassword { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessageResourceName = "MinLengthPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessageResourceName = "PasswordsDoNotMatch", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? ConfirmNewPassword { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (string.IsNullOrEmpty(CurrentPassword))
                {
                    var localizer = validationContext.GetService(typeof(IStringLocalizer<EStoreX.Core.Resources.DTO.Account.AuthValidationMessages>)) as IStringLocalizer<EStoreX.Core.Resources.DTO.Account.AuthValidationMessages>;
                    yield return new ValidationResult(
                        localizer?["RequiredCurrentPassword"].Value ?? "Current password is required to set a new password.",
                        new[] { nameof(CurrentPassword) });
                }

                if (string.IsNullOrEmpty(ConfirmNewPassword))
                {
                    var localizer = validationContext.GetService(typeof(IStringLocalizer<EStoreX.Core.Resources.DTO.Account.AuthValidationMessages>)) as IStringLocalizer<EStoreX.Core.Resources.DTO.Account.AuthValidationMessages>;
                    yield return new ValidationResult(
                        localizer?["RequiredConfirmNewPassword"].Value ?? "Please confirm the new password.",
                        new[] { nameof(ConfirmNewPassword) });
                }
            }
        }
    }
}
