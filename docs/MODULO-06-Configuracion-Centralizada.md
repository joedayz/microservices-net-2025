# Módulo 6 – Configuración centralizada

## 🧠 Teoría

### Config Server (Azure App Configuration)

Azure App Configuration proporciona un servicio centralizado para gestionar configuraciones de aplicaciones:
- Configuración centralizada
- Feature flags
- Versionamiento de configuración
- Integración con Key Vault

### Feature Flags

Permiten activar/desactivar funcionalidades sin redeploy:
- A/B testing
- Rollout gradual
- Kill switches
- Configuración dinámica

### Secret Management (Azure Key Vault)

Azure Key Vault almacena secretos de forma segura:
- Connection strings
- API keys
- Certificados
- Rotación automática

## 🧪 Laboratorio 6

### Objetivo
Configurar Azure App Configuration + Key Vault:
- Conectar a App Configuration
- Leer configuración centralizada
- Integrar con Key Vault para secretos
- Implementar feature flags

### Implementación

**Paquetes NuGet:**
- `Microsoft.Azure.AppConfiguration.AspNetCore`
- `Azure.Extensions.AspNetCore.Configuration.Secrets`

**Configuración:**
```csharp
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
        .Select(KeyFilter.Any, LabelFilter.Null)
        .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
});
```

### Próximos pasos

Ver documentación oficial de Azure para implementación completa.

