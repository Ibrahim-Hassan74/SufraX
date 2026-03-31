using AutoMapper;
using EStoreX.Core.Domain.Entities.Orders;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.RepositoryContracts.Orders;
using EStoreX.Core.ServiceContracts.Orders;
using EStoreX.Core.Services.Common;
using Microsoft.Extensions.Localization;

namespace EStoreX.Core.Services.Orders
{
    public class DeliveryMethodService : BaseService, IDeliveryMethodService
    {
        private readonly IDeliveryMethodRepository _repository;
        private readonly IStringLocalizer<DeliveryMethodService> _localizer;

        public DeliveryMethodService(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<DeliveryMethodService> localizer) : base(unitOfWork, mapper) 
        {
            _repository = unitOfWork.DeliveryMethodRepository;
            _localizer = localizer;
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<DeliveryMethodResponse>> GetAllAsync()
        {
            var methods = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<DeliveryMethodResponse>>(methods);
        }
        /// <inheritdoc/>
        public async Task<DeliveryMethodResponse?> GetByIdAsync(Guid id)
        {
            var method = await _repository.GetByIdAsync(id);
            return method == null ? null : _mapper.Map<DeliveryMethodResponse>(method);
        }

        /// <inheritdoc/>
        public async Task<DeliveryMethodResponse> CreateAsync(DeliveryMethodRequest request)
        {
            var entity = _mapper.Map<DeliveryMethod>(request);
            entity.Id = Guid.NewGuid();

            await _repository.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DeliveryMethodResponse>(entity);
        }

        /// <inheritdoc/>
        public async Task<DeliveryMethodResponse?> UpdateAsync(Guid id, DeliveryMethodRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(request, entity);
            await _repository.UpdateAsync(entity);

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<DeliveryMethodResponse>(entity);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            await _repository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<DeliveryMethodResponse?> GetByNameAsync(string name)
        {
            var method =  await _repository.GetByNameAsync(name);
            return method == null ? null : _mapper.Map<DeliveryMethodResponse>(method);
        }
    }
}
