# I5a — validación de T5.1–T5.2

Rama firstball/i5a-tuning-core, incremento abierto para revisión de Claude. [Informe](../reviews/2026-10-23-first-ball-i5a-tuning-core.md), [TDD y regresión](evidence/2026-10-23-i5a-tuning-core-tests.json).

- Nueve ciclos de comportamiento RED válido→GREEN; **250/250 EditMode y 92/92 PlayMode**, cero fallos/omitidos, XML frescos y sin errores CS.
- TuningState inmediato/pendiente, guardado de Visible, Overlay sobre Defaults; Store atómico, selección activa, corrupción y directorio de solo lectura real en macOS.
- Histograma de sesión y ventana exacta, JSONL con trazabilidad y errores de E/S, CPU/GPU opcionales/deduplicados, térmico actual/peor/tiempos.
- Arranque e integración con segmentos de 10 s, fase/semilla y descartes; controles sintéticos, sin atribuir rendimiento del iPhone.
- Pausa manual preservada tras capa de sistema; 30 casos del host y dos de consumidores de configuración cubren P1/P2. Guard de inyección antes de Reset, incluyendo planos float exactos y rechazo del float inmediatamente exterior, sin alterar motores.
- DefaultPreset.json intacto, P2 default y P1 configurable, hash **f6a394a12daf**. Core/Simulation, Runtime/Physics, Core/Spike, escenarios, ProjectSettings, plan y spec intactos.

Exports dev/prod y compilación iOS: pendientes de completar. Puente térmico runtime de T5.2 previsto en ambos perfiles; puente Spike/DevPanel excluido de prod. Sin instalación ni nueva campaña. Desde el 27 de octubre debe renovarse el perfil antes de instalar; vigencia efectiva siempre comprobada. No se cambian herramientas.

Detenerse al entregar informe/PR para Claude. No cerrar/fusionar I5a ni comenzar I5b/T5.3; spec y aceptación integral permanecen para sus gates posteriores.
