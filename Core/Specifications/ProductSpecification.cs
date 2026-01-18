using Core.Entities;

namespace Core.Specifications
{
    public class ProductSpecification : BaseSpecification<Product>
    {
        public ProductSpecification(ProductSpecParams specParams) 
            : base(x =>
                (string.IsNullOrWhiteSpace(specParams.Search) || 
                 x.Name.ToLower().Contains(specParams.Search) ||
                 x.Brand.ToLower().Contains(specParams.Search) ||
                 x.Type.ToLower().Contains(specParams.Search)) &&
                (specParams.Brands.Count == 0 || specParams.Brands.Contains(x.Brand.ToLower())) &&
                (specParams.Types.Count == 0 || specParams.Types.Contains(x.Type.ToLower()))
            )
        {
            ApplySort(specParams.Sort);
            ApplyPaging(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);
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