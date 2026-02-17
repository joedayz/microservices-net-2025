# Módulo 5 – Performance y consultas

## 🧠 Teoría

### Índices

Los índices mejoran el rendimiento de consultas:
- **Índices primarios**: Automáticos en claves primarias
- **Índices secundarios**: En columnas frecuentemente consultadas
- **Índices compuestos**: Múltiples columnas
- **Trade-off**: Mejor lectura, peor escritura

### Plan de ejecución

Analizar planes de ejecución para optimizar:
- `EXPLAIN ANALYZE` en PostgreSQL
- Identificar table scans costosos
- Optimizar queries lentas

### Paginación eficiente

**Offset pagination:**
```sql
SELECT * FROM products ORDER BY id LIMIT 10 OFFSET 20;
```
- Simple pero lento con offsets grandes

**Cursor pagination:**
```sql
SELECT * FROM products WHERE id > last_id ORDER BY id LIMIT 10;
```
- Más eficiente para grandes datasets

### Cache distribuido (Redis)

**Redis** es un almacén de datos en memoria:
- Muy rápido (sub-milisegundo)
- Estructuras de datos avanzadas
- Persistencia opcional
- Escalado horizontal

**Patrones de cache:**
- **Cache-Aside**: Aplicación maneja cache
- **Write-Through**: Escribir en cache y BD simultáneamente
- **Write-Behind**: Escribir en cache, luego en BD
- **Refresh-Ahead**: Pre-cargar datos antes de expirar

## 🧪 Laboratorio 5 - Paso a Paso

### Objetivo
Añadir Redis caching a operaciones GET:
- Cache de productos individuales
- Cache de lista de productos
- Invalidación automática en escrituras

### Paso 1: Iniciar Redis con Docker o Podman

**Con Docker:**
```bash
# Desde la raíz del proyecto
docker-compose up -d redis

# Verificar que está corriendo
docker ps

# Probar conexión (opcional)
docker exec -it microservices-redis redis-cli ping
# Debe responder: PONG
```

**Con Podman:**
```bash
# Desde la raíz del proyecto
podman compose up -d redis

# Verificar que está corriendo
podman ps

# Probar conexión (opcional)
podman exec -it microservices-redis redis-cli ping
# Debe responder: PONG
```

### Paso 2: Agregar Paquete NuGet

**Linux/macOS/Windows:**
```bash
# Desde la carpeta ProductService
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

O editar `ProductService.csproj`:

```xml
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.0" />
```

### Paso 3: Configurar Connection String

**Archivo: `appsettings.json`** (agregar)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=microservices_db;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  }
}
```

### Paso 4: Crear Interfaz de Cache

**Archivo: `Infrastructure/Cache/IProductCache.cs`**

```csharp
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
```

### Paso 5: Implementar Redis Cache

**Archivo: `Infrastructure/Cache/RedisProductCache.cs`**

```csharp
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.Application.DTOs;

namespace ProductService.Infrastructure.Cache;

public class RedisProductCache : IProductCache
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisProductCache> _logger;
    private const string AllProductsKey = "products:all";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisProductCache(
        IDistributedCache cache,
        ILogger<RedisProductCache> logger)
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
```

### Paso 6: Crear Fallback en Memoria

**Archivo: `Infrastructure/Cache/InMemoryProductCache.cs`**

```csharp
using Microsoft.Extensions.Caching.Memory;
using ProductService.Application.DTOs;

namespace ProductService.Infrastructure.Cache;

public class InMemoryProductCache : IProductCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryProductCache> _logger;
    private const string AllProductsKey = "products:all";

    public InMemoryProductCache(
        IMemoryCache cache,
        ILogger<InMemoryProductCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"products:{id}";
        _cache.TryGetValue(key, out ProductDto? product);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<ProductDto>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(AllProductsKey, out IEnumerable<ProductDto>? products);
        return Task.FromResult(products);
    }

    public Task SetAsync(Guid id, ProductDto product, CancellationToken cancellationToken = default)
    {
        var key = $"products:{id}";
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };
        _cache.Set(key, product, options);
        _cache.Remove(AllProductsKey);
        return Task.CompletedTask;
    }

    public Task SetAllAsync(IEnumerable<ProductDto> products, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };
        _cache.Set(AllProductsKey, products, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"products:{id}";
        _cache.Remove(key);
        _cache.Remove(AllProductsKey);
        return Task.CompletedTask;
    }

    public Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        _cache.Remove(AllProductsKey);
        return Task.CompletedTask;
    }
}
```

### Paso 7: Actualizar Servicio de Aplicación

**Archivo: `Application/Services/ProductService.cs`** (actualizar métodos GetByIdAsync y GetAllAsync)

```csharp
using ProductService.Application.DTOs;
using ProductService.Domain;
using ProductService.Infrastructure.Cache;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IProductCache _cache;  // Agregar
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IProductCache cache,  // Agregar
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _cache = cache;  // Agregar
        _logger = logger;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);
        
        // Intentar obtener del cache
        var cached = await _cache.GetAsync(id, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Si no está en cache, obtener de BD
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return null;
        }

        var dto = MapToDto(product);
        
        // Guardar en cache
        await _cache.SetAsync(id, dto, cancellationToken);
        
        return dto;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all products");
        
        // Intentar obtener del cache
        var cached = await _cache.GetAllAsync(cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Si no está en cache, obtener de BD
        var products = await _repository.GetAllAsync(cancellationToken);
        var dtos = products.Select(MapToDto).ToList();
        
        // Guardar en cache
        await _cache.SetAllAsync(dtos, cancellationToken);
        
        return dtos;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product: {ProductName}", dto.Name);
        
        var product = new Product(dto.Name, dto.Description, dto.Price, dto.Stock);
        var createdProduct = await _repository.CreateAsync(product, cancellationToken);
        
        var result = MapToDto(createdProduct);
        
        // Guardar en cache e invalidar lista
        await _cache.SetAsync(createdProduct.Id, result, cancellationToken);
        
        return result;
    }

    public async Task<bool> UpdateAsync(Guid id, CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        // ... código existente ...
        
        var updated = await _repository.UpdateAsync(product, cancellationToken);
        
        if (updated)
        {
            // Invalidar cache
            await _cache.RemoveAsync(id, cancellationToken);
        }
        
        return updated;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ... código existente ...
        
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        
        if (deleted)
        {
            // Invalidar cache
            await _cache.RemoveAsync(id, cancellationToken);
        }
        
        return deleted;
    }

    // ... MapToDto ...
}
```

### Paso 8: Configurar Program.cs

**Archivo: `Program.cs`** (agregar configuración de cache)

```csharp
using ProductService.Infrastructure.Cache;

// ... código existente ...

// Register Redis Cache
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
    builder.Services.AddScoped<IProductCache, RedisProductCache>();
}
else
{
    // Fallback a cache en memoria si Redis no está disponible
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IProductCache, InMemoryProductCache>();
}

// ... resto del código ...
```

### Paso 9: Compilar y Ejecutar

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run
```

### Paso 10: Probar Cache

**⚠️ Importante:** Verifica el puerto en `Properties/launchSettings.json`. Por defecto es `5001`.

**Linux/macOS (Bash/Zsh):**
```bash
# Primera llamada (cache miss - va a BD)
curl http://localhost:5001/api/v1/Products | jq
# Observar logs: "Cache MISS for all products"

# Segunda llamada (cache hit - desde Redis)
curl http://localhost:5001/api/v1/Products | jq
# Observar logs: "Cache HIT for all products"
```

**Windows (PowerShell):**
```powershell
# Primera llamada (cache miss - va a BD)
Invoke-RestMethod http://localhost:5001/api/v1/Products | ConvertTo-Json
# Observar logs: "Cache MISS for all products"

