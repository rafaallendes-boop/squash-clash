# Validación I5b / T5.3

Regresión final: **253/253 EditMode, 102/102 PlayMode**, sin fallos/omitidos. Cinco grupos RED válidos→GREEN, con XML frescos y compilación sin errores; intentos rechazados descritos en [informe](../reviews/2026-10-23-first-ball-i5b-dev-panel.md) y [manifiesto de fuentes/ejecuciones](evidence/2026-10-23-i5b-tests.json).

Exports limpios dev/prod: RELEASE-CHECK: OK. Prod excluye DevPanel y puente Spike; puente térmico runtime incluido en ambos perfiles. Build dev **0.1.0+11e505f**, Release ARM64 firmada, metadata/preset exactos y perfil vigente verificados antes de instalar. Instalación exitosa; lanzamiento automático rechazado por iPhone bloqueado (Locked), comprobación manual solicitada al PO y pendiente. [Evidencia](evidence/2026-10-23-i5b-native-build.json). No campaña física ni medición de rendimiento en este incremento.

Defaults: solo las claves previstas launcher.repeatSeconds=0 y debug.slowMotion=1; valores previos idénticos. Hash **164b0c21e88b**, backend P2/P1 configurable. Sin cambios de motores, escenarios, medidas/umbrales/criterios, settings, plan o spec. Informe para revisión de Claude y PO; no fusionar I5b ni adelantar T5.4/calibración/gameplay.
