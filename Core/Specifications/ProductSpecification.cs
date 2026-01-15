using Core.Entities;

namespace Core.Specifications
{
    public class ProductSpecification : BaseSpecification<Product>
    {
        public ProductSpecification(string? brand, string? type, string? sort) : base(x =>
            (string.IsNullOrWhiteSpace(brand) || x.Brand == brand) &&
            (string.IsNullOrWhiteSpace(type) || x.Type == type)
        )
        {
            ApplySort(sort);
        }

        private void ApplySort(string? sort)
        {
            switch (sort)
            {
                case "priceAsc":
                    AddOrderBy(p => p.Price);
                    break;
                case "priceDesc":
                    AddOrderByDescending(p => p.Price);
                    break;
                case "nameAsc":
                    AddOrderBy(p => p.Name);
                    break;
                case "nameDesc":
                    AddOrderByDescending(p => p.Name);
                    break;
                case "brandAsc":
                    AddOrderBy(p => p.Brand);
                    break;
                case "brandDesc":
                    AddOrderByDescending(p => p.Brand);
                    break;
                case "typeAsc":
                    AddOrderBy(p => p.Type);
                    break;
                case "typeDesc":
                    AddOrderByDescending(p => p.Type);
                    break;
                default:
                    AddOrderBy(p => p.Name);
                    break;
            }
        }
    }
}