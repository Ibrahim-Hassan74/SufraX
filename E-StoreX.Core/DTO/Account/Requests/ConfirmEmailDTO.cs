using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    /// <summary>
    /// Data Transfer Object for confirming a user's email.
    /// </summary>
    public class ConfirmEmailDTO
    {
        /// <summary>
        /// The unique identifier of the user to confirm the email for.
        /// </summary>
        [Required(ErrorMessageResourceName = "RequiredUserId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The token used to confirm the user's email address.
        /// </summary>
        [Required(ErrorMessageResourceName = "RequiredToken", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [MinLength(10, ErrorMessageResourceName = "MinLengthToken", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Optional URL to redirect the user to after confirmation.
        /// </summary>
        [Url(ErrorMessageResourceName = "InvalidUrl", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? RedirectTo { get; set; }
    }
}
