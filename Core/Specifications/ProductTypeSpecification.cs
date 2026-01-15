using Core.Entities;

namespace Core.Specifications
{
    public class ProductTypeSpecification : BaseSpecification<Product>
    {
        public ProductTypeSpecification()
        {
            ApplySelect(x => x.Type);
            ApplyDistinct();
            AddOrderBy(x => x.Type);
        }
    }
}