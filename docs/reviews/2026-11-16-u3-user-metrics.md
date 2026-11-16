# U3 — métricas mínimas para usuarios

16 nov 2026 · Codex · `codex/u3-user-metrics`, desde `origin/main` a1fa915, independiente de U1. Sin fusión ni instalación.

Alcance: GUID aleatorio persistente en `persistentDataPath/metrics/install-id.txt`, incluido en cada línea JSONL como installId. Se elige cada línea porque el formato actual tiene un sobre por registro y carece de cabecera. Retención actual intacta; el archivo de identidad no coincide con el patrón de logs. Sin nombre de dispositivo, IDFA ni datos personales.

Eventos: app-session (start/background/foreground/end, duración de fase y acumulada), menu (pantalla/acción), tutorial-step (paso, duración, completo/omitido), match-start y match-end (ID de partido, dificultad/mano capturadas al inicio, resultado, marcador, duración, abandono), pause (player/interruption). Focus y pause del SO se combinan para evitar doble transición. El fin del partido se observa mediante los eventos ya existentes del director; no duplica rally-point. session-start/session-summary existentes pertenecen a PerformanceSession DEV y se conservan.

Solo registro, sin UI nueva ni cambios de gesto, timing, contrato, reloj, física o defaults. Duraciones con tiempo real de la app, separado de SimClock. Misma ruta sin defines en dev/prod. AutoQA no corresponde si se mantiene este alcance.

TDD y compuertas pendientes: RED de identidad y eventos en EditMode; integración PlayMode menú → tutorial → partido → fin, omisión/abandono e interrupción; suites completas EditMode/PlayMode y check-release.

RED: EditMode UserMetricsTests **0/10 por aserción**; PlayMode ZUserMetricsFlowTests **0/3 por aserción**, XML frescos y sin errores CS. Evidencia ignorada `Logs/evidence/u3/events-red-*` y `flow-red-*`. ProjectSettings restaurado tras la salida completa de PlayMode. Implementación de registro y conexión a fuentes existentes en curso; las compuertas siguen pendientes.

GREEN EditMode completo: **747/747**, cero omitidos, XML fresco y sin errores CS. Implementación de eventos conectada; PlayMode completo y check-release pendientes. Duraciones reales incluyen pausas/segundo plano; app-session registra por separado cada fase para poder distinguirlas al analizar.
