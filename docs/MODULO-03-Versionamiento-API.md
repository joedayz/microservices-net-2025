# Módulo 3 – Buenas prácticas de diseño

## 🧠 Teoría

### Versionamiento de API

El versionamiento permite evolucionar APIs sin romper clientes existentes.

**Estrategias comunes:**

1. **URL Versioning** (Recomendado)
   - `GET /api/v1/products`
   - `GET /api/v2/products`

2. **Header Versioning**
   - `X-Version: 2.0`
   - `Accept: application/vnd.api+json;version=2`

3. **Query String Versioning**
   - `GET /api/products?version=2`

**Mejores prácticas:**
- Versionar desde el inicio (v1)
- Documentar cambios entre versiones
- Mantener versiones anteriores por tiempo razonable
- Comunicar deprecación con anticipación

### DTOs vs Entities

**Entities (Domain):**
- Representan conceptos del negocio
- Contienen lógica de dominio
- No deben exponerse directamente en APIs

**DTOs (Data Transfer Objects):**
- Objetos planos para transferencia
- Sin lógica de negocio
- Pueden diferir de entidades
- Protegen el dominio

**Ventajas de usar DTOs:**
- Desacoplamiento de dominio
- Control de qué se expone
- Flexibilidad para cambios
- Mejor rendimiento (proyecciones)

### Idempotencia

Operaciones idempotentes pueden ejecutarse múltiples veces sin cambiar el resultado:
- `GET /products/{id}` - Siempre idempotente
- `PUT /products/{id}` - Debe ser idempotente
- `DELETE /products/{id}` - Debe ser idempotente
- `POST /products` - NO es idempotente (usa idempotency-key)

### Regla del 12-Factor

Aplicación para construir aplicaciones SaaS modernas:

1. **Codebase**: Un código base, múltiples despliegues
2. **Dependencies**: Declarar y aislar dependencias
3. **Config**: Configuración en el entorno
4. **Backing services**: Tratar servicios de respaldo como recursos adjuntos
5. **Build, release, run**: Separar etapas de construcción y ejecución
6. **Processes**: Ejecutar la aplicación como uno o más procesos sin estado
7. **Port binding**: Exportar servicios mediante vinculación de puertos
8. **Concurrency**: Escalar mediante el modelo de procesos
9. **Disposability**: Maximizar la robustez con inicio rápido y apagado elegante
10. **Dev/prod parity**: Mantener desarrollo, staging y producción lo más similares posible
11. **Logs**: Tratar logs como flujos de eventos
12. **Admin processes**: Ejecutar tareas administrativas como procesos de un solo uso

## 🧪 Laboratorio 3 - Paso a Paso

### Objetivo
Implementar versionamiento de API y mejorar Swagger:
- Versión 1.0: API básica
- Versión 2.0: API con paginación
- Swagger UI mejorado

### Paso 1: Agregar Paquetes NuGet

```bash
# Desde la carpeta ProductService
dotnet add package Asp.Versioning.Mvc
dotnet add package Asp.Versioning.Mvc.ApiExplorer
dotnet add package Swashbuckle.AspNetCore
```

O editar `ProductService.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Asp.Versioning.Mvc" Version="8.1.0" />
  <PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
</ItemGroup>
```

**⚠️ Nota importante:** No agregues `Microsoft.AspNetCore.OpenApi` ya que causa conflictos con Swashbuckle. Swashbuckle incluye sus propias dependencias de OpenAPI.

### Paso 2: Crear Estructura de Carpetas

```bash
# Crear carpetas para versiones
mkdir -p Controllers/V1
mkdir -p Controllers/V2
```

### Paso 3: Configurar API Versioning en Program.cs

**Archivo: `Program.cs`** (agregar después de `AddControllers()`)

```csharp
// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
        new Asp.Versioning.UrlSegmentApiVersionReader(),
        new Asp.Versioning.HeaderApiVersionReader("X-Version"),
        new Asp.Versioning.QueryStringApiVersionReader("version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### Paso 4: Configurar Swagger

**Archivo: `Program.cs`** (agregar después de API Versioning)

```csharp
// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

**Archivo: `Program.cs`** (actualizar pipeline)

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Product Service API v2");
        options.RoutePrefix = string.Empty; // Swagger UI en la raíz
    });
}
```

### Paso 5: Crear Controlador V1

**Archivo: `Controllers/V1/ProductsV1Controller.cs`**

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;

namespace ProductService.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id, version = "1.0" }, product);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

### Paso 6: Crear DTO para Respuesta Paginada

**Archivo: `Application/DTOs/PagedResult.cs`**

```csharp
namespace ProductService.Application.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
```

### Paso 7: Crear Controlador V2 con Paginación

**Archivo: `Controllers/V2/ProductsV2Controller.cs`**

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;

namespace ProductService.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var allProducts = await _productService.GetAllAsync(cancellationToken);
        var productsList = allProducts.ToList();
        
        var totalCount = productsList.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var pagedProducts = productsList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<ProductDto>
        {
            Items = pagedProducts,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found");
        }

        return Ok(product);
    }
}
```

### Paso 8: Eliminar Controlador Antiguo

```bash
# Eliminar el controlador sin versión (si existe)
rm Controllers/ProductsController.cs
```

### Paso 9: Compilar y Ejecutar

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run
```

### Paso 10: Probar Versionamiento

**⚠️ Importante:** Verifica el puerto en `Properties/launchSettings.json`. Por defecto es `5001`.

**Opción 1: Por URL (Recomendado)**
```bash
# Versión 1 - Todos los productos
curl http://localhost:5001/api/v1/Products

# Versión 1 - Con formato JSON
curl http://localhost:5001/api/v1/Products | jq

# Versión 2 (con paginación)
curl "http://localhost:5001/api/v2/Products?page=1&pageSize=5" | jq

# Obtener producto específico
curl http://localhost:5001/api/v1/Products/{id} | jq
```

**Opción 2: Por Header**
```bash
# Nota: Requiere que la ruta base sea /api/[controller] sin versión
curl -H "X-Version: 2.0" http://localhost:5001/api/Products
```

**Opción 3: Por Query String**
```bash
curl "http://localhost:5001/api/Products?version=2.0"
```

**Nota sobre las rutas:**
- ✅ `/api/v1/Products` - Versión explícita v1 (recomendado para producción)
- ✅ `/api/v2/Products` - Versión explícita v2 con paginación
- ✅ `/api/products` - Ruta sin versión sigue funcionando (del Módulo 1)
- El `[controller]` en la ruta se reemplaza con "Products" (sin "Controller")
- **Recomendación:** Usa rutas versionadas (`/api/v1/Products`) para mayor claridad y control
- **Nota:** El controlador sin versión (`/api/products`) se mantiene para compatibilidad con el Módulo 1

### Paso 11: Acceder a Swagger UI

1. Abrir navegador: `http://localhost:5001/swagger` o `http://localhost:5001` (según configuración)
   - **Nota:** Verifica el puerto en `Properties/launchSettings.json`
2. Verás un selector en la parte superior para elegir entre:
   - Product Service API v1
   - Product Service API v2
3. Probar endpoints desde Swagger UI
4. Swagger mostrará las rutas correctas: `/api/v1/Products` y `/api/v2/Products`

### ✅ Checklist de Verificación

- [ ] Paquetes de versionamiento instalados
- [ ] Swashbuckle instalado
- [ ] Carpetas V1 y V2 creadas
- [ ] API Versioning configurado en Program.cs
- [ ] Swagger configurado
- [ ] ProductsV1Controller creado
- [ ] ProductsV2Controller creado con paginación
- [ ] PagedResult DTO creado
- [ ] Proyecto compila sin errores
- [ ] Swagger UI muestra ambas versiones
- [ ] Endpoint v1 funciona sin paginación
- [ ] Endpoint v2 funciona con paginación
- [ ] Versionamiento por URL funciona
- [ ] Versionamiento por header funciona
- [ ] Versionamiento por query string funciona

### 📊 Estructura Final

```
Controllers/
├── V1/
│   └── ProductsV1Controller.cs    # API v1.0 (CRUD completo)
└── V2/
    └── ProductsV2Controller.cs    # API v2.0 (con paginación)

Application/DTOs/
└── PagedResult.cs                  # DTO para respuestas paginadas
```

### 💡 Conceptos Aplicados

✅ **Versionamiento por URL**: `/api/v1/products` vs `/api/v2/products`
✅ **Versionamiento por Header**: `X-Version: 2.0`
✅ **Versionamiento por Query**: `?version=2.0`
✅ **DTOs separados de Entities**: Nunca exponer entidades directamente
✅ **Documentación con Swagger**: UI interactiva
✅ **Paginación**: Mejora rendimiento con grandes datasets
✅ **Respuestas estructuradas**: Metadatos en respuestas

### 🔄 Comparación de Versiones

| Característica | v1.0 | v2.0 |
|---------------|------|------|
| Paginación | ❌ | ✅ |
| Metadatos | ❌ | ✅ |
| CRUD completo | ✅ | Parcial (GET) |
| Compatibilidad | Base | Extendida |

### 🐛 Solución de Problemas

**Error: "ApiVersion not found"**
- Verificar que los paquetes estén instalados
- Verificar que `AddApiVersioning()` esté configurado

**Error: "GetSwagger does not have an implementation"**
- Este error ocurre cuando hay conflicto entre `Microsoft.AspNetCore.OpenApi` y `Swashbuckle.AspNetCore`
- **Solución:** Remover `Microsoft.AspNetCore.OpenApi` del `.csproj` y usar solo Swashbuckle
- Ejecutar: `dotnet clean && dotnet restore`

**Swagger no muestra versiones**
- Verificar que `AddApiExplorer()` esté configurado
- Verificar que `SubstituteApiVersionInUrl = true`

**Endpoints no funcionan**
- Verificar rutas: deben incluir `v{version:apiVersion}`
- Verificar atributos `[ApiVersion("1.0")]` en controladores

**Paginación no funciona**
- Verificar que page y pageSize sean parámetros de query
- Verificar lógica de cálculo de TotalPages

**"No devuelve nada" o "Connection refused"**
- Verificar el puerto correcto: por defecto es `5001` (no 5000)
- Verificar en `Properties/launchSettings.json` el puerto configurado
- Usar la ruta completa con versión: `/api/v1/Products` (no `/api/products`)
- El nombre del controlador es "Products" con mayúscula P
- Verificar que el servicio esté corriendo: `dotnet run`

