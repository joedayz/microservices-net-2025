using ProductService.Application.DTOs;

namespace ProductService.Infrastructure.Cache;

public interface IProductCache
{
    Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>?> GetAllAsync(CancellationToken cancellationToken = default);
    Task SetAsync(Guid id, ProductDto product, CancellationToken cancellationToken = default);
    Task SetAllAsync(IEnumerable<ProductDto> products, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoveAllAsync(CancellationToken cancellationToken = default);
}
