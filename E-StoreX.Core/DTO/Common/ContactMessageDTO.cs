using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Common
{
    public class ContactMessageDTO
    {
        [Required(ErrorMessageResourceName = "RequiredName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        [StringLength(100, ErrorMessageResourceName = "MaxLengthName", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "RequiredEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        [DataType(DataType.EmailAddress)]
        [StringLength(100, ErrorMessageResourceName = "MaxLengthEmail", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        public string Email { get; set; }
        [Required(ErrorMessageResourceName = "RequiredSubject", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        [StringLength(200, ErrorMessageResourceName = "MaxLengthSubject", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        public string Subject { get; set; }
        [Required(ErrorMessageResourceName = "RequiredMessage", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        [StringLength(4000, ErrorMessageResourceName = "MaxLengthMessage", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Common.CommonValidationMessages))]
        public string Message { get; set; }
    }
}
