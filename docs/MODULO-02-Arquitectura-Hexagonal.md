# Módulo 2 – Principios y patrones de diseño

## 🧠 Teoría

### Domain-Driven Design (DDD)

DDD es un enfoque para desarrollar software complejo conectándolo profundamente con un modelo en evolución del negocio.

**Conceptos clave:**
- **Entidades**: Objetos con identidad única (Product)
- **Value Objects**: Objetos sin identidad (Money, Address)
- **Aggregates**: Grupo de entidades que se tratan como una unidad
- **Bounded Context**: Límite explícito donde un modelo de dominio aplica
- **Ubiquitous Language**: Lenguaje común entre desarrolladores y expertos de dominio

### Bounded Context

Cada microservicio representa un Bounded Context:
- **ProductService**: Contexto de catálogo de productos
- **OrderService**: Contexto de órdenes y pedidos
- **UserService**: Contexto de usuarios y autenticación

### Hexagonal Architecture (Ports & Adapters)

La arquitectura hexagonal separa la lógica de negocio de los detalles técnicos:

```
┌─────────────────────────────────────┐
│         Application Layer           │
│  (Use Cases, Services, DTOs)        │
└─────────────────────────────────────┘
              ↕
┌─────────────────────────────────────┐
│          Domain Layer               │
│  (Entities, Value Objects,          │
│   Domain Services, Repositories)    │
└─────────────────────────────────────┘
              ↕
┌─────────────────────────────────────┐
│       Infrastructure Layer          │
│  (Database, External APIs,          │
│   Message Queues, etc.)             │
└─────────────────────────────────────┘
```

**Beneficios:**
- Desacoplamiento de frameworks
- Testabilidad mejorada
- Independencia de tecnologías externas
- Facilita cambios en infraestructura

### CQRS (Command Query Responsibility Segregation)

Separa operaciones de lectura (Queries) de escritura (Commands):
- **Commands**: Modifican estado, retornan éxito/fallo
- **Queries**: Solo leen datos, no modifican estado

### Event Sourcing

Almacena todos los cambios como una secuencia de eventos:
- Estado actual = Aplicación de eventos
- Permite reconstruir estado histórico
- Útil para auditoría y debugging

### API Composition

Combinar datos de múltiples microservicios:
- API Gateway agrega datos de varios servicios
- Patrón BFF (Backend for Frontend)

### Saga Pattern

Gestiona transacciones distribuidas:
- Cada paso tiene compensación
- Orquestación vs Coreografía

## 🧪 Laboratorio 2 - Paso a Paso

### Objetivo
Refactorizar el microservicio para implementar arquitectura hexagonal:
- Separar Domain, Application, Infrastructure
- Implementar DTOs
- Usar servicios de aplicación (casos de uso)

### Paso 1: Crear estructura de carpetas

```bash
# Ya deberías tener estas carpetas del Lab 1
# Si no, créalas:
mkdir -p Application/DTOs
mkdir -p Application/Services
```

### Paso 2: Crear DTOs

**Archivo: `Application/DTOs/ProductDto.cs`**

```csharp
namespace ProductService.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Archivo: `Application/DTOs/CreateProductDto.cs`**

```csharp
namespace ProductService.Application.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

### Paso 3: Crear interfaz del servicio de aplicación

**Archivo: `Application/Services/IProductService.cs`**

```csharp
using ProductService.Application.DTOs;

namespace ProductService.Application.Services;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

### Paso 4: Implementar el servicio de aplicación

**Archivo: `Application/Services/ProductService.cs`**

```csharp
using ProductService.Application.DTOs;
using ProductService.Domain;

namespace ProductService.Application.Services;

public class ProductService : IProductService
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

        // Actualizar propiedades (necesitarías métodos en el dominio)
        // Por ahora, simplificado
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
```

### Paso 5: Actualizar el repositorio (agregar métodos faltantes)

**Archivo: `Domain/IProductRepository.cs`** (actualizar)

```csharp
namespace ProductService.Domain;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**Archivo: `Infrastructure/InMemoryProductRepository.cs`** (actualizar)

```csharp
using ProductService.Domain;

namespace ProductService.Infrastructure;

public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Product>>(_products.Values);
    }

    public Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (!_products.ContainsKey(product.Id))
            return Task.FromResult(false);

        _products[product.Id] = product;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_products.Remove(id));
    }
}
```

