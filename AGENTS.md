# Reglas de trabajo

- Fuente de verdad: `docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md` (r2), spec v0.3, revisiones y decisiones registradas del PO.
- Trabajar solo en el incremento autorizado. **E1 / I1 (T1.1–T1.3) cerrado el 2026-10-21**. El PO autorizó después **T1.4** (crear y publicar `squash-clash` privado y verificar su visibilidad) e **I2 / T2.1–T2.3** (geometría, configuración, cancha visual y cámara fija).
- I2 se implementa en `firstball/i2-court`, con PR hacia `main` para revisión de Claude y punto de control del PO en el iPhone. No fusionar antes de esas validaciones. **T3.1 y etapas posteriores no están autorizadas.**
- I1 se hace en `main` como excepción de bootstrap. Commits pequeños y coherentes; Claude revisa el commit local. Desde I2, incrementos en ramas y PR según el plan.
- No modificar `docs/superpowers/specs/`. No cambiar umbrales, escenarios ni decisiones deportivas para hacer pasar pruebas. Reportar discrepancias y registrar las aclaraciones autorizadas.
- TDD para comportamiento de producto con `scripts/test.sh editmode|playmode [filtro]`; comprobar el fallo antes de implementar y la suite verde después. Los experimentos de física no son regresión ni criterio de CI.
- No declarar éxito con cero tests, XML antiguo, errores de compilación o artefactos de build previos. Registrar comandos, resultados y limitaciones.
- La configuración generada y los scripts de bootstrap se verifican con las comprobaciones e integración del plan; evitar pruebas que solo repliquen la implementación.
- En I2 implementar solo T2.1–T2.3: cancha visual sin colisionadores, clasificador geométrico, configuración/preset base, cámara fija y cápsulas de referencia. No implementar pelota, reloj, simulación, predictor, reglas de punto ni sistemas posteriores.
- `dev`: `SQUASH_DEV`, Player no Development, IL2CPP/Xcode Release. `prod`: sin define, no Development, Release. `debug`: Development y Debug. Pasar defines mediante `extraScriptingDefines`, sin mutar los persistentes.
- El remoto privado `squash-clash` y la PR de I2 están autorizados. No cambiar su visibilidad, instalar otro Xcode, elegir backend de física, promover presets calibrados ni modificar criterios sin aprobación del PO. El ajuste de defaults `camera.*` para el encuadre está permitido por T2.3 y debe documentarse en la PR.
- No versionar `scripts/device.env`, credenciales, identificadores personales del iPhone, logs, cachés o builds. No exportar claves privadas.
- Documentación en español; identificadores, código y mensajes de commit en inglés. Citar fuentes oficiales para decisiones técnicas externas.
