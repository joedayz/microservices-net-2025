# Módulo 12 – Balanceo de carga

## 🧠 Teoría

### Load Balancer vs Application Gateway

**Load Balancer (L4):**
- Balanceo a nivel de red
- Basado en IP y puerto
- Más rápido, menos inteligente

**Application Gateway (L7):**
- Balanceo a nivel de aplicación
- Basado en URL, headers
- SSL termination
- WAF integrado

### Kubernetes Service Types

**ClusterIP:**
- Acceso interno al cluster
- IP virtual del cluster

**NodePort:**
- Expone puerto en cada nodo
- Acceso externo básico

**LoadBalancer:**
- IP externa dedicada
- Integración con cloud provider

**Ingress:**
- Routing HTTP/HTTPS
- SSL termination
- Path-based routing

## 🧪 Laboratorio 12

### Objetivo
Desplegar microservicio en AKS:
- Crear cluster AKS
- Desplegar aplicación
- Configurar servicios
- Exponer con Ingress

### Estructura

```
infrastructure/kubernetes/
├── product-service/
│   ├── deployment.yaml
│   ├── service.yaml
│   └── ingress.yaml
```

### Próximos pasos

Ver documentación oficial de AKS.

