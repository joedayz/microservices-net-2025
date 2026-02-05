using Microsoft.AspNetCore.Mvc;
using ProductService.Domain;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController: ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductsController> _logger;


    public ProductsController(IProductRepository repository, ILogger<ProductsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products");
        var products = await _repository.GetAllAsync(cancellationToken);
        return Ok(products);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);
        var product = await _repository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            return NotFound($"Product with ID {id} not found");
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product: {ProductName}", request.Name);
        
        var product = new Product(request.Name, request.Description, request.Price, request.Stock);
        var createdProduct = await _repository.CreateAsync(product, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
    }
}

// DTO para crear productos
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
