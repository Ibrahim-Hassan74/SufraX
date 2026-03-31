using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.Domain.Entities.Favourites;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Favourites;
using EStoreX.Core.ServiceContracts.Favourites;
using EStoreX.Core.Services.Common;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Favourites
{
    public class FavouriteService : BaseService, IFavouriteService
    {
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly IStringLocalizer<FavouriteService> _localizer;

        public FavouriteService(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<FavouriteService> localizer) : base(unitOfWork, mapper)
        {
            _favouriteRepository = _unitOfWork.FavouriteRepository;
            _localizer = localizer;
        }
        /// <inheritdoc/>
        public async Task<ApiResponse> AddToFavouriteAsync(Guid userId, Guid productId)
        {
            var favourite = new Favourite
            {
                UserId = userId,
                ProductId = productId
            };

            if (await _favouriteRepository.IsFavouriteAsync(favourite))
                return ApiResponseFactory.Conflict(_localizer["ProductAlreadyInFavourites"].Value);

            await _favouriteRepository.AddToFavouriteAsync(favourite);
            return ApiResponseFactory.Success(_localizer["ProductAddedToFavourites"].Value);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> RemoveFromFavouriteAsync(Guid userId, Guid productId)
        {
            var favourite = new Favourite
            {
                UserId = userId,
                ProductId = productId
            };

            if (!await _favouriteRepository.IsFavouriteAsync(favourite))
                return ApiResponseFactory.NotFound(_localizer["ProductNotFoundInFavourites"].Value);

            var removed = await _favouriteRepository.RemoveFromFavouriteAsync(favourite);

            if (!removed)
                return ApiResponseFactory.InternalServerError(_localizer["FailedToRemoveFromFavourites"].Value);

            return ApiResponseFactory.Success(_localizer["ProductRemovedFromFavourites"].Value);
        }

        /// <inheritdoc/>
        public async Task<List<ProductResponse>> GetUserFavouritesAsync(Guid userId)
        {
            var favourites = await _favouriteRepository.GetUserFavouritesAsync(userId);
            return _mapper.Map<List<ProductResponse>>(favourites);
        }
    }
}
