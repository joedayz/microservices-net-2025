# Módulo 8 – Seguridad

## 🧠 Teoría

### OAuth2, OIDC

**OAuth2:**
- Framework de autorización
- Tokens de acceso
- Scopes y permisos

**OpenID Connect (OIDC):**
- Extensión de OAuth2
- Autenticación + Autorización
- ID tokens

### Azure AD

Azure Active Directory proporciona:
- Autenticación empresarial
- Single Sign-On (SSO)
- Multi-factor authentication
- Integración con aplicaciones

### API Gateway Flows

**Flujos comunes:**
1. Client → API Gateway → Microservicio
2. Validación de token en Gateway
3. Propagación de claims
4. Rate limiting

## 🧪 Laboratorio 8

### Objetivo
Proteger API con Azure AD App Registration:
- Registrar aplicación en Azure AD
- Configurar autenticación JWT
- Validar tokens
- Proteger endpoints

### Implementación

**Paquetes NuGet:**
- `Microsoft.AspNetCore.Authentication.JwtBearer`

**Configuración:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/{tenant-id}";
        options.Audience = "{client-id}";
    });
```

### Próximos pasos

Ver documentación oficial de Azure AD.

