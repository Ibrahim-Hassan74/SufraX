using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class ResendEmailRequest
    {
        [Required(ErrorMessageResourceName = "RequiredEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }
}
