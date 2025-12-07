# Módulo 13 – Azure Cloud para Microservicios

## 🧠 Teoría

### Resource Groups, VNets, Subnets

**Resource Groups:**
- Agrupación lógica de recursos
- Facilita gestión y facturación
- Lifecycle management

**Virtual Networks (VNets):**
- Redes privadas en Azure
- Aislamiento de recursos
- Peering entre VNets

**Subnets:**
- Segmentación de VNets
- Network security groups
- Control de tráfico

### Container Registry (ACR)

Azure Container Registry almacena imágenes Docker:
- Privado y seguro
- Integración con AKS
- Escaneo de vulnerabilidades
- Geo-replicación

### AKS

Azure Kubernetes Service:
- Kubernetes gestionado
- Auto-scaling
- Integración con Azure
- CI/CD integrado

## 🧪 Laboratorio 13

### Objetivo
Dockerizar microservicio y subirlo al ACR:
- Crear Dockerfile
- Build de imagen
- Push a ACR
- Pull en AKS

### Comandos

```bash
# Build
docker build -t productservice:latest .

# Tag para ACR
docker tag productservice:latest myregistry.azurecr.io/productservice:latest

# Push
docker push myregistry.azurecr.io/productservice:latest
```

### Próximos pasos

Ver documentación oficial de ACR y AKS.