# Segunda llamada (cache hit - desde Redis)
Invoke-RestMethod http://localhost:5001/api/v1/Products | ConvertTo-Json
# Observar logs: "Cache HIT for all products"
```

**Verificar en Redis (opcional):**

**Con Docker:**
```bash
docker exec -it microservices-redis redis-cli
KEYS *
GET products:all
exit
```

**Con Podman:**
```bash
podman exec -it microservices-redis redis-cli
KEYS *
GET products:all
exit
```

### Paso 11: Verificar Invalidación

**Linux/macOS (Bash/Zsh):**
```bash
# Crear producto nuevo
curl -X POST http://localhost:5001/api/v1/Products \
  -H "Content-Type: application/json" \
  -d '{"name":"Tablet","description":"Android tablet","price":299.99,"stock":20}' | jq

# Obtener todos (debe invalidar cache y refrescar)
curl http://localhost:5001/api/v1/Products | jq
```

**Windows (CMD):**
```cmd
curl -X POST http://localhost:5001/api/v1/Products -H "Content-Type: application/json" -d "{\"name\":\"Tablet\",\"description\":\"Android tablet\",\"price\":299.99,\"stock\":20}"
```

**Windows (PowerShell):**
```powershell
# Crear producto nuevo
$body = @{
    name = "Tablet"
    description = "Android tablet"
    price = 299.99
    stock = 20
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri http://localhost:5001/api/v1/Products -ContentType "application/json" -Body $body | ConvertTo-Json

# Obtener todos (debe invalidar cache y refrescar)
Invoke-RestMethod http://localhost:5001/api/v1/Products | ConvertTo-Json
```

### ✅ Checklist de Verificación

- [ ] Redis corriendo en Docker o Podman
- [ ] Paquete StackExchangeRedis instalado
- [ ] Connection string configurado
- [ ] Interfaz IProductCache creada
- [ ] RedisProductCache implementado
- [ ] InMemoryProductCache implementado (fallback)
- [ ] ProductService actualizado para usar cache
- [ ] Program.cs configurado con Redis
- [ ] Cache funciona (ver logs de hit/miss)
- [ ] Invalidación funciona en escrituras
- [ ] Fallback a memoria funciona si Redis no está disponible

### 📊 Estrategia Cache-Aside Implementada

1. ✅ Leer del cache primero
2. ✅ Si no existe, leer de BD
3. ✅ Guardar resultado en cache
4. ✅ Invalidar cache en escrituras (Create/Update/Delete)

### 💡 Conceptos Clave

**DistributedCacheEntryOptions:**
- `AbsoluteExpirationRelativeToNow`: Tiempo absoluto de expiración
- `SlidingExpiration`: Expiración deslizante (se renueva al acceder)

**Serialización JSON:**
- Redis almacena strings
- Serializar DTOs a JSON
- Deserializar al leer

**Invalidación:**
- Remover entradas específicas
- Invalidar listas completas
- Mantener consistencia

### 🐛 Solución de Problemas

**Error: "Cannot connect to Redis"**
- Verificar que Redis esté corriendo: `docker ps` o `podman ps`
- Verificar connection string en `appsettings.json`
- Verificar puerto (6379)
- Probar conexión: `docker exec -it microservices-redis redis-cli ping` (o `podman exec...`)

**Cache no funciona**
- Verificar logs de hit/miss en la consola
- Verificar que IProductCache esté registrado en Program.cs
- Verificar serialización JSON
- Asegurarse de que Redis esté corriendo correctamente

**Datos desactualizados**
- Verificar invalidación en escrituras (Create/Update/Delete)
- Reducir tiempo de expiración para testing
- Limpiar cache manualmente si es necesario:
  ```bash
  # Con Docker:
  docker exec -it microservices-redis redis-cli FLUSHALL
  # Con Podman:
  podman exec -it microservices-redis redis-cli FLUSHALL
  ```

