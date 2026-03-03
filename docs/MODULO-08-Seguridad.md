# Módulo 8 – Seguridad: JWT Authentication en Microservicios

## 🧠 Teoría

### OAuth2, OIDC y JWT

**OAuth2:**
- Framework de autorización estándar de la industria
- Define flujos (flows) para obtener tokens de acceso
- Separa la autenticación de la autorización
- Scopes definen los permisos del token

**OpenID Connect (OIDC):**
- Extensión de OAuth2 para **autenticación**
- Agrega un `id_token` (JWT) además del `access_token`
- Proporciona endpoint `/userinfo` con datos del usuario
- Es el estándar para "Login con Google/Microsoft/etc."

**JSON Web Token (JWT):**
- Formato compacto y auto-contenido para transmitir claims entre partes
- Estructura: `Header.Payload.Signature` (Base64URL)
- **Header:** algoritmo y tipo (`{"alg": "HS256", "typ": "JWT"}`)
- **Payload:** claims del usuario (`sub`, `name`, `role`, `exp`, etc.)
- **Signature:** garantiza que el token no fue alterado

```
eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbiIsInJvbGUiOiJBZG1pbiJ9.firma_digital
|___ Header ___|.___________ Payload ___________|.___ Signature ___|
```

### Estrategia de Seguridad en Microservicios

```
┌──────────┐    ┌──────────────┐    ┌─────────────────┐
│  Cliente  │───▶│  Auth        │───▶│  Genera JWT     │
│  (SPA)   │    │  Controller  │    │  con roles      │
└──────────┘    └──────────────┘    └─────────────────┘
                                              │
                      Token JWT               │
                ┌─────────────────────────────┘
                ▼
┌──────────────────────────────────────────────────────┐
│              Authorization Header                     │
│         Bearer eyJhbGciOi...                         │
├──────────────────────────────────────────────────────┤
│                                                       │
│  ┌──────────────┐   ┌──────────────┐                 │
│  │ ProductService│   │ OrderService │                 │
│  │              │   │              │                 │
│  │ [Authorize]  │   │ [Authorize]  │                 │
│  │ Valida JWT   │   │ Valida JWT   │                 │
│  │ Verifica rol │   │ Verifica rol │                 │
│  └──────────────┘   └──────────────┘                 │
│                                                       │
│  Misma clave secreta ──▶ Validación independiente    │
└──────────────────────────────────────────────────────┘
```

**Principios aplicados:**
1. **Autenticación centralizada:** Un único endpoint genera los tokens
2. **Validación distribuida:** Cada microservicio valida el JWT independientemente
3. **Zero Trust:** Cada request debe incluir un token válido
4. **Principio de mínimo privilegio:** Roles y políticas granulares

### Azure AD (Producción)

En producción, reemplazaríamos nuestro `AuthController` por Azure AD:
- Autenticación empresarial con SSO
- Multi-factor authentication (MFA)
- Integración con Microsoft 365
- App Registrations para cada microservicio

> **Nota:** En este laboratorio usamos un AuthController local para simplificar.
> En el Módulo 9 (API Gateway) veremos cómo centralizar la autenticación.

---

## 🧪 Laboratorio 8 – Proteger APIs con JWT

### Objetivo

Al finalizar este laboratorio habrás:
1. ✅ Configurado autenticación JWT en ProductService y OrderService
2. ✅ Creado un endpoint de login que genera tokens con roles
3. ✅ Protegido endpoints con `[Authorize]` y políticas por rol
4. ✅ Configurado Swagger UI para enviar tokens JWT
5. ✅ Probado el flujo completo: login → token → acceso protegido

### Arquitectura de seguridad

```
Usuarios de prueba:
┌─────────────────────────────────────────────┐
│  admin  / admin123   → Rol: Admin           │
│  reader / reader123  → Rol: Reader          │
│  user   / user123    → Rol: User            │
└─────────────────────────────────────────────┘

Políticas de autorización:
┌─────────────────────────────────────────────┐
│  AdminOnly  → Solo rol "Admin"              │
│  ReadOnly   → Roles "Admin" o "Reader"      │
└─────────────────────────────────────────────┘

Protección de endpoints:
┌──────────────────────────────────────────────────────┐
│  GET  /api/v1/products        → [AllowAnonymous]     │
│  GET  /api/v1/products/{id}   → [AllowAnonymous]     │
│  POST /api/v1/products        → [Authorize] AdminOnly│
│  PUT  /api/v1/products/{id}   → [Authorize] AdminOnly│
│  DEL  /api/v1/products/{id}   → [Authorize] AdminOnly│
│  GET  /api/v2/products/*      → [Authorize] ReadOnly │
│  GET  /api/v1/config          → [Authorize] AdminOnly│
│                                                       │
│  GET  /api/v1/orders          → [AllowAnonymous]     │
│  GET  /api/v1/orders/{id}     → [Authorize]          │
│  POST /api/v1/orders          → [Authorize] AdminOnly│
│  DEL  /api/v1/orders/{id}     → [Authorize] AdminOnly│
│                                                       │
│  POST /api/auth/login         → Público (genera JWT) │
│  GET  /api/auth/users         → Público (ver users)  │
└──────────────────────────────────────────────────────┘
```

