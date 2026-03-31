using EStoreX.Core.DTO.Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class UploadUserPhotoDto
    {
        [Required(ErrorMessageResourceName = "RequiredFile", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Account.AuthValidationMessages))]
        public IFormFile File { get; set; }
        public ImageCropDto? Crop { get; set; }
    }
}
