using Domain.Entities.Product;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.ServiceContracts.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Common
{
    public class EntityImageManager<TEntity> : IEntityImageManager<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IStringLocalizer<EntityImageManager<TEntity>> _localizer;

        public EntityImageManager(IUnitOfWork unitOfWork, IImageService imageService, IStringLocalizer<EntityImageManager<TEntity>> localizer)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _localizer = localizer;
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> GetImagesAsync(
            Guid entityId,
            Func<IUnitOfWork, Guid, Task<TEntity>> getEntityFunc,
            Func<TEntity, IEnumerable<Photo>> getImages)
        {
            var entity = await getEntityFunc(_unitOfWork, entityId);
            if (entity == null)
                return ApiResponseFactory.NotFound(string.Format(_localizer["EntityNotFound"].Value, typeof(TEntity).Name));

            var images = getImages(entity).Select(p => new PhotoInfo() { ImageName = p.ImageName, Id = p.Id }).ToList();
            return ApiResponseFactory.Success(_localizer["ImagesRetrievedSuccessfully"].Value, images);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> AddImagesAsync(
            Guid entityId,
            List<IFormFile> files,
            string folderName,
            Func<IUnitOfWork, Guid, Task<TEntity>> getEntityFunc,
            Action<TEntity, List<string>> assignImages)
        {
            var entity = await getEntityFunc(_unitOfWork, entityId);
            if (entity == null)
                return ApiResponseFactory.NotFound(string.Format(_localizer["EntityNotFound"].Value, typeof(TEntity).Name));
            if (files == null || files.Count == 0)
                return ApiResponseFactory.BadRequest(_localizer["NoFilesProvided"].Value);

            var formFiles = new FormFileCollection();
            foreach (var f in files) formFiles.Add(f);

            var imagePaths = await _imageService.AddImageAsync(formFiles, $"{folderName}");
            assignImages(entity, imagePaths);

            await _unitOfWork.CompleteAsync();
            return ApiResponseFactory.Success(_localizer["ImagesAddedSuccessfully"].Value);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> UpdateImagesAsync(
            Guid entityId,
            List<IFormFile> files,
            string folderName,
            Func<IUnitOfWork, Guid, Task<TEntity>> getEntityFunc,
            Func<TEntity, ICollection<Photo>> getImages,
            Action<TEntity, List<string>> assignImages)
        {
            var entity = await getEntityFunc(_unitOfWork, entityId);
            if (entity == null)
                return ApiResponseFactory.NotFound(string.Format(_localizer["EntityNotFound"].Value, typeof(TEntity).Name));
            if (files == null || files.Count == 0)
                return ApiResponseFactory.BadRequest(_localizer["NoFilesProvided"].Value);

            // delete old
            foreach (var photo in getImages(entity).ToList())
            {
                _imageService.DeleteImageAsync(photo.ImageName);
                getImages(entity).Remove(photo);
            }

            // add new
            var formFiles = new FormFileCollection();
            foreach (var f in files) formFiles.Add(f);

            var imagePaths = await _imageService.AddImageAsync(formFiles, $"{folderName}");
            assignImages(entity, imagePaths);

            await _unitOfWork.CompleteAsync();
            return ApiResponseFactory.Success(_localizer["ImagesUpdatedSuccessfully"].Value);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> DeleteImageAsync(
            Guid entityId,
            Guid photoId,
            Func<IUnitOfWork, Guid, Task<TEntity>> getEntityFunc,
            Func<TEntity, ICollection<Photo>> getImages)
        {
            var entity = await getEntityFunc(_unitOfWork, entityId);
            if (entity == null)
                return ApiResponseFactory.NotFound(string.Format(_localizer["EntityNotFound"].Value, typeof(TEntity).Name));

            var photo = getImages(entity).FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                return ApiResponseFactory.NotFound(_localizer["PhotoNotFound"].Value);

            _imageService.DeleteImageAsync(photo.ImageName);
            getImages(entity).Remove(photo);

            await _unitOfWork.CompleteAsync();
            return ApiResponseFactory.Success(_localizer["ImageDeletedSuccessfully"].Value);
        }
    }
}
