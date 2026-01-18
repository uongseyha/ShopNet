namespace Core.Specifications
{
    public class ProductSpecParams: PagingParams
    {
        private string? _search;

        // Filtering
        private List<string> _brands = [];
        public List<string> Brands
        {
            get => _brands;
            set
            {
                _brands = value.SelectMany(b => b.Split(',',
                    StringSplitOptions.RemoveEmptyEntries)).ToList();
            }
        }

        private List<string> _types = [];
        public List<string> Types
        {
            get => _types;
            set
            {
                _types = value.SelectMany(b => b.Split(',',
                    StringSplitOptions.RemoveEmptyEntries)).ToList();
            }
        }
        
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