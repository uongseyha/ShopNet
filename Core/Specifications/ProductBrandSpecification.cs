using Core.Entities;

namespace Core.Specifications
{
    public class ProductBrandSpecification : BaseSpecification<Product>
    {
        public ProductBrandSpecification()
        {
            ApplySelect(x => x.Brand);
            ApplyDistinct();
            AddOrderBy(x => x.Brand);
        }
    }
}