using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Discount.Request;
using EStoreX.Core.DTO.Discount.Response;
using EStoreX.Core.DTO.Discounts.Responses;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.ServiceContracts.Discount;
using EStoreX.Core.Services.Common;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Discounts
{
    public class DiscountService : BaseService, IDiscountService
    {
        private readonly IStringLocalizer<DiscountService> _localizer;
        public DiscountService(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<DiscountService> localizer) : base(unitOfWork, mapper) 
        {
            _localizer = localizer;
        }
        public async Task<ApiResponse> CreateDiscountAsync(DiscountRequest request)
        {
            if (request.EndDate.HasValue && request.EndDate <= request.StartDate)
                return ApiResponseFactory.BadRequest(_localizer["DateRangeError"].Value);

            var discount = _mapper.Map<Discount>(request);
            discount.Code = Guid.NewGuid().ToString("N")[..8].ToUpper();

            await _unitOfWork.DiscountRepository.AddAsync(discount);
            await _unitOfWork.CompleteAsync();

            var response = _mapper.Map<DiscountResponse>(discount);
            return ApiResponseFactory.Success(_localizer["DiscountCreated"].Value, response);
        }

        public async Task<ApiResponse> UpdateDiscountAsync(Guid id, DiscountRequest request)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(id);
            if (discount == null)
                return ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value);

            if (request.EndDate.HasValue && request.EndDate <= request.StartDate)
                return ApiResponseFactory.BadRequest(_localizer["DateRangeError"].Value);

            _mapper.Map(request, discount);

            await _unitOfWork.CompleteAsync();

            var response = _mapper.Map<DiscountResponse>(discount);
            return ApiResponseFactory.Success(_localizer["DiscountUpdated"].Value, response);
        }

        public async Task<ApiResponse> DeleteDiscountAsync(Guid id)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(id);
            if (discount == null)
                return ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value);

            await _unitOfWork.DiscountRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return ApiResponseFactory.Success(_localizer["DiscountDeleted"].Value);
        }

        public async Task<ApiResponse> GetDiscountByIdAsync(Guid id)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(id, x => x.Product, c => c.Category, b => b.Brand);
            if (discount == null)
                return ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value);

            if (discount.Status != DiscountStatus.Active)
                return ApiResponseFactory.BadRequest(_localizer["DiscountNotActive"].Value);

            var response = _mapper.Map<DiscountResponse>(discount);
            return ApiResponseFactory.Success(_localizer["DiscountRetrieved"].Value, response);
        }

        public async Task<ApiResponse> GetDiscountByCodeAsync(string code)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByCodeAsync(code);
            if (discount == null)
                return ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value);

            if (discount.Status != DiscountStatus.Active)
                return ApiResponseFactory.BadRequest(_localizer["DiscountNotActive"].Value);

            var response = _mapper.Map<DiscountResponse>(discount);
            return ApiResponseFactory.Success(_localizer["DiscountRetrieved"].Value, response);
        }

        public async Task<ApiResponse> GetAllDiscountsAsync()
        {
            var discounts = await _unitOfWork.DiscountRepository.GetAllAsync(x => x.Product, c => c.Category, b => b.Brand);
            var mapped = _mapper.Map<List<DiscountResponse>>(discounts);
            return ApiResponseFactory.Success(_localizer["DiscountsRetrieved"].Value, mapped);
        }

        public async Task<ApiResponse> GetActiveDiscountsAsync()
        {
            var discounts = await _unitOfWork.DiscountRepository.GetActiveDiscountsAsync();
             
            var mapped = _mapper.Map<List<DiscountResponse>>(discounts);
            return ApiResponseFactory.Success(_localizer["ActiveDiscountsRetrieved"].Value, mapped);
        }

        public async Task<ApiResponse> GetExpiredDiscountsAsync()
        {
            var discounts = await _unitOfWork.DiscountRepository.GetExpiredDiscountsAsync();
            var mapped = _mapper.Map<List<DiscountResponse>>(discounts);
            return ApiResponseFactory.Success(_localizer["ExpiredDiscountsRetrieved"].Value, mapped);
        }


        public async Task<ApiResponse> GetNotStartedDiscountsAsync()
        {
            var discounts = await _unitOfWork.DiscountRepository.GetNotStartedDiscountsAsync();
            var mapped = _mapper.Map<List<DiscountResponse>>(discounts);
            return ApiResponseFactory.Success(_localizer["UpcomingDiscountsRetrieved"].Value, mapped);
        }

        public async Task<ApiResponse> ActivateDiscountAsync(Guid id)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(id);
            if (discount == null)
                return ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value);

            discount.StartDate = DateTime.UtcNow;
            discount.EndDate = DateTime.UtcNow.AddDays(7);

            await _unitOfWork.DiscountRepository.UpdateAsync(discount);
            await _unitOfWork.CompleteAsync();

            return ApiResponseFactory.Success(_localizer["DiscountActivated"].Value);
        }

        public async Task<ApiResponse> ExpireDiscountAsync(Guid id)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(id);
            if (discount == null)
                return ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value);

            discount.EndDate = DateTime.UtcNow;

            await _unitOfWork.DiscountRepository.UpdateAsync(discount);
            await _unitOfWork.CompleteAsync();

            return ApiResponseFactory.Success(_localizer["DiscountExpired"].Value);
        }

        public async Task<ApiResponse> UpdateDiscountDatesAsync(Guid id, DateTime startDate, DateTime? endDate)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(id);
            if (discount == null)
                return ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value);

            discount.StartDate = startDate;
            discount.EndDate = endDate;

            await _unitOfWork.DiscountRepository.UpdateAsync(discount);
            await _unitOfWork.CompleteAsync();

            return ApiResponseFactory.Success(_localizer["DiscountDatesUpdated"].Value);
        }

        public async Task<ApiResponse> ApplyDiscountToProductAsync(Guid productId, string code)
        {
            var discount = await _unitOfWork.DiscountRepository.GetActiveDiscountByCodeAsync(code);

            if (discount == null)
                return ApiResponseFactory.BadRequest(_localizer["InvalidDiscountCode"].Value);

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId, x => x.Category, y => y.Brand);
            if (product == null)
                return ApiResponseFactory.NotFound(_localizer["ProductNotFound"].Value);

            if (discount.ProductId.HasValue && discount.ProductId != product.Id)
                return ApiResponseFactory.BadRequest(_localizer["DiscountNotValidProduct"].Value);

            if (discount.CategoryId.HasValue && discount.CategoryId != product.CategoryId)
                return ApiResponseFactory.BadRequest(_localizer["DiscountNotValidCategory"].Value);

            if (discount.BrandId.HasValue && discount.BrandId != product.BrandId)
                return ApiResponseFactory.BadRequest(_localizer["DiscountNotValidBrand"].Value);

            var discountedPrice = product.NewPrice - (product.NewPrice * (discount.Percentage / 100m));

            var response = new AppliedDiscountResponse
            {
                Product = product.NameEn,
                OriginalPrice = product.NewPrice,
                DiscountedPrice = discountedPrice,
                DiscountPercentage = discount.Percentage
            };

            return ApiResponseFactory.Success(_localizer["DiscountApplied"].Value, response);

        }


        public async Task<ApiResponse> ValidateDiscountCodeAsync(string code)
        {
            var discount = await _unitOfWork.DiscountRepository.GetByCodeAsync(code);

            if (discount == null)
                return ApiResponseFactory.BadRequest(_localizer["InvalidDiscountCode"].Value);

            if (discount.Status != DiscountStatus.Active)
                return ApiResponseFactory.BadRequest(_localizer["DiscountNotActive"].Value);

            return ApiResponseFactory.Success(_localizer["DiscountCodeValid"].Value);
        }

    }
}
