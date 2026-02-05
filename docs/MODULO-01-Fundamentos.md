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

**Linux/macOS (Bash/Zsh):**
```bash
# Navegar a la carpeta de servicios
cd src/Services

# Crear nuevo proyecto Web API
dotnet new webapi -n ProductService --no-https --use-controllers

# Navegar al proyecto
cd ProductService
```

**Windows (CMD):**
```cmd
REM Navegar a la carpeta de servicios
cd src\Services

REM Crear nuevo proyecto Web API
dotnet new webapi -n ProductService --no-https --use-controllers

REM Navegar al proyecto
cd ProductService
```

**Windows (PowerShell):**
```powershell
# Navegar a la carpeta de servicios
cd src/Services

# Crear nuevo proyecto Web API
dotnet new webapi -n ProductService --no-https --use-controllers

# Navegar al proyecto
cd ProductService
```

### Paso 2: Crear la estructura de carpetas

**Linux/macOS (Bash/Zsh):**
```bash
# Crear carpetas para arquitectura limpia
mkdir Domain
mkdir Infrastructure
mkdir Application
```

**Windows (CMD):**
```cmd
REM Crear carpetas para arquitectura limpia
mkdir Domain
mkdir Infrastructure
mkdir Application
```

**Windows (PowerShell):**
```powershell
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

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product: {ProductName}", request.Name);
        
        var product = new Product(request.Name, request.Description, request.Price, request.Stock);
        var createdProduct = await _repository.CreateAsync(product, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
    }
}

// DTO para crear productos (agregar al final del archivo)
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
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
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

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

**Linux/macOS (Bash/Zsh):**
```bash
# Restaurar paquetes
dotnet restore

# Ejecutar el servicio
dotnet run
```

**Windows (CMD):**
```cmd
REM Restaurar paquetes
dotnet restore

REM Ejecutar el servicio
dotnet run
```

**Windows (PowerShell):**
```powershell
# Restaurar paquetes
dotnet restore

# Ejecutar el servicio
dotnet run
```

El servicio estará disponible en: `http://localhost:5001` (verifica el puerto en los logs o en `Properties/launchSettings.json`)

### Paso 9: Probar los endpoints

**⚠️ Importante:** Verifica el puerto en `Properties/launchSettings.json`. Por defecto es `5001`.

**Opción 1: Usando curl**

**Linux/macOS (Bash/Zsh):**
```bash
# Obtener todos los productos
curl http://localhost:5001/api/products

# Obtener producto por ID (reemplazar {id} con un ID real)
curl http://localhost:5001/api/products/{id}

# Con formato JSON (si tienes jq instalado)
curl http://localhost:5001/api/products | jq

# Crear un nuevo producto (POST)
curl -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Tablet","description":"Android tablet","price":299.99,"stock":20}'

# Crear producto con formato JSON (si tienes jq instalado)
curl -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Monitor","description":"4K Monitor","price":499.99,"stock":15}' | jq
```

**Windows (CMD):**
```cmd
REM Obtener todos los productos
curl http://localhost:5001/api/products

REM Obtener producto por ID (reemplazar {id} con un ID real)
curl http://localhost:5001/api/products/{id}

REM Crear un nuevo producto (POST)
curl -X POST http://localhost:5001/api/products -H "Content-Type: application/json" -d "{\"name\":\"Tablet\",\"description\":\"Android tablet\",\"price\":299.99,\"stock\":20}"

REM Crear otro producto
curl -X POST http://localhost:5001/api/products -H "Content-Type: application/json" -d "{\"name\":\"Monitor\",\"description\":\"4K Monitor\",\"price\":499.99,\"stock\":15}"
```

**Windows (PowerShell):**
```powershell
# Obtener todos los productos
Invoke-RestMethod -Uri http://localhost:5001/api/products -Method Get

# O con curl (si está disponible en PowerShell 7+)
curl http://localhost:5001/api/products

# Obtener producto por ID (reemplazar {id} con un ID real)
Invoke-RestMethod -Uri http://localhost:5001/api/products/{id} -Method Get

# Crear un nuevo producto (POST) - Método 1: Invoke-RestMethod
$body = @{
    name = "Tablet"
    description = "Android tablet"
    price = 299.99
    stock = 20
} | ConvertTo-Json

Invoke-RestMethod -Uri http://localhost:5001/api/products -Method Post -Body $body -ContentType "application/json"

# Crear producto - Método 2: Invoke-WebRequest
$jsonBody = '{"name":"Monitor","description":"4K Monitor","price":499.99,"stock":15}'
Invoke-WebRequest -Uri http://localhost:5001/api/products -Method Post -Body $jsonBody -ContentType "application/json"

# Crear producto - Método 3: curl (PowerShell 7+)
curl -X POST http://localhost:5001/api/products `
  -H "Content-Type: application/json" `
  -d '{"name":"Keyboard","description":"Mechanical keyboard","price":89.99,"stock":30}'
```

**Nota:** Después del Módulo 3 (Versionamiento), también estarán disponibles las rutas versionadas:
- `/api/v1/Products` (versión 1)
- `/api/v2/Products` (versión 2 con paginación)

**Opción 2: Usando navegador**
- Abrir: `http://localhost:5001/api/products`

**Opción 3: Usando Postman o Thunder Client**
- GET `http://localhost:5001/api/products`
- GET `http://localhost:5001/api/products/{id}`
- POST `http://localhost:5001/api/products`
  - Body (JSON):
    ```json
    {
      "name": "Tablet",
      "description": "Android tablet",
      "price": 299.99,
      "stock": 20
    }
    ```

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
- [ ] Controlador con endpoints GET y POST creado
- [ ] Program.cs configurado con DI (AddSingleton)
- [ ] Servicio ejecuta sin errores
- [ ] Endpoint GET /api/products retorna productos
- [ ] Endpoint GET /api/products/{id} retorna producto específico
- [ ] Endpoint POST /api/products crea nuevos productos
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
- Verificar que `AddSingleton<IProductRepository, InMemoryProductRepository>()` esté en Program.cs
- Nota: Usamos `AddSingleton` en lugar de `AddScoped` para que los datos persistan en el repositorio en memoria

**No aparecen productos**
- Verificar que SeedDataAsync se ejecute correctamente (deberías ver logs al iniciar el servicio)
- Verificar que el repositorio esté registrado como `Singleton` (no `Scoped`) para que los datos persistan
- Agregar breakpoint o logs para debuggear
- Reiniciar el servicio y verificar los logs de seed

**"Connection refused" o "No devuelve nada"**
- Verificar el puerto correcto en `Properties/launchSettings.json` (por defecto es 5001, no 5000)
- Verificar que el servicio esté corriendo: `dotnet run`
- Usar la URL correcta: `http://localhost:5001/api/products` (ajusta el puerto según tu configuración)
- En Windows PowerShell, usar `Invoke-RestMethod` en lugar de `curl` si curl no funciona
- Verificar que no haya firewall bloqueando el puerto