---

### Paso 1 – Agregar paquete NuGet

Instalar el paquete de autenticación JWT en ambos servicios:

```bash
# ProductService
cd src/Services/ProductService
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# OrderService
cd ../OrderService
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Verificar que el `.csproj` de cada servicio incluya:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.3" />
```

---

### Paso 2 – Configurar JWT en appsettings.json

#### ProductService – `appsettings.json`

Agregar la sección `Jwt` al archivo de configuración:

```json
{
  "Jwt": {
    "Key": "S3cur3K3y_F0r_D3v3l0pm3nt_Purp0s3s_Only_2025!",
    "Issuer": "microservices-net-2025",
    "Audience": "microservices-api",
    "ExpirationMinutes": 60
  }
}
```

#### OrderService – `appsettings.json`

Agregar la misma sección (misma clave para que ambos servicios validen el mismo token):

```json
{
  "Jwt": {
    "Key": "S3cur3K3y_F0r_D3v3l0pm3nt_Purp0s3s_Only_2025!",
    "Issuer": "microservices-net-2025",
    "Audience": "microservices-api",
    "ExpirationMinutes": 60
  }
}
```

> **⚠️ Importante:** En producción, la clave NUNCA se pone en `appsettings.json`.
> Se usa `dotnet user-secrets`, Azure Key Vault o variables de entorno.

---

### Paso 3 – Registrar Authentication y Authorization en Program.cs

#### ProductService – `Program.cs`

Agregar los **usings** necesarios al inicio del archivo:

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
```

Luego, **después** del bloque de API Versioning y **antes** de Swagger, agregar la configuración JWT:

```csharp
// ============================
// JWT Authentication
// ============================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? "S3cur3K3y_F0r_D3v3l0pm3nt_Purp0s3s_Only_2025!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           // Valida quién emitió el token
        ValidateAudience = true,         // Valida para quién es el token
        ValidateLifetime = true,         // Valida que no haya expirado
        ValidateIssuerSigningKey = true, // Valida la firma digital
        ValidIssuer = jwtSettings["Issuer"] ?? "microservices-net-2025",
        ValidAudience = jwtSettings["Audience"] ?? "microservices-api",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero       // Sin tolerancia de tiempo
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ReadOnly", policy => policy.RequireRole("Admin", "Reader"));
});
```

**¿Qué hace cada validación?**

| Parámetro | Propósito |
|---|---|
| `ValidateIssuer` | Rechaza tokens emitidos por otro sistema |
| `ValidateAudience` | Rechaza tokens destinados a otra API |
| `ValidateLifetime` | Rechaza tokens expirados |
| `ValidateIssuerSigningKey` | Rechaza tokens con firma inválida |
| `ClockSkew = Zero` | Sin los 5 minutos de tolerancia por defecto |

#### Configurar Swagger con soporte JWT

Reemplazar la configuración simple de Swagger por una que incluya el botón "Authorize":

```csharp
// Swagger / OpenAPI (versioned)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Configurar JWT en Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Ejemplo: eyJhbGciOi..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer"
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
```

#### Agregar middleware de Authentication en el pipeline

Buscar la línea `app.UseAuthorization()` y agregar `app.UseAuthentication()` **justo antes**:

```csharp
app.UseHttpsRedirection();
app.UseAuthentication();   // ← NUEVO: Valida el token JWT
app.UseAuthorization();    // ← Ya existía: Verifica roles/políticas
app.MapControllers();
```

> **⚠️ El orden importa:** `UseAuthentication()` SIEMPRE va antes de `UseAuthorization()`.

#### OrderService – `Program.cs`

Aplicar exactamente la misma configuración. Agregar los usings:

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
```

Y registrar Authentication + Authorization después de `AddSingleton<IOrderRepository>`:

