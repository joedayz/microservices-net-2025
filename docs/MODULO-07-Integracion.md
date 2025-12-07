# Módulo 7 – Integración

## 🧠 Teoría

### REST vs gRPC

**REST:**
- HTTP/JSON
- Fácil de depurar
- Compatible con navegadores
- Overhead mayor

**gRPC:**
- HTTP/2, Protocol Buffers
- Muy eficiente
- Streaming bidireccional
- Mejor para microservicios internos

### Event-Driven con Service Bus / Event Hub

**Azure Service Bus:**
- Colas y topics
- Mensajería asíncrona
- Garantías de entrega
- Dead letter queue

**Azure Event Hub:**
- Streaming de eventos
- Alta throughput
- Particionamiento
- Retención configurable

### Eventual Consistency

En microservicios, la consistencia eventual es común:
- Cada servicio tiene su propia BD
- Eventos propagan cambios
- Sincronización asíncrona
- Trade-off: Simplicidad vs Consistencia

## 🧪 Laboratorio 7

### Objetivo
Microservicio publicando eventos a Azure Service Bus:
- Configurar Service Bus
- Publicar eventos de dominio
- Consumir eventos
- Manejar errores

### Implementación

**Paquetes NuGet:**
- `Azure.Messaging.ServiceBus`

**Publicar evento:**
```csharp
await _serviceBusClient.CreateSender("product-events")
    .SendMessageAsync(new ServiceBusMessage(json));
```

### Próximos pasos

Ver documentación oficial de Azure Service Bus.

