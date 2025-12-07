# Módulo 10 – Serverless & Event-Driven

## 🧠 Teoría

### Azure Functions

Azure Functions permite ejecutar código sin infraestructura:
- Pay-per-use
- Escalado automático
- Triggers variados (HTTP, Queue, Timer, etc.)
- Integración con otros servicios Azure

### Durable Functions

Durable Functions extiende Azure Functions:
- Orquestación de funciones
- State management
- Patrones complejos (Fan-out/Fan-in, Human interaction)

### Integración con eventos

Las Functions pueden:
- Consumir eventos de Service Bus
- Procesar eventos de Event Hub
- Reaccionar a cambios en Cosmos DB
- Integrar con Logic Apps

## 🧪 Laboratorio 10

### Objetivo
Crear Azure Function que consuma evento:
- Configurar trigger de Service Bus
- Procesar mensaje
- Integrar con otros servicios
- Manejar errores

### Estructura

```
Functions/
└── ProcessProductEvent/
    └── ProcessProductEvent.cs
```

### Próximos pasos

Ver documentación oficial de Azure Functions.

