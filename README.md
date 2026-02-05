# Taller: Microservicios .NET + Azure + Terraform + Istio

## 📚 Estructura del Taller

Este taller está diseñado para aprender a construir microservicios empresariales usando .NET 10, Azure, Terraform e Istio.

### 🎯 Objetivos

- Comprender los fundamentos de arquitectura de microservicios
- Implementar microservicios con .NET 10 siguiendo mejores prácticas
- Integrar servicios con Azure (App Configuration, Key Vault, Service Bus, AKS)
- Automatizar infraestructura con Terraform
- Implementar observabilidad con Istio
- Crear pipelines CI/CD completos

### 📋 Módulos

- ✅ **Módulo 1**: Fundamentos de Microservicios - **COMPLETADO**
- ✅ **Módulo 2**: Principios y patrones de diseño (DDD, Hexagonal Architecture) - **COMPLETADO**
- ✅ **Módulo 3**: Buenas prácticas de diseño (Versionamiento, DTOs) - **COMPLETADO**
- ✅ **Módulo 4**: Persistencia de datos (PostgreSQL, MongoDB) - **COMPLETADO**
- ✅ **Módulo 5**: Performance y consultas (Redis, índices) - **COMPLETADO**
- 📝 **Módulo 6**: Configuración centralizada (Azure App Configuration, Key Vault) - **DOCUMENTADO**
- 📝 **Módulo 7**: Integración (REST, gRPC, Service Bus) - **DOCUMENTADO**
- 📝 **Módulo 8**: Seguridad (Azure AD, OAuth2) - **DOCUMENTADO**
- 📝 **Módulo 9**: Comunicación (API Gateway, gRPC) - **DOCUMENTADO**
- 📝 **Módulo 10**: Serverless (Azure Functions) - **DOCUMENTADO**
- 📝 **Módulo 11**: Alta disponibilidad (Polly, Circuit Breaker) - **DOCUMENTADO**
- 📝 **Módulo 12**: Balanceo de carga (AKS) - **DOCUMENTADO**
- 📝 **Módulo 13**: Azure Cloud (ACR, AKS) - **DOCUMENTADO**
- 📝 **Módulo 14**: DevOps (CI/CD Pipelines) - **DOCUMENTADO**
- 📝 **Módulo 15**: Terraform (IaC) - **DOCUMENTADO**
- 📝 **Módulo 16**: Observabilidad (Istio, Jaeger, Kiali, Prometheus) - **DOCUMENTADO**

### 🏗️ Estructura del Proyecto

```
microservices-net-2025/
├── src/
│   ├── Services/
│   │   ├── ProductService/          # Microservicio de Productos
│   │   ├── OrderService/            # Microservicio de Órdenes
│   │   └── UserService/             # Microservicio de Usuarios
│   ├── Gateway/                    # API Gateway (Ocelot/YARP)
│   └── Functions/                   # Azure Functions
├── infrastructure/
│   ├── terraform/                   # Scripts de Terraform
│   └── kubernetes/                  # Manifiestos de Kubernetes
├── docker/                          # Dockerfiles
├── .github/
│   └── workflows/                   # GitHub Actions
└── docs/                            # Documentación de módulos
```

### 🚀 Requisitos Previos

- .NET 10 SDK
- Docker Desktop (o Podman)
- Azure CLI
- Terraform
- kubectl
- istioctl
- JetBrains Rider (recomendado) o Visual Studio Code / Visual Studio

### 💻 Instalación

#### .NET 10 SDK

**Windows:**
```bash
# Descargar e instalar desde:
# https://dotnet.microsoft.com/download/dotnet/10.0

# Verificar instalación
dotnet --version
# Debe mostrar: 10.x.x
```

**macOS:**
```bash
# Usando Homebrew
brew install --cask dotnet-sdk

# Verificar instalación
dotnet --version
# Debe mostrar: 10.x.x
```

**Linux (Ubuntu/Debian):**
```bash
# Agregar repositorio de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar .NET 10 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Verificar instalación
dotnet --version
# Debe mostrar: 10.x.x
```

**Descarga directa:**
- Visita: https://dotnet.microsoft.com/download/dotnet/10.0
- Selecciona tu sistema operativo
- Descarga e instala el SDK (no solo el Runtime)

#### JetBrains Rider

**Windows/macOS/Linux:**
```bash
# Descargar desde:
# https://www.jetbrains.com/rider/download/

# O usar JetBrains Toolbox (recomendado):
# https://www.jetbrains.com/toolbox-app/
```

**Alternativas:**
- **Visual Studio Code** con extensión C#: https://code.visualstudio.com/
- **Visual Studio 2022**: https://visualstudio.microsoft.com/
- **Rider** (recomendado para este taller): https://www.jetbrains.com/rider/

