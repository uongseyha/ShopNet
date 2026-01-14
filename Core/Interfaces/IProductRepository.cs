using Core.Entities;

namespace Core.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(string? brand = null, string? type = null, string? sort = null);
        Task<Product?> GetByIdAsync(int id);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> SaveChangesAsync();
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<IEnumerable<string>> GetTypesAsync();
    }
}