### Paso 6: Refactorizar el controlador

**Archivo: `Controllers/ProductsController.cs`** (reemplazar)

```csharp
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found");
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _productService.UpdateAsync(id, dto, cancellationToken);
        if (!updated)
        {
            return NotFound($"Product with ID {id} not found");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound($"Product with ID {id} not found");
        }

        return NoContent();
    }
}
```

### Paso 7: Actualizar Program.cs

**Archivo: `Program.cs`** (actualizar)

```csharp
using ProductService.Application.Services;
using ProductService.Domain;
using ProductService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Registrar repositorio
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();

// Registrar servicio de aplicación
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

// Seed inicial de datos
using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
    await SeedDataAsync(repository);
}

app.Run();

static async Task SeedDataAsync(IProductRepository repository)
{
    var products = new[]
    {
        new Product("Laptop", "High-performance laptop", 1299.99m, 10),
        new Product("Mouse", "Wireless mouse", 29.99m, 50),
        new Product("Keyboard", "Mechanical keyboard", 89.99m, 30)
    };

    foreach (var product in products)
    {
        await repository.CreateAsync(product);
    }
}
```

### Paso 8: Compilar y ejecutar

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run
```

### Paso 9: Probar los endpoints

**⚠️ Importante:** Verifica el puerto en `Properties/launchSettings.json`. Por defecto es `5001`.

```bash
# GET todos los productos
curl http://localhost:5001/api/products | jq

# GET producto por ID
curl http://localhost:5001/api/products/{id} | jq

# POST crear producto
curl -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Tablet","description":"Android tablet","price":299.99,"stock":20}' | jq

# PUT actualizar producto
curl -X PUT http://localhost:5001/api/products/{id} \
  -H "Content-Type: application/json" \
  -d '{"name":"Tablet Pro","description":"Android tablet Pro","price":399.99,"stock":15}'

# DELETE eliminar producto
curl -X DELETE http://localhost:5001/api/products/{id}
```

**Nota:** Ajusta el puerto según tu configuración. Si usas `jq`, los resultados se formatearán mejor.

### ✅ Checklist de Verificación

- [ ] DTOs creados (ProductDto, CreateProductDto)
- [ ] Interfaz IProductService creada
- [ ] Servicio de aplicación implementado
- [ ] Controlador refactorizado para usar IProductService
- [ ] Program.cs actualizado con registros DI
- [ ] Proyecto compila sin errores
- [ ] Endpoints GET funcionan
- [ ] Endpoint POST crea productos
- [ ] Endpoint PUT actualiza productos
- [ ] Endpoint DELETE elimina productos
- [ ] DTOs se usan en lugar de entidades directamente

### 📊 Estructura Final

```
ProductService/
├── Domain/                          # Capa de dominio
│   ├── Product.cs                   # Entidad de dominio
│   └── IProductRepository.cs        # Puerto (interfaz)
├── Application/                     # Capa de aplicación
│   ├── DTOs/                        # Data Transfer Objects
│   │   ├── ProductDto.cs
│   │   └── CreateProductDto.cs
│   └── Services/                    # Casos de uso
│       ├── IProductService.cs
│       └── ProductService.cs
├── Infrastructure/                  # Capa de infraestructura
│   └── InMemoryProductRepository.cs # Adaptador (implementación)
└── Controllers/                     # Capa de presentación
    └── ProductsController.cs
```

### 🔄 Flujo de Datos

1. **Controller** recibe HTTP request con DTO
2. **Application Service** (caso de uso) procesa la lógica
3. **Domain Entity** contiene reglas de negocio
4. **Repository** (infraestructura) persiste datos
5. **DTO** se retorna al cliente (nunca la entidad directamente)

### 💡 Beneficios Obtenidos

✅ Separación clara de responsabilidades
✅ Domain independiente de infraestructura
✅ DTOs protegen el dominio
✅ Fácil de testear (mocks de interfaces)
✅ Fácil cambiar implementación (ej: de memoria a BD)

### 🐛 Solución de Problemas

**Error: "IProductService not registered"**
- Verificar que `AddScoped<IProductService, ProductService>()` esté en Program.cs

**Error: "Cannot convert Product to ProductDto"**
- Verificar que el método `MapToDto` esté implementado correctamente

**DTOs no se validan**
- Agregar Data Annotations o FluentValidation (módulo siguiente)