#### Docker Desktop (o Podman)

**Docker Desktop:**
- **Windows/macOS**: https://www.docker.com/products/docker-desktop
- **Linux**: https://docs.docker.com/engine/install/

**Podman (alternativa):**
- Ver instrucciones en [`docs/PODMAN-SETUP.md`](./docs/PODMAN-SETUP.md)

**Verificar instalación:**
```bash
docker --version
# o
podman --version
```

#### Otras herramientas (opcional para módulos avanzados)

- **Azure CLI**: https://docs.microsoft.com/cli/azure/install-azure-cli
- **Terraform**: https://www.terraform.io/downloads
- **kubectl**: https://kubernetes.io/docs/tasks/tools/
- **istioctl**: https://istio.io/latest/docs/setup/getting-started/#download

### 📖 Cómo usar este taller

1. Cada módulo tiene su propia carpeta con teoría y laboratorio
2. Los laboratorios están numerados secuencialmente
3. Sigue el orden de los módulos para una mejor comprensión
4. El proyecto final integra todos los conceptos aprendidos

### 🔧 Configuración Inicial

```bash
# Clonar el repositorio
git clone <repo-url>
cd microservices-net-2025

# Iniciar servicios de infraestructura (PostgreSQL, Redis, MongoDB)
# Con Docker:
docker-compose up -d

# Con Podman:
podman compose up -d

# Verificar que los contenedores están corriendo
docker ps    # o podman ps

# Restaurar dependencias del ProductService
cd src/Services/ProductService
dotnet restore

# Ejecutar migraciones y servicio
dotnet run
```

**Nota:** Si usas Podman, consulta [`docs/PODMAN-SETUP.md`](./docs/PODMAN-SETUP.md) para instrucciones específicas.

### 🚀 Estado del Proyecto

**Completado:**
- ✅ Estructura base del proyecto
- ✅ ProductService con arquitectura hexagonal completa
- ✅ Integración con PostgreSQL y Entity Framework Core
- ✅ Redis caching implementado
- ✅ Versionamiento de API (v1 y v2)
- ✅ Swagger/OpenAPI configurado
- ✅ Dockerfile para containerización
- ✅ Documentación completa de todos los módulos

**En progreso:**
- ⏳ OrderService y UserService
- ⏳ API Gateway (YARP)
- ⏳ Azure Service Bus integration
- ⏳ Terraform scripts
- ⏳ CI/CD pipelines
- ⏳ Despliegue en AKS
- ⏳ Istio y observabilidad

### 📚 Documentación

Cada módulo tiene su propia documentación completa con **guías paso a paso** en `/docs`:

**Módulos Implementados (con código completo y pasos detallados):**
- 📖 [`MODULO-01-Fundamentos.md`](./docs/MODULO-01-Fundamentos.md) - Teoría y Lab 1 paso a paso
- 📖 [`MODULO-02-Arquitectura-Hexagonal.md`](./docs/MODULO-02-Arquitectura-Hexagonal.md) - DDD y arquitectura paso a paso
- 📖 [`MODULO-03-Versionamiento-API.md`](./docs/MODULO-03-Versionamiento-API.md) - Versionamiento y Swagger paso a paso
- 📖 [`MODULO-04-Persistencia-Datos.md`](./docs/MODULO-04-Persistencia-Datos.md) - PostgreSQL y EF Core paso a paso
- 📖 [`MODULO-05-Redis-Cache.md`](./docs/MODULO-05-Redis-Cache.md) - Caching distribuido paso a paso

**Módulos Documentados (con teoría y guías de implementación):**
- 📝 `MODULO-06-16.md` - Documentación de módulos avanzados (Azure, Terraform, Istio)
- 📖 [`PROYECTO-FINAL.md`](./docs/PROYECTO-FINAL.md) - Guía del proyecto integrador
- 📖 [`GUIA-PASO-A-PASO.md`](./docs/GUIA-PASO-A-PASO.md) - Índice general y guía de uso

### 🎯 Empezar el Taller

1. **Lee la [Guía Paso a Paso](./docs/GUIA-PASO-A-PASO.md)** para entender la estructura
2. **Sigue los módulos en orden** (1 → 2 → 3 → 4 → 5)
3. **Cada módulo incluye:**
   - 🧠 Teoría del concepto
   - 🧪 Laboratorio con pasos numerados
   - ✅ Checklist de verificación
   - 🐛 Solución de problemas
4. **Completa el proyecto final** integrando todos los conceptos

### 📝 Licencia

Este proyecto es parte de un taller educativo.

