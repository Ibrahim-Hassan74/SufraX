using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class LoginDTO
    {
        [Required(ErrorMessageResourceName = "RequiredEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Required(ErrorMessageResourceName = "RequiredPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceName = "MinLengthPassword", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string? Password { get; set; }
        public bool RememberMe { get; set; } = false;
        //public IEnumerable<AuthenticationScheme> Schemes { get; set; } = new List<AuthenticationScheme>();
    }
}