```csharp
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// ============================
// JWT Authentication
// ============================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? "S3cur3K3y_F0r_D3v3l0pm3nt_Purp0s3s_Only_2025!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "microservices-net-2025",
        ValidAudience = jwtSettings["Audience"] ?? "microservices-api",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ReadOnly", policy => policy.RequireRole("Admin", "Reader"));
});
```

Y en el pipeline HTTP:

```csharp
app.UseHttpsRedirection();
app.UseAuthentication();   // ← NUEVO
app.UseAuthorization();
app.MapControllers();
```

---

### Paso 4 – Crear los DTOs de autenticación

#### ProductService – `DTOs/LoginDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

#### OrderService – `DTOs/LoginDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

---

### Paso 5 – Crear el AuthController

Este controller simula un Identity Provider (IdP) para el laboratorio. En producción,
esto lo haría Azure AD, Keycloak, Auth0, etc.

#### ProductService – `Controllers/AuthController.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProductService.DTOs;

namespace ProductService.Controllers;

/// <summary>
/// Controller para autenticación (desarrollo/laboratorio).
/// En producción se usaría Azure AD, Keycloak u otro IdP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    // Usuarios simulados para el laboratorio
    private static readonly Dictionary<string, (string Password, string Role)> Users = new()
    {
        ["admin"] = ("admin123", "Admin"),
        ["reader"] = ("reader123", "Reader"),
        ["user"] = ("user123", "User")
    };

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Genera un JWT token para el usuario autenticado.
    /// Usuarios disponibles: admin/admin123, reader/reader123, user/user123
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        // Validar credenciales simuladas
        if (!Users.TryGetValue(loginDto.Username.ToLower(), out var userData) ||
            userData.Password != loginDto.Password)
        {
            _logger.LogWarning("Login fallido para usuario: {Username}", loginDto.Username);
            return Unauthorized(new { Message = "Credenciales inválidas" });
        }

        var (_, role) = userData;
        var token = GenerateJwtToken(loginDto.Username, role);
        var expiration = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60));

        _logger.LogInformation("Login exitoso: {Username} con rol {Role}",
            loginDto.Username, role);

        return Ok(new TokenResponseDto
        {
            Token = token,
            Expiration = expiration,
            Username = loginDto.Username,
            Role = role
        });
    }

    /// <summary>
    /// Muestra los usuarios disponibles para pruebas.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTestUsers()
    {
        var users = Users.Select(u => new
        {
            Username = u.Key,
            Password = u.Value.Password,
            Role = u.Value.Role
        });

        return Ok(users);
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = jwtSettings["Key"]
                  ?? "S3cur3K3y_F0r_D3v3l0pm3nt_Purp0s3s_Only_2025!";
        var issuer = jwtSettings["Issuer"] ?? "microservices-net-2025";
        var audience = jwtSettings["Audience"] ?? "microservices-api";
        var expirationMinutes = _configuration.GetValue<int>(
            "Jwt:ExpirationMinutes", 60);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)  // ← Clave para [Authorize(Roles)]
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**Anatomía del token generado:**

```json
// Header
{
  "alg": "HS256",
  "typ": "JWT"
}

// Payload (claims)
{
  "sub": "admin",
  "jti": "a1b2c3d4-...",
  "iat": 1709337600,
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "admin",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
  "exp": 1709341200,
  "iss": "microservices-net-2025",
  "aud": "microservices-api"
}
```

#### OrderService – `Controllers/AuthController.cs`

Crear el mismo controller pero con el namespace `OrderService.Controllers` y usando `OrderService.DTOs`.

> **💡 Tip:** El código es idéntico al de ProductService, solo cambian los namespaces.
> Ambos servicios comparten la misma clave JWT, así que un token generado en uno
> es válido en el otro.

---

### Paso 6 – Proteger los Controllers con [Authorize]

#### ProductService – V1 ProductsController

Agregar `using Microsoft.AspNetCore.Authorization;` y los atributos:

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;

namespace ProductService.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]                                          // ← Protege TODO el controller
public class ProductsController: ControllerBase
{
    // ... constructor sin cambios ...

    [HttpGet]
    [AllowAnonymous]                                 // ← Lectura pública
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(...)

