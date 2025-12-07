# Módulo 9 – Comunicación e interoperabilidad

## 🧠 Teoría

### API Gateway (Ocelot o YARP)

**Ocelot:**
- .NET específico
- Configuración por JSON
- Routing, rate limiting, load balancing

**YARP (Yet Another Reverse Proxy):**
- Microsoft oficial
- Alto rendimiento
- Configuración programática
- Mejor para .NET 6+

### gRPC en .NET

gRPC es ideal para comunicación entre microservicios:
- Contratos fuertemente tipados (.proto)
- Streaming bidireccional
- Muy eficiente
- Mejor rendimiento que REST

### Recomendaciones de resiliencia

- **Circuit Breaker**: Abrir circuito tras fallos
- **Retry**: Reintentar con backoff exponencial
- **Timeout**: Evitar esperas indefinidas
- **Bulkhead**: Aislar recursos

## 🧪 Laboratorio 9

### Objetivo
Crear API Gateway con YARP:
- Configurar routing
- Agregar rate limiting
- Implementar load balancing
- Health checks

### Implementación

**Paquetes NuGet:**
- `Yarp.ReverseProxy`

**Configuración:**
```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

### Próximos pasos

Ver documentación oficial de YARP.

