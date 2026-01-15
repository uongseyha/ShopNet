using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(StoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            return await _context.Products
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetTypesAsync()
        {
            return await _context.Products
                .Select(p => p.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
    }
}