    [HttpGet("{id}")]
    [AllowAnonymous]                                 // ← Lectura pública
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> GetById(...)

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]                // ← Solo Admin puede crear
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> Create(...)

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]                // ← Solo Admin puede editar
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(...)

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]                // ← Solo Admin puede eliminar
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(...)
}
```

#### ProductService – V2 ProductsController

```csharp
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ReadOnly")]                     // ← Admin o Reader
public class ProductsController : ControllerBase
```

#### ProductService – ConfigController

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]                    // ← Solo Admin ve la config
public class ConfigController: ControllerBase
```

#### OrderService – OrdersController

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Clients;
using OrderService.Domain;
using OrderService.DTOs;

namespace OrderService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]                                          // ← Protege TODO el controller
public class OrdersController : ControllerBase
{
    // ... constructor sin cambios ...

    [HttpGet]
    [AllowAnonymous]                                 // ← Listar órdenes es público
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(...)

    [HttpGet("{id}")]                                // ← Requiere autenticación
    public async Task<ActionResult<OrderDto>> GetById(...)

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]                // ← Solo Admin crea órdenes
    public async Task<ActionResult<OrderDto>> Create(...)

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]                // ← Solo Admin elimina
    public async Task<IActionResult> Delete(...)
}
```

**Resumen de atributos:**

| Atributo | Efecto |
|---|---|
| `[Authorize]` (en clase) | Todos los endpoints requieren token válido |
| `[AllowAnonymous]` | Excepciona un endpoint del `[Authorize]` de clase |
| `[Authorize(Policy = "AdminOnly")]` | Requiere token con `role: Admin` |
| `[Authorize(Policy = "ReadOnly")]` | Requiere token con `role: Admin` o `Reader` |

---

### Paso 7 – Verificar la compilación

```bash
cd src/Services/ProductService
dotnet build

cd ../OrderService
dotnet build
```

Ambos servicios deben compilar sin errores nuevos.

---

### Paso 8 – Probar el flujo completo

#### 8.1 – Levantar ProductService

```bash
cd src/Services/ProductService
dotnet run
```

#### 8.2 – Ver usuarios disponibles

```bash
curl http://localhost:5001/api/auth/users | jq
```

Respuesta:
```json
[
  { "username": "admin",  "password": "admin123",  "role": "Admin"  },
  { "username": "reader", "password": "reader123", "role": "Reader" },
  { "username": "user",   "password": "user123",   "role": "User"   }
]
```

#### 8.3 – Probar acceso SIN token (endpoints públicos)

```bash
# ✅ GET productos es público
curl http://localhost:5001/api/v1/products | jq

# ✅ GET producto por ID es público
curl http://localhost:5001/api/v1/products/{id} | jq
```

#### 8.4 – Probar acceso SIN token (endpoints protegidos)

```bash
# ❌ POST producto sin token → 401 Unauthorized
curl -X POST http://localhost:5001/api/v1/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","description":"Test","price":10,"stock":5}'
```

Respuesta esperada: **401 Unauthorized**

#### 8.5 – Obtener un token JWT (Login)

```bash
# Login como admin
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | jq
```

Respuesta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-03-02T20:00:00Z",
  "username": "admin",
  "role": "Admin"
}
```

