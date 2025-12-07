# Módulo 1 – Fundamentos de Microservicios

## 🧠 Teoría

### ¿Qué es un microservicio?

Un microservicio es una arquitectura de software que estructura una aplicación como una colección de servicios débilmente acoplados, cada uno implementando una funcionalidad de negocio específica.

**Características clave:**
- **Autonomía**: Cada servicio puede desarrollarse, desplegarse y escalarse independientemente
- **Desacoplamiento**: Los servicios se comunican a través de APIs bien definidas
- **Tecnología heterogénea**: Cada servicio puede usar diferentes tecnologías
- **Deployment independiente**: Cambios en un servicio no afectan a otros

### Monolito vs Microservicios

**Monolito:**
- ✅ Simplicidad inicial
- ✅ Desarrollo más rápido al inicio
- ✅ Transacciones ACID más fáciles
- ❌ Escalado limitado
- ❌ Deployment acoplado
- ❌ Tecnología única

**Microservicios:**
- ✅ Escalado independiente
- ✅ Deployment independiente
- ✅ Tecnología heterogénea
- ✅ Resiliencia mejorada
- ❌ Complejidad operacional
- ❌ Consistencia eventual
- ❌ Overhead de comunicación

### Comunicación

**Síncrona (REST, gRPC):**
- Cliente espera respuesta inmediata
- Acoplamiento temporal
- Más simple de implementar
- Puede causar cascadas de fallos

**Asíncrona (Message Queues, Event Bus):**
- Cliente no espera respuesta inmediata
- Desacoplamiento temporal
- Mayor resiliencia
- Consistencia eventual

## 🧪 Laboratorio 1 - Paso a Paso

### Objetivo
Crear un microservicio mínimo en .NET con:
- Endpoint GET básico
- Capa de dominio simple
- Repositorio en memoria

### Paso 1: Crear el proyecto

```bash
# Navegar a la carpeta de servicios
cd src/Services

# Crear nuevo proyecto Web API
dotnet new webapi -n ProductService --no-https --use-controllers

# Navegar al proyecto
cd ProductService
```

### Paso 2: Crear la estructura de carpetas

```bash
# Crear carpetas para arquitectura limpia
mkdir Domain
mkdir Infrastructure
mkdir Application
```

### Paso 3: Crear la entidad de dominio

**Archivo: `Domain/Product.cs`**

```csharp
namespace ProductService.Domain;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }

    public Product(string name, string description, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CreatedAt = DateTime.UtcNow;
    }
}
```

### Paso 4: Crear la interfaz del repositorio

**Archivo: `Domain/IProductRepository.cs`**

```csharp
namespace ProductService.Domain;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);
}
```

### Paso 5: Implementar el repositorio en memoria

**Archivo: `Infrastructure/InMemoryProductRepository.cs`**

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
}
```

### Paso 6: Crear el controlador

**Archivo: `Controllers/ProductsController.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using ProductService.Domain;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository repository,
        ILogger<ProductsController> logger)
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
}
```

### Paso 7: Configurar Program.cs

**Archivo: `Program.cs`** (reemplazar contenido)

```csharp
using ProductService.Domain;
using ProductService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Registrar repositorio
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();

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

### Paso 8: Ejecutar el servicio

```bash
# Restaurar paquetes
dotnet restore

# Ejecutar el servicio
dotnet run
```

El servicio estará disponible en: `http://localhost:5000`

### Paso 9: Probar los endpoints

**⚠️ Importante:** Verifica el puerto en `Properties/launchSettings.json`. Por defecto es `5001`.

**Opción 1: Usando curl**
```bash
# Obtener todos los productos
curl http://localhost:5001/api/products

# Obtener producto por ID (reemplazar {id} con un ID real)
curl http://localhost:5001/api/products/{id}

# Con formato JSON (si tienes jq instalado)
curl http://localhost:5001/api/products | jq
```

**Nota:** Después del Módulo 3 (Versionamiento), también estarán disponibles las rutas versionadas:
- `/api/v1/Products` (versión 1)
- `/api/v2/Products` (versión 2 con paginación)

**Opción 2: Usando navegador**
- Abrir: `http://localhost:5001/api/products`

**Opción 3: Usando Postman o Thunder Client**
- GET `http://localhost:5001/api/products`
- GET `http://localhost:5001/api/products/{id}`

**Nota:** Si el servicio está en otro puerto, verifica los logs al ejecutar `dotnet run` o revisa `Properties/launchSettings.json`.

### Paso 10: Verificar logs

Observa los logs en la consola para ver:
- "Getting all products"
- "Getting product with ID: {id}"

### ✅ Checklist de Verificación

- [ ] Proyecto creado correctamente
- [ ] Estructura de carpetas creada
- [ ] Entidad Product creada
- [ ] Interfaz IProductRepository creada
- [ ] Repositorio en memoria implementado
- [ ] Controlador con endpoints GET creado
- [ ] Program.cs configurado con DI
- [ ] Servicio ejecuta sin errores
- [ ] Endpoint GET /api/products retorna productos
- [ ] Endpoint GET /api/products/{id} retorna producto específico
- [ ] Logs aparecen en consola

### 📝 Conceptos aprendidos

1. ✅ Separación de responsabilidades (Domain, Infrastructure, Controllers)
2. ✅ Inversión de dependencias (interfaces)
3. ✅ Endpoints REST básicos
4. ✅ Logging básico
5. ✅ Dependency Injection en .NET
6. ✅ Seed de datos iniciales

### 🐛 Solución de Problemas

**Error: "Cannot find namespace"**
- Verificar que los `using` statements estén correctos
- Verificar que los namespaces coincidan

**Error: "Service not registered"**
- Verificar que `AddScoped<IProductRepository, InMemoryProductRepository>()` esté en Program.cs

**No aparecen productos**
- Verificar que SeedDataAsync se ejecute correctamente
- Agregar breakpoint o logs para debuggear

**"Connection refused" o "No devuelve nada"**
- Verificar el puerto correcto en `Properties/launchSettings.json` (por defecto es 5001, no 5000)
- Verificar que el servicio esté corriendo: `dotnet run`
- Usar la URL correcta: `http://localhost:5001/api/products` (ajusta el puerto según tu configuración)

