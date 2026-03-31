using Domain.Entities.Product;
using EStoreX.Core.Domain.IdentityEntities;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.ServiceContracts.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Common
{
    public class ImageService : IImageService
    {
        private readonly IFileProvider _fileProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ImageService> _localizer;

public ImageService(IFileProvider fileProvider, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ImageService> localizer)
{
    _fileProvider = fileProvider;
    _webHostEnvironment = webHostEnvironment;
    _localizer = localizer;
}

        public async Task<List<string>> AddImageAsync(IFormFileCollection files, string src)
        {
            var saveImageSrc = new List<string>();
            if (files == null || files.Count == 0)
            {
                throw new ArgumentException(_localizer["NoFilesProvided"].Value, nameof(files));
            }
            if (string.IsNullOrWhiteSpace(src))
            {
                throw new ArgumentException(_localizer["SourceFolderEmpty"].Value, nameof(src));
            }
            src = src.Replace(" ", "");
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "Images", src);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var imagePath = Path.Combine(path, fileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    saveImageSrc.Add(Path.Combine("Images", src, fileName));
                }
            }
            return saveImageSrc;
        }

        public async Task<string> SaveUserAvatarAsync(IFormFile file, string folder, ImageCropDto? crop)
        {
            var rootPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", folder);
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var fullPath = Path.Combine(rootPath, fileName);

            using var image = Image.Load(file.OpenReadStream());

            if (crop != null)
            {
                image.Mutate(x =>
                    x.Crop(new Rectangle(
                        crop.X,
                        crop.Y,
                        crop.Width,
                        crop.Height
                    )));
            }

            image.Mutate(x => x.Resize(512, 512));

            image.Metadata.ExifProfile = null;

            await image.SaveAsJpegAsync(fullPath);

            return Path.Combine("Images", folder, fileName)
                .Replace("\\", "/");
        }

        public bool DeleteImageAsync(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                throw new ArgumentException(_localizer["SourcePathEmpty"].Value, nameof(src));
            }

            var info = _fileProvider.GetFileInfo(src);
            if (!info.Exists) info = _fileProvider.GetFileInfo(src.Replace(" ", ""));

            if (!info.Exists)
            {
                throw new FileNotFoundException(_localizer["ImageDoesNotExist"].Value, src);
            }

            var root = info.PhysicalPath;
            if (!File.Exists(root))
            {
                return false;
            }
            File.Delete(root);

            return true;
        }

        public async Task ImportExternalAvatarAsync(ApplicationUser user, string imageUrl)
        {
            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(imageUrl);

            var fileName = "avatar.jpg";
            var stream = new MemoryStream(bytes);

            var formFile = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var folderName = user.UserName.Replace(" ", "").ToLowerInvariant();

            var imagePath = await SaveUserAvatarAsync(
                formFile,
                $"Users/{folderName}",
                crop: null
            );

            user.Photo = new Photo
            {
                ImageName = imagePath,
                UserId = user.Id
            };
        }

    }
}
