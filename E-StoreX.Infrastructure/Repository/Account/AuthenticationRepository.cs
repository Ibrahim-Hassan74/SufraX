using Domain.Entities.Common;
using EStoreX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EStoreX.Core.RepositoryContracts.Account;

namespace EStoreX.Infrastructure.Repository.Account
{
    /// <inheritdoc/>
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<Address?> GetAddress(Guid userId)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(x => x.ApplicationUserId == userId);
            return address;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAddress(Guid userId, Address address)
        {
            var userAddress = await _context.Addresses.FirstOrDefaultAsync(x => x.ApplicationUserId == userId);
            if (userAddress == null)
            {
                address.ApplicationUserId = userId;
                await _context.Addresses.AddAsync(address);
            }
            else
            {
                userAddress.FirstName = address.FirstName;
                userAddress.LastName = address.LastName;
                userAddress.City = address.City;
                userAddress.Street = address.Street;
                userAddress.ZipCode = address.ZipCode;
                userAddress.State = address.State;

                //_context.Addresses.Update(userAddress);
            }
            int res = await _context.SaveChangesAsync();
            return res > 0;
        }
    }
}
