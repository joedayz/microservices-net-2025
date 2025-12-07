# Módulo 14 – DevOps con CI/CD

## 🧠 Teoría

### GitHub Actions o Azure DevOps Pipelines

**GitHub Actions:**
- Integrado con GitHub
- YAML-based
- Marketplace de acciones
- Gratis para repos públicos

**Azure DevOps Pipelines:**
- Integración con Azure
- YAML o UI
- Release management
- Más features empresariales

### Build & Deploy automático

Pipeline típico:
1. **Build**: Compilar código
2. **Test**: Ejecutar tests
3. **Package**: Crear artefactos
4. **Deploy**: Desplegar a ambiente

### SonarCloud

Análisis de código estático:
- Code smells
- Vulnerabilidades
- Code coverage
- Quality gates

## 🧪 Laboratorio 14

### Objetivo
Pipeline CI/CD – build + test + deploy AKS:
- Configurar GitHub Actions
- Build y test automático
- Deploy a AKS
- SonarCloud integration

### Estructura

```
.github/
└── workflows/
    └── ci-cd.yml
```

### Próximos pasos

Ver documentación oficial de GitHub Actions y Azure DevOps.

