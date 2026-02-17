using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.Application.DTOs;

namespace ProductService.Infrastructure.Cache;

public class RedisProductCache: IProductCache
{

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisProductCache> _logger;
    private const string AllProductsKey = "products:all";
    private static readonly JsonSerializerOptions JsonOptions = new()  {
        PropertyNameCaseInsensitive = true
    };

    public RedisProductCache(IDistributedCache cache, ILogger<RedisProductCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"products:{id}";
        var cached = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(cached))
        {
            _logger.LogDebug("Cache miss for product {ProductId}", id);
            return null;
        }

        _logger.LogDebug("Cache hit for product {ProductId}", id);
        return JsonSerializer.Deserialize<ProductDto>(cached, JsonOptions);
    }

    public async Task<IEnumerable<ProductDto>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetStringAsync(AllProductsKey, cancellationToken);

        if (string.IsNullOrEmpty(cached))
        {
            _logger.LogDebug("Cache miss for all products");
            return null;
        }

        _logger.LogDebug("Cache hit for all products");
        return JsonSerializer.Deserialize<IEnumerable<ProductDto>>(cached, JsonOptions);
    }

    public async Task SetAsync(Guid id, ProductDto product, CancellationToken cancellationToken = default)
    {
        var key = $"products:{id}";
        var json = JsonSerializer.Serialize(product, JsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };

        await _cache.SetStringAsync(key, json, options, cancellationToken);
        _logger.LogDebug("Cached product {ProductId}", id);

        // Invalidar cache de todos los productos
        await RemoveAllAsync(cancellationToken);
    }

    public async Task SetAllAsync(IEnumerable<ProductDto> products, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(products, JsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };

        await _cache.SetStringAsync(AllProductsKey, json, options, cancellationToken);
        _logger.LogDebug("Cached all products");
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"products:{id}";
        await _cache.RemoveAsync(key, cancellationToken);
        _logger.LogDebug("Removed cache for product {ProductId}", id);

        // Invalidar cache de todos los productos
        await RemoveAllAsync(cancellationToken);
    }

    public async Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(AllProductsKey, cancellationToken);
        _logger.LogDebug("Removed cache for all products");
    }
}
