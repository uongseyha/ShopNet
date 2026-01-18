using Core.Entities;

namespace Core.Specifications
{
    public class ProductWithFiltersForCountSpecification : BaseSpecification<Product>
    {
        public ProductWithFiltersForCountSpecification(ProductSpecParams specParams)
            : base(x =>
                (string.IsNullOrWhiteSpace(specParams.Search) ||
                 x.Name.ToLower().Contains(specParams.Search) ||
                 x.Brand.ToLower().Contains(specParams.Search) ||
                 x.Type.ToLower().Contains(specParams.Search)) &&
                (specParams.Brands.Count == 0 || specParams.Brands.Contains(x.Brand.ToLower())) &&
                (specParams.Types.Count == 0 || specParams.Types.Contains(x.Type.ToLower()))
            )
        {
        }
    }
}