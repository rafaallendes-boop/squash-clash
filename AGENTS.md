# Reglas de trabajo

- Fuente de verdad: `docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md` (r2), spec v0.3, revisiones y decisiones registradas del PO.
- Trabajar solo en el incremento autorizado. Actualmente: **E1 / I1 (T1.1–T1.3)**. Detenerse para validación del PO. T1.4 (remoto GitHub), E2 y etapas posteriores no están autorizadas.
- I1 se hace en `main` como excepción de bootstrap. Commits pequeños y coherentes; Claude revisa el commit local. Desde I2, incrementos en ramas y PR según el plan.
- No modificar `docs/superpowers/specs/`. No cambiar umbrales, escenarios ni decisiones deportivas para hacer pasar pruebas. Reportar discrepancias y registrar las aclaraciones autorizadas.
- TDD para comportamiento de producto con `scripts/test.sh editmode|playmode [filtro]`; comprobar el fallo antes de implementar y la suite verde después. Los experimentos de física no son regresión ni criterio de CI.
- No declarar éxito con cero tests, XML antiguo, errores de compilación o artefactos de build previos. Registrar comandos, resultados y limitaciones.
- La configuración generada y los scripts de bootstrap se verifican con las comprobaciones e integración del plan; evitar pruebas que solo repliquen la implementación.
- No implementar cancha, pelota, simulación ni sistemas posteriores durante E1. Solo la configuración estática de proyecto exigida por T1.2, pantalla inicial y herramientas mínimas de T1.3.
- `dev`: `SQUASH_DEV`, Player no Development, IL2CPP/Xcode Release. `prod`: sin define, no Development, Release. `debug`: Development y Debug. Pasar defines mediante `extraScriptingDefines`, sin mutar los persistentes.
- No crear remoto, instalar otro Xcode, elegir backend de física, promover presets ni modificar criterios sin aprobación del PO.
- No versionar `scripts/device.env`, credenciales, identificadores personales del iPhone, logs, cachés o builds. No exportar claves privadas.
- Documentación en español; identificadores, código y mensajes de commit en inglés. Citar fuentes oficiales para decisiones técnicas externas.