> **💡 Tip:** Copia el valor de `token` y pégalo en [jwt.io](https://jwt.io) para ver los claims decodificados.

#### 8.6 – Acceder con token (Admin)

```bash
# Guardar token en variable
TOKEN=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | jq -r '.token')

# ✅ POST producto con token Admin → 201 Created
curl -X POST http://localhost:5001/api/v1/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Tablet","description":"Nueva tablet","price":499.99,"stock":15}' | jq

# ✅ DELETE producto con token Admin → 204
curl -X DELETE http://localhost:5001/api/v1/products/{id} \
  -H "Authorization: Bearer $TOKEN"

# ✅ GET config con token Admin → 200
curl http://localhost:5001/api/v1/config \
  -H "Authorization: Bearer $TOKEN" | jq
```

#### 8.7 – Probar con rol Reader

```bash
# Login como reader
TOKEN_READER=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"reader","password":"reader123"}' | jq -r '.token')

# ✅ GET v2 products (ReadOnly policy) → 200
curl http://localhost:5001/api/v2/products \
  -H "Authorization: Bearer $TOKEN_READER" | jq

# ❌ POST producto con Reader → 403 Forbidden
curl -X POST http://localhost:5001/api/v1/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN_READER" \
  -d '{"name":"Test","description":"Test","price":10,"stock":5}'
```

Respuesta esperada: **403 Forbidden** (autenticado pero sin permisos)

#### 8.8 – Probar con rol User (sin políticas)

```bash
TOKEN_USER=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"user123"}' | jq -r '.token')

# ❌ GET v2 products (necesita ReadOnly) → 403 Forbidden
curl http://localhost:5001/api/v2/products \
  -H "Authorization: Bearer $TOKEN_USER"

# ❌ POST producto (necesita AdminOnly) → 403 Forbidden
curl -X POST http://localhost:5001/api/v1/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN_USER" \
  -d '{"name":"Test","description":"Test","price":10,"stock":5}'
```

#### 8.9 – Token compartido entre servicios

El mismo token funciona en OrderService (misma clave JWT):

```bash
# Login en ProductService
TOKEN=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | jq -r '.token')

# ✅ Usar ese token en OrderService → funciona!
curl http://localhost:5003/api/v1/orders \
  -H "Authorization: Bearer $TOKEN" | jq
```

---

### Paso 9 – Probar desde Swagger UI

1. Abrir `http://localhost:5001` en el navegador (Swagger UI)
2. Hacer clic en el botón **🔓 Authorize** (arriba a la derecha)
3. En el campo "Value", pegar el token JWT (sin el prefijo "Bearer")
4. Hacer clic en **Authorize** → **Close**
5. Ahora las peticiones desde Swagger incluirán el header `Authorization: Bearer <token>`
6. Probar los endpoints protegidos

---

### Paso 10 – Credenciales inválidas

```bash
# ❌ Password incorrecto → 401
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"wrong"}'
```

Respuesta:
```json
{
  "message": "Credenciales inválidas"
}
```

---

## 📊 Matriz de respuestas HTTP

| Escenario | HTTP Status | Significado |
|---|---|---|
| Sin token → endpoint público | **200 OK** | Acceso permitido |
| Sin token → endpoint protegido | **401 Unauthorized** | No autenticado |
| Token válido + rol correcto | **200/201/204** | Acceso permitido |
| Token válido + rol incorrecto | **403 Forbidden** | Autenticado pero sin permisos |
| Token expirado | **401 Unauthorized** | Token ya no es válido |
| Token con firma inválida | **401 Unauthorized** | Token alterado/falso |

---

## 🔑 Conceptos clave

### Diferencia entre 401 y 403

```
401 Unauthorized  = "No sé quién eres"  (falta token o token inválido)
403 Forbidden     = "Sé quién eres, pero no tienes permiso" (token válido, rol incorrecto)
```

### ¿Por qué ClockSkew = Zero?

Por defecto, .NET permite 5 minutos de tolerancia en la expiración del token (para compensar
diferencias de reloj entre servidores). Con `TimeSpan.Zero`, el token expira exactamente
cuando dice el claim `exp`.

### Token compartido entre servicios

Como ambos servicios usan la **misma clave secreta**, un token generado por ProductService
es válido en OrderService y viceversa. Esto es una decisión de diseño:

- **Clave compartida** → más simple, un login sirve para todo
- **Claves separadas** → más seguro, cada servicio tiene su propio scope

En producción con Azure AD, cada servicio tendría su propio App Registration con scopes específicos.

---

## 🏗️ Estructura final de archivos modificados/creados

```
src/Services/
├── ProductService/
│   ├── appsettings.json              ← +Jwt section
│   ├── Program.cs                     ← +Authentication, +Authorization, +Swagger JWT
│   ├── DTOs/
│   │   └── LoginDto.cs               ← NUEVO
│   └── Controllers/
│       ├── AuthController.cs          ← NUEVO (genera tokens)
│       ├── V1/
│       │   ├── ProductsV1Controller.cs ← +[Authorize], +[AllowAnonymous]
│       │   └── ConfigController.cs     ← +[Authorize(Policy = "AdminOnly")]
│       └── V2/
│           └── ProductsV2Controller.cs ← +[Authorize(Policy = "ReadOnly")]
│
└── OrderService/
    ├── appsettings.json               ← +Jwt section
    ├── Program.cs                      ← +Authentication, +Authorization
    ├── DTOs/
    │   └── LoginDto.cs                ← NUEVO
    └── Controllers/
        ├── AuthController.cs           ← NUEVO (genera tokens)
        └── OrdersController.cs         ← +[Authorize], +[AllowAnonymous]
```

---

## 🚀 Próximos pasos

- **Módulo 9 – API Gateway:** Centralizar la validación de tokens en Ocelot/YARP
- **Producción:** Reemplazar el AuthController por Azure AD App Registration
- **Mejoras:** Refresh tokens, token revocation, rate limiting por usuario

