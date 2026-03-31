using EStoreX.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class UpdateUserRoleDTO
    {
        [Required(ErrorMessageResourceName = "RequiredUserId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public string UserId { get; set; } = string.Empty;
        [Required(ErrorMessageResourceName = "RequiredRole", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public UserTypeOptions Role { get; set; }
    }
}
