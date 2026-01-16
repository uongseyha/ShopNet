namespace Core.Specifications
{
    public class ProductSpecParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        private string? _search;

        // Pagination
        public int PageIndex { get; set; } = 1;
        
        public int PageSize 
        { 
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        // Filtering
        public string? Brand { get; set; }
        public string? Type { get; set; }

        // Sorting
        public string? Sort { get; set; }

        // Search
        public string? Search 
        { 
            get => _search;
            set => _search = value?.ToLower();
        }
    }
}