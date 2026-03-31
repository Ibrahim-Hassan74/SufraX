using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class RegisterApiClientRequest
    {
        [Required(ErrorMessageResourceName = "RequiredClientName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string ClientName { get; set; } = null!;
    }
}
