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
                (string.IsNullOrWhiteSpace(specParams.Brand) || x.Brand == specParams.Brand) &&
                (string.IsNullOrWhiteSpace(specParams.Type) || x.Type == specParams.Type)
            )
        {
        }
    }
}