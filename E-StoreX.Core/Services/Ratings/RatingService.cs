using AutoMapper;
using EStoreX.Core.Domain.Entities.Rating;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.DTO.Ratings.Requests;
using EStoreX.Core.DTO.Ratings.Response;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Ratings;
using EStoreX.Core.ServiceContracts.Ratings;
using EStoreX.Core.Services.Common;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Ratings
{
    public class RatingService : BaseService ,IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IStringLocalizer<RatingService> _localizer;

        public RatingService(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<RatingService> localizer) : base(unitOfWork, mapper)
        {
            _ratingRepository = _unitOfWork.RatingRepository;
            _localizer = localizer;
        }
        /// <inheritdoc/>
        public async Task<RatingResponse> AddRatingAsync(RatingAddRequest request, Guid userId)
        {
            var existing = await _ratingRepository.GetUserRatingForProductAsync(request.ProductId, userId);
            if (existing != null)
                throw new InvalidOperationException(_localizer["UserAlreadyRated"].Value);

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new KeyNotFoundException(_localizer["ProductNotFound"].Value);

            var rating = _mapper.Map<Rating>(request);
            rating.UserId = userId;

            await _ratingRepository.AddAsync(rating);
            await _unitOfWork.CompleteAsync();

            var ratingWithUser = await _ratingRepository.GetUserRatingForProductAsync(request.ProductId, userId);

            return _mapper.Map<RatingResponse>(ratingWithUser);
        }

        /// <inheritdoc/>
        public async Task<RatingResponse?> UpdateRatingAsync(Guid ratingId, RatingUpdateRequest request, Guid userId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null || rating.UserId != userId)
                return null;

            rating.Score = request.Score;
            rating.Comment = request.Comment;

            await _ratingRepository.UpdateAsync(rating);
            await _unitOfWork.CompleteAsync();

            var ratingWithUser = await _ratingRepository.GetUserRatingForProductAsync(rating.ProductId, userId);

            return _mapper.Map<RatingResponse>(ratingWithUser);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteRatingAsync(Guid ratingId, Guid userId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null || rating.UserId != userId)
                return false;

            await _ratingRepository.DeleteAsync(ratingId);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RatingResponse>> GetRatingsForProductAsync(Guid productId)
        {
            var ratings = await _ratingRepository.GetRatingsByProductAsync(productId);
            return _mapper.Map<IEnumerable<RatingResponse>>(ratings);
        }

        /// <inheritdoc/>
        public async Task<ProductRatingResponse> GetProductRatingSummaryAsync(Guid productId)
        {
            var ratings = await _ratingRepository.GetRatingsByProductAsync(productId);

            var summary = new ProductRatingResponse
            {
                ProductId = productId,
                AverageScore = ratings.Any() ? ratings.Average(r => r.Score) : 0,
                TotalRatings = ratings.Count(),
                ScoreDistribution = ratings
                    .GroupBy(r => r.Score)
                    .ToDictionary(g => g.Key, g => g.Count())
            };


            return summary;
        }
        /// <inheritdoc/>
        public async Task<RatingResponse?> GetUserRatingForProductAsync(Guid productId, Guid userId)
        {
            var rating = await _ratingRepository.GetUserRatingForProductAsync(productId, userId);
            if (rating == null)
                return null;

            return _mapper.Map<RatingResponse>(rating);
        }
        /// <inheritdoc/>
        public async Task<bool> DeleteRatingAsAdminAsync(Guid id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null)
                return false;

            await _ratingRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return true;
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<AdminRatingResponse>> GetAllRatingsForAdminAsync()
        {
            var ratings = await _ratingRepository.GetAllAsync(
                r => r.Product,
                r => r.Product.Brand,
                r => r.Product.Category,
                r  => r.Product.Photos,
                r => r.User
            );

            return _mapper.Map<IEnumerable<AdminRatingResponse>>(ratings);
        }

    }
}
