# Módulo 11 – Alta disponibilidad y tolerancia

## 🧠 Teoría

### Circuit Breaker (Polly)

El Circuit Breaker previene cascadas de fallos:
- **Closed**: Funcionamiento normal
- **Open**: Circuito abierto, fallos rápidos
- **Half-Open**: Probando recuperación

### Retry

Reintentos con backoff:
- **Exponential Backoff**: Espera exponencial
- **Jitter**: Variación aleatoria
- **Max Attempts**: Límite de reintentos

### Fallback

Respuesta alternativa cuando falla:
- Cache
- Valor por defecto
- Servicio degradado

### Patterns de resiliencia

- **Bulkhead**: Aislar recursos
- **Timeout**: Límites de tiempo
- **Health Checks**: Monitoreo de salud

## 🧪 Laboratorio 11

### Objetivo
Añadir Polly a microservicio:
- Configurar retry policy
- Implementar circuit breaker
- Agregar fallback
- Timeout policies

### Implementación

**Paquetes NuGet:**
- `Polly`
- `Microsoft.Extensions.Http.Polly`

**Configuración:**
```csharp
builder.Services.AddHttpClient()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

### Próximos pasos

Ver documentación oficial de Polly.

