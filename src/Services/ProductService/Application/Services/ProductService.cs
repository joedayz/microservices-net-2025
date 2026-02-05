using ProductService.Application.DTOs;
using ProductService.Domain;

namespace ProductService.Application.Services;

public class ProductService: IProductService
{

    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product == null ? null : MapToDto(product);
    }


    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all products");
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product: {ProductName}", dto.Name);

        var product = new Product(dto.Name, dto.Description, dto.Price, dto.Stock);
        var createdProduct = await _repository.CreateAsync(product, cancellationToken);

        return MapToDto(createdProduct);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return false;
        }
        return await _repository.UpdateAsync(product, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            UpdatedAt = null
        };
    }
}
