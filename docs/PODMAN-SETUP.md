# Configuración con Podman

Este proyecto puede ejecutarse con **Podman** en lugar de Docker. Las instrucciones son similares pero con comandos ligeramente diferentes.

## Iniciar Servicios con Podman

```bash
# Desde la raíz del proyecto
cd microservices-net-2025

# Iniciar todos los servicios
podman compose up -d

# O iniciar servicios específicos
podman compose up -d postgres redis

# Verificar que están corriendo
podman ps

# Ver logs
podman compose logs postgres
podman compose logs redis

# Detener servicios
podman compose down

# Detener y eliminar volúmenes
podman compose down -v
```

## Diferencias con Docker

- Usa `podman compose` en lugar de `docker-compose`
- Los comandos son idénticos después de eso
- Podman no requiere daemon corriendo (rootless por defecto)

## Solución de Problemas

**Error: "podman compose: command not found"**
- Instalar podman-compose: `brew install podman-compose` (macOS)
- O usar: `podman-compose` (con guión)

**Puerto ya en uso:**
- Verificar qué está usando el puerto: `lsof -i :5432`
- Detener contenedores existentes: `podman stop $(podman ps -q)`

**Permisos:**
- Podman generalmente funciona sin root
- Si hay problemas, verificar configuración de Podman

## Verificar Conexión

```bash
# Probar PostgreSQL
podman exec -it microservices-postgres psql -U postgres -d microservices_db

# Probar Redis
podman exec -it microservices-redis redis-cli ping
```

