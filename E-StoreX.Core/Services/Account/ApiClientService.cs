using Domain.Entities.Common;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.RepositoryContracts.Account;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.ServiceContracts.Account;
using System.Security.Cryptography;

namespace EStoreX.Core.Services.Account
{
    public class ApiClientService : IApiClientService
    {
        private readonly IApiClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ApiClientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _clientRepository = _unitOfWork.ApiClientRepository;
        }

        /// <inheritdoc/>
        public async Task<ApiClient> CreateClientAsync(string clientName)
        {
            string apiKey;

            do
            {
                apiKey = GenerateApiKey();
            }
            while (await _clientRepository.ApiKeyExistsAsync(apiKey));

            var client = new ApiClient
            {
                Id = Guid.NewGuid(),
                ClientName = clientName,
                ApiKey = apiKey,
                IsActive = true
            };

            await _clientRepository.AddAsync(client);
            await _unitOfWork.CompleteAsync();


            return client;
        }

        /// <inheritdoc/>
        public async Task<ApiClient?> GetClientAsync(Guid clientId)
        {
            return await _clientRepository.GetByIdAsync(clientId);
        }

        /// <inheritdoc/>
        public async Task<ApiClient?> GetByApiKeyAsync(string apiKey)
            => await _clientRepository.GetByApiKeyAsync(apiKey);
        /// <inheritdoc/>
        public async Task<bool> ActiveClientAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);

            if (client == null)
                return false;

            client.IsActive = true;
            client.UpdatedAt = DateTime.UtcNow;
             
            await _clientRepository.UpdateAsync(client);
            await _unitOfWork.CompleteAsync();

            return true;
        }
        /// <inheritdoc/>
        public async Task<bool> DeActivateClientAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);

            if (client == null)
                return false;

            client.IsActive = false;
            client.UpdatedAt = DateTime.UtcNow;

            await _clientRepository.UpdateAsync(client);
            await _unitOfWork.CompleteAsync();

            return true;
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<ApiClient>> GetClientsAsync()
        {
            return await _clientRepository.GetAllAsync();
        }
        /// <inheritdoc/>
        public async Task<bool> RemoveClientAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return false;
            await _clientRepository.DeleteAsync(clientId);
            await _unitOfWork.CompleteAsync();
            return true;
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateClientAsync(Guid clientId, UpdateClientRequest request)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);

            if (client == null)
                return false;

            if (!string.IsNullOrWhiteSpace(request.ClientName))
                client.ClientName = request.ClientName;

            if(!string.IsNullOrWhiteSpace(request.OAuthCallbackUrl))
                client.OAuthCallbackUrl = request.OAuthCallbackUrl;

            if (!string.IsNullOrWhiteSpace(request.AccountActivationCallbackUrl))
                client.AccountActivationCallbackUrl = request.AccountActivationCallbackUrl;

            if (!string.IsNullOrWhiteSpace(request.PasswordResetCallbackUrl))
                client.PasswordResetCallbackUrl = request.PasswordResetCallbackUrl;

            if (!string.IsNullOrWhiteSpace(request.Email))
                client.Email = request.Email;

            client.UpdatedAt = DateTime.UtcNow;

            await _clientRepository.UpdateAsync(client);

            var res = await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<ApiClient?> RotateApiKeyAsync(Guid clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null) return null;

            string newApiKey;
            do
            {
                newApiKey = GenerateApiKey();
            }
            while (await _clientRepository.ApiKeyExistsAsync(newApiKey));

            client.ApiKey = newApiKey;
            client.UpdatedAt = DateTime.UtcNow;

            await _clientRepository.UpdateAsync(client);
            await _unitOfWork.CompleteAsync();

            return client;
        }

        /// <summary>
        /// Generates a secure random API key in Hex format.
        /// </summary>
        /// <param name="length">Number of random bytes.</param>
        /// <returns>Generated API key.</returns>
        private static string GenerateApiKey(int length = 32)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

}
