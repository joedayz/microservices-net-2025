# Módulo 16 – Observabilidad y Service Mesh con Istio

## 🧠 Teoría

### Sidecar

Istio usa sidecar proxy (Envoy):
- Inyectado automáticamente
- Intercepta tráfico
- Sin cambios en código
- Transparente para aplicación

### Telemetría

Istio recopila automáticamente:
- Métricas (Prometheus)
- Traces (Jaeger/Zipkin)
- Logs (fluentd)

### Tracing (Jaeger)

Distributed tracing:
- Seguir requests entre servicios
- Identificar cuellos de botella
- Visualizar flujos

### Logs (Kiali)

Kiali visualiza:
- Topología de servicios
- Flujos de tráfico
- Health checks
- Grafos de servicios

### Métricas (Prometheus)

Prometheus recopila métricas:
- Request rate
- Latency
- Error rate
- Resource usage

### mTLS

Mutual TLS entre servicios:
- Encriptación automática
- Autenticación mutua
- Sin cambios en código

### Traffic Shifting

Distribuir tráfico:
- Canary deployments
- A/B testing
- Rollout gradual

### Canary Release

Desplegar nueva versión gradualmente:
- 10% → 50% → 100%
- Monitorear métricas
- Rollback si problemas

## 🧪 Laboratorio 16

### Objetivo
Instalar Istio en AKS + activar observabilidad:
- Instalar Istio
- Configurar mTLS
- Implementar traffic shifting
- Configurar canary release

### Comandos

**Linux/macOS/Windows (con kubectl instalado):**
```bash
# Instalar Istio
istioctl install --set values.defaultRevision=default

# Habilitar mTLS
kubectl apply -f - <<EOF
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
spec:
  mtls:
    mode: STRICT
EOF
```

**Nota:** Los comandos de `istioctl` y `kubectl` funcionan de manera idéntica en Linux, macOS y Windows. Asegúrate de tener instaladas ambas herramientas en tu sistema.

### Próximos pasos

Ver documentación oficial de Istio.

