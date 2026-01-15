using Core.Entities;

namespace Core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<IEnumerable<string>> GetTypesAsync();
    }
}