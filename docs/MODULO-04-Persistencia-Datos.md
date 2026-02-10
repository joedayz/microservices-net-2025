# Módulo 4 – Persistencia de datos

## 🧠 Teoría

### SQL Server / PostgreSQL

**SQL Server:**
- Base de datos relacional de Microsoft
- Excelente integración con .NET
- Soporte para JSON, Graph API
- Costo de licencia

**PostgreSQL:**
- Base de datos relacional open-source
- Muy robusta y escalable
- Soporte avanzado para JSON, arrays, tipos personalizados
- Gratuita y ampliamente usada

### MongoDB

Base de datos NoSQL orientada a documentos:
- Esquema flexible
- Escalado horizontal fácil
- Buen rendimiento para lectura
- Consistencia eventual

### Consideraciones de partición y consistencia

**Partición (Sharding):**
- Dividir datos en múltiples servidores
- Mejora rendimiento y escalabilidad
- Complejidad operacional

**Consistencia:**
- **ACID**: Transacciones atómicas (SQL)
- **BASE**: Básicamente disponible, estado suave, consistencia eventual (NoSQL)
- Microservicios: Preferir consistencia eventual

## 🧪 Laboratorio 4 - Paso a Paso

### Objetivo
Integrar PostgreSQL con Entity Framework Core:
- Configurar DbContext
- Crear migraciones
- Implementar repositorio con EF Core
- CRUD completo

### Paso 1: Instalar herramientas de EF Core

```bash
# Instalar dotnet-ef tool globalmente
dotnet tool install --global dotnet-ef

# Verificar instalación
dotnet ef --version
```

### Paso 2: Agregar paquetes NuGet

```bash
# Desde la carpeta del proyecto ProductService
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

O editar `ProductService.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
</ItemGroup>
```

### Paso 3: Iniciar PostgreSQL con Docker

```bash
# Desde la raíz del proyecto
docker-compose up -d postgres

# Verificar que está corriendo
docker ps
```

### Paso 4: Configurar Connection String

**Archivo: `appsettings.json`** (agregar)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=microservices_db;Username=postgres;Password=postgres"
  }
}
```

### Paso 5: Crear DbContext

**Archivo: `Infrastructure/ProductDbContext.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using ProductService.Domain;

namespace ProductService.Infrastructure;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);

            // Índices
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
```

### Paso 6: Crear Factory para Migraciones

**Archivo: `Infrastructure/DesignTimeDbContextFactory.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductService.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();
        
        // Connection string para desarrollo local
        var connectionString = "Host=localhost;Port=5432;Database=microservices_db;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new ProductDbContext(optionsBuilder.Options);
    }
}
```

### Paso 7: Crear Migración

```bash
# Crear migración inicial
dotnet ef migrations add InitialCreate --output-dir Infrastructure/Migrations

# Verificar que se creó la carpeta Migrations con los archivos
```

### Paso 8: Implementar Repositorio con EF Core

**Archivo: `Infrastructure/EfProductRepository.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using ProductService.Domain;

namespace ProductService.Infrastructure;

public class EfProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;
    private readonly ILogger<EfProductRepository> _logger;

    public EfProductRepository(
        ProductDbContext context,
        ILogger<EfProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created product with ID: {ProductId}", product.Id);
        return product;
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existingProduct = await _context.Products.FindAsync(new object[] { product.Id }, cancellationToken);
        if (existingProduct == null)
        {
            return false;
        }

        _context.Entry(existingProduct).CurrentValues.SetValues(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated product with ID: {ProductId}", product.Id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted product with ID: {ProductId}", id);
        return true;
    }
}
```

### Paso 9: Actualizar Program.cs

El cambio principal es reemplazar `InMemoryProductRepository` por `EfProductRepository` y agregar el `DbContext`. Se mantiene la configuración de API Versioning y Swagger del Módulo 3.

**Archivo: `Program.cs`** (actualizar)

```csharp
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProductService;
using ProductService.Application.Services;
using ProductService.Domain;
using ProductService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// API Versioning (del Módulo 3)
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Version"),
            new QueryStringApiVersionReader("version")
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// Swagger / OpenAPI (del Módulo 3)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// Entity Framework Core + PostgreSQL (NUEVO en Módulo 4)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(connectionString));

// DI - Cambiar InMemoryProductRepository por EfProductRepository
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
// builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>(); // Ya no se usa

// Register application services (Scoped porque depende del DbContext que es Scoped)
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();

var app = builder.Build();

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant()
            );
        }

        options.RoutePrefix = string.Empty; // Swagger en /
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and migrate
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

    // Aplicar migraciones automáticamente
    await dbContext.Database.MigrateAsync();

    // Seed initial data if database is empty
    if (!dbContext.Products.Any())
    {
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Seeding initial data...");
        await SeedDataAsync(repository);
        logger.LogInformation("Seed data completed successfully");
    }
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
        Console.WriteLine($"Seeded product: {product.Name} (ID: {product.Id})");
    }
}
```

**⚠️ Nota importante:** El repositorio ahora es `AddScoped` (no `AddSingleton`) porque `ProductDbContext` es Scoped por defecto en EF Core. También el `IProductService` pasa a `AddScoped` por la misma razón.

### Paso 10: Compilar y Ejecutar

```bash
# Compilar
dotnet build

# Ejecutar (las migraciones se aplicarán automáticamente)
dotnet run
```

### Paso 11: Verificar Base de Datos

```bash
# Conectar a PostgreSQL
docker exec -it microservices-postgres psql -U postgres -d microservices_db

# Ver tablas
\dt

# Ver productos
SELECT * FROM "Products";

# Salir
\q
```

### Paso 12: Probar Endpoints

**⚠️ Importante:** Verifica el puerto en `Properties/launchSettings.json`. Por defecto es `5001`.
Las rutas usan versionamiento del Módulo 3: `/api/v1/Products`.

#### Crear producto (POST)

**Linux/macOS (Bash/Zsh):**
```bash
# Crear producto (se guardará en PostgreSQL)
curl -X POST http://localhost:5001/api/v1/Products \
  -H "Content-Type: application/json" \
  -d '{"name":"Monitor","description":"4K Monitor","price":499.99,"stock":15}' | jq
```

**Windows (CMD):**
```cmd
curl -X POST http://localhost:5001/api/v1/Products -H "Content-Type: application/json" -d "{\"name\":\"Monitor\",\"description\":\"4K Monitor\",\"price\":499.99,\"stock\":15}"
```

**Windows (PowerShell):**
```powershell
$body = @{
    name = "Monitor"
    description = "4K Monitor"
    price = 499.99
    stock = 15
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri http://localhost:5001/api/v1/Products -ContentType "application/json" -Body $body | ConvertTo-Json
```

#### Obtener todos los productos (GET)

**Linux/macOS (Bash/Zsh):**
```bash
# Obtener todos (v1 - desde PostgreSQL)
curl http://localhost:5001/api/v1/Products | jq

# Obtener todos con paginación (v2)
curl "http://localhost:5001/api/v2/Products?page=1&pageSize=5" | jq
```

**Windows (CMD):**
```cmd
REM Obtener todos (v1)
curl http://localhost:5001/api/v1/Products

REM Obtener todos con paginación (v2)
curl "http://localhost:5001/api/v2/Products?page=1&pageSize=5"
```

**Windows (PowerShell):**
```powershell
# Obtener todos (v1)
Invoke-RestMethod http://localhost:5001/api/v1/Products | ConvertTo-Json

# Obtener todos con paginación (v2)
Invoke-RestMethod "http://localhost:5001/api/v2/Products?page=1&pageSize=5" | ConvertTo-Json
```

#### Verificar persistencia

```bash
# 1. Detener el servicio (Ctrl+C)
# 2. Volver a ejecutar
dotnet run

# 3. Obtener productos - los datos seed y el Monitor creado deben seguir ahí
curl http://localhost:5001/api/v1/Products | jq
```

**Resultado esperado:** Los datos persisten después de reiniciar el servicio porque ahora se guardan en PostgreSQL (no en memoria).

### ✅ Checklist de Verificación

- [ ] PostgreSQL corriendo en Docker
- [ ] Paquetes EF Core instalados
- [ ] Connection string configurado
- [ ] DbContext creado con configuración
- [ ] DesignTimeDbContextFactory creado
- [ ] Migración creada exitosamente
- [ ] EfProductRepository implementado
- [ ] Program.cs actualizado con DbContext
- [ ] Migraciones se aplican automáticamente
- [ ] Datos se persisten en PostgreSQL
- [ ] Datos persisten después de reiniciar servicio
- [ ] Logs de EF Core aparecen en consola

### 📊 Estructura Creada

```
Infrastructure/
├── ProductDbContext.cs              # DbContext
├── DesignTimeDbContextFactory.cs   # Factory para migraciones
├── EfProductRepository.cs           # Repositorio con EF Core
└── Migrations/                      # Migraciones de EF Core
    ├── [timestamp]_InitialCreate.cs
    └── ProductDbContextModelSnapshot.cs
```

### 💡 Conceptos Clave

**AsNoTracking():**
- Para consultas de solo lectura
- Mejor rendimiento
- No trackea cambios

**SaveChangesAsync():**
- Persiste cambios en BD
- Retorna número de filas afectadas
- Maneja transacciones automáticamente

**Migraciones:**
- Versionan el esquema de BD
- Aplicables de forma incremental
- Reversibles

### 🐛 Solución de Problemas

**Error: "dotnet-ef not found"**
- Instalar: `dotnet tool install --global dotnet-ef`
- Agregar al PATH si es necesario

**Error: "Cannot connect to PostgreSQL"**
- Verificar que Docker esté corriendo: `docker ps`
- Verificar que el contenedor esté healthy: `docker compose ps`
- Verificar connection string en `appsettings.json`
- Verificar que el puerto 5432 no esté ocupado por otra instancia de PostgreSQL

**Error: `42P07: relation "Products" already exists`**
- La tabla ya existe en la BD pero EF Core no tiene registro en `__EFMigrationsHistory`
- **Solución rápida (borra datos):** Limpiar el volumen de Docker y empezar de cero:

```bash
docker compose down -v && docker compose up -d postgres
```

- **Solución sin perder datos:** Marcar la migración como aplicada manualmente:

```bash
docker exec -it microservices-postgres psql -U postgres -d microservices_db \
  -c "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20260210030522_InitialCreate', '10.0.0');"
```

**Error: "Migration already exists"**
- Eliminar carpeta `Infrastructure/Migrations` si es necesario
- O crear nueva migración con nombre diferente: `dotnet ef migrations add NombreDiferente`

**Datos no persisten**
- Verificar que se use `EfProductRepository` (no `InMemoryProductRepository`)
- Verificar que el DI use `AddScoped` (no `AddSingleton`) para el repositorio EF
- Verificar logs de EF Core en la consola
- Verificar directamente en PostgreSQL:

```bash
docker exec -it microservices-postgres psql -U postgres -d microservices_db -c 'SELECT * FROM "Products";'
```

