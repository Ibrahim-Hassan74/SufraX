using Domain.Entities.Product;
using EStoreX.Infrastructure.Repository.Common;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EStoreX.Infrastructure.Repository.Products
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid brandId)
        {
            return await _context.Brands.AnyAsync(b => b.Id == brandId);
        }
        /// <inheritdoc/>
        public async Task<Brand?> GetByNameAsync(string name)
        {
            return await _context.Brands.Include(b => b.Photos)
                                 .FirstOrDefaultAsync(b => b.NameEn == name);
        }
        public async Task<IEnumerable<Category>> GetCategoriesByBrandIdAsync(Guid brandId)
        {
            return await _context.CategoryBrands
                .Where(cb => cb.BrandId == brandId)
                .Select(cb => cb.Category)
                .ToListAsync();
        }
    }
}
