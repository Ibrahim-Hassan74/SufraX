using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class ResendEmailRequest
    {
        [Required(ErrorMessage = "{0} can't be blank")]
        [EmailAddress(ErrorMessage = "{0} Should be in proper email address format")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }
}
