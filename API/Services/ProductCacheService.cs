using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Services;

/// <summary>
/// Caches product list, brands, types, and single product in Redis with 30-day expiry.
/// Cache-aside pattern: try cache first, on miss load from DB and populate cache.
/// </summary>
public class ProductCacheService
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromDays(30);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private const string KeyPrefixProducts = "shop:products:list:";
    private const string KeyBrands = "shop:products:brands";
    private const string KeyTypes = "shop:products:types";
    private const string KeyProductPrefix = "shop:product:id:";

    private readonly IDistributedCache _cache;
    private readonly IGenericRepository<Product> _repository;

    public ProductCacheService(
        IDistributedCache cache,
        IGenericRepository<Product> repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Pagination<Product>?> GetProductsAsync(ProductSpecParams specParams, CancellationToken ct = default)
    {
        var key = KeyPrefixProducts + BuildListKey(specParams);
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes == null || bytes.Length == 0) return null;
        return JsonSerializer.Deserialize<Pagination<Product>>(bytes, JsonOptions);
    }

    public async Task SetProductsAsync(ProductSpecParams specParams, Pagination<Product> data, CancellationToken ct = default)
    {
        var key = KeyPrefixProducts + BuildListKey(specParams);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheExpiry }, ct);
    }

    public async Task<IReadOnlyList<string>?> GetBrandsAsync(CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(KeyBrands, ct);
        if (bytes == null || bytes.Length == 0) return null;
        return JsonSerializer.Deserialize<List<string>>(bytes, JsonOptions);
    }

    public async Task SetBrandsAsync(IEnumerable<string> data, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data.ToList(), JsonOptions);
        await _cache.SetAsync(KeyBrands, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheExpiry }, ct);
    }

    public async Task<IReadOnlyList<string>?> GetTypesAsync(CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(KeyTypes, ct);
        if (bytes == null || bytes.Length == 0) return null;
        return JsonSerializer.Deserialize<List<string>>(bytes, JsonOptions);
    }

    public async Task SetTypesAsync(IEnumerable<string> data, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data.ToList(), JsonOptions);
        await _cache.SetAsync(KeyTypes, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheExpiry }, ct);
    }

    public async Task<Product?> GetProductAsync(int id, CancellationToken ct = default)
    {
        var key = KeyProductPrefix + id;
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes == null || bytes.Length == 0) return null;
        return JsonSerializer.Deserialize<Product>(bytes, JsonOptions);
    }

    public async Task SetProductAsync(Product product, CancellationToken ct = default)
    {
        var key = KeyProductPrefix + product.Id;
        var bytes = JsonSerializer.SerializeToUtf8Bytes(product, JsonOptions);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheExpiry }, ct);
    }

    public async Task RemoveProductAsync(int id, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(KeyProductPrefix + id, ct);
    }

    private static string BuildListKey(ProductSpecParams p)
    {
        var sb = new StringBuilder(128);
        sb.Append(p.PageIndex).Append('_').Append(p.PageSize);
        sb.Append('_').Append(p.Sort ?? "name");
        sb.Append('_').Append(p.Search ?? "");
        if (p.Brands.Count > 0)
        {
            foreach (var b in p.Brands.OrderBy(x => x, StringComparer.Ordinal))
                sb.Append('_').Append(b);
        }
        if (p.Types.Count > 0)
        {
            foreach (var t in p.Types.OrderBy(x => x, StringComparer.Ordinal))
                sb.Append('_').Append(t);
        }
        var input = sb.ToString();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
        return hash;
    }
}
