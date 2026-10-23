# I5a — validación de T5.1–T5.2

**Entrega completa para revisión de Claude**, rama firstball/i5a-tuning-core, [PR #8](https://github.com/rafaallendes-boop/squash-clash/pull/8), incremento abierto y sin fusionar. [Informe](../reviews/2026-10-23-first-ball-i5a-tuning-core.md), [TDD y regresión](evidence/2026-10-23-i5a-tuning-core-tests.json).

- Nueve ciclos de comportamiento RED válido→GREEN; **250/250 EditMode y 92/92 PlayMode**, cero fallos/omitidos, XML frescos y sin errores CS.
- TuningState inmediato/pendiente, guardado de Visible, Overlay sobre Defaults; Store atómico, selección activa, corrupción y directorio de solo lectura real en macOS.
- Histograma de sesión y ventana exacta, JSONL con trazabilidad y errores de E/S, CPU/GPU opcionales/deduplicados, térmico actual/peor/tiempos.
- Arranque e integración con segmentos de 10 s, fase/semilla y descartes; controles sintéticos, sin atribuir rendimiento del iPhone.
- Pausa manual preservada tras capa de sistema; 30 casos del host y dos de consumidores de configuración cubren P1/P2. Guard de inyección antes de Reset, incluyendo planos float exactos y rechazo del float inmediatamente exterior, sin alterar motores.
- DefaultPreset.json intacto, P2 default y P1 configurable, hash **f6a394a12daf**. Core/Simulation, Runtime/Physics, Core/Spike, escenarios, ProjectSettings, plan y spec intactos.

[Build y exports verificados](evidence/2026-10-23-i5a-native-build.json): **0.1.0+7ce86f6 dev**, árbol limpio, exports limpios dev/prod, no Development, IL2CPP/Xcode Release ARM64, firma estricta y metadata/preset embebidos exactos. Puente térmico runtime de T5.2 presente en ambos perfiles, símbolo local y wrapper directo verificados; puente Spike/DevPanel excluido de prod. Xcode no encontró el destino físico; compilación dev exitosa con generic/platform=iOS. Prod exportado, sin compilación Xcode adicional. **Sin instalación, nueva campaña ni mediciones de dispositivo.** Desde el 27 de octubre debe renovarse el perfil antes de instalar; vigencia efectiva siempre comprobada. No se cambian herramientas.

**Se detiene al entregar informe/PR para Claude.** No cerrar/fusionar I5a ni comenzar I5b/T5.3; spec y aceptación integral permanecen para sus gates posteriores.
