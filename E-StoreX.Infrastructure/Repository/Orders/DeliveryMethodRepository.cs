using EStoreX.Core.Domain.Entities.Orders;
using EStoreX.Infrastructure.Repository.Common;
using EStoreX.Core.RepositoryContracts.Orders;
using EStoreX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace EStoreX.Infrastructure.Repository.Orders
{
    public class DeliveryMethodRepository : GenericRepository<DeliveryMethod>, IDeliveryMethodRepository
    {
        private readonly ApplicationDbContext _context;
        public DeliveryMethodRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DeliveryMethod?> GetByNameAsync(string name)
        {
            return await _context.DeliveryMethods
                .FirstOrDefaultAsync(d => d.NameEn == name);
        }
    }
}
