# Módulo 15 – Infraestructura como Código con Terraform

## 🧠 Teoría

### IaC (Infrastructure as Code)

IaC permite gestionar infraestructura como código:
- Versionamiento
- Reproducibilidad
- Automatización
- Documentación implícita

### Arquitectura reproducible

Con Terraform puedes:
- Definir recursos Azure
- Versionar cambios
- Aplicar cambios de forma idempotente
- Colaborar en equipo

### State Management

Terraform mantiene estado:
- Estado local o remoto
- Blob storage para estado compartido
- Locking para evitar conflictos

## 🧪 Laboratorio 15

### Objetivo
Crear ACR + AKS + Service Bus con Terraform:
- Definir recursos Azure
- Configurar networking
- Aplicar cambios
- Gestionar estado

### Estructura

```
infrastructure/terraform/
├── main.tf
├── variables.tf
├── outputs.tf
└── modules/
    ├── aks/
    ├── acr/
    └── service-bus/
```

### Próximos pasos

Ver documentación oficial de Terraform y Azure Provider.

