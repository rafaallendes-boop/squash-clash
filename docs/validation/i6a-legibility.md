# Validación I6a / T6.1–T6.2

Fuente **75072666956d6529ab120d640c3df6f40391b93d**, **262/262 EditMode y 120/120 PlayMode**, cero fallos/omitidos, XML frescos y sin errores de compilación. RED válidos y descartes documentados en [informe](../reviews/2026-10-24-first-ball-i6a-legibility.md) y [manifiesto](evidence/2026-10-24-i6a-tests.json). QA visual editor revisada, no aceptación H1.

Defaults **0d39e6016bd8**: 41 claves; las nueve adiciones expresas del plan son Immediate, los 32 valores anteriores idénticos y P2 sigue por defecto. Escala visual 1–2,5, radio físico 20 mm intacto. Sin simulaciones/colliders nuevos, motores/spec/escenarios/criterios/umbrales/settings intactos; no T6.3 ni gameplay.

Exports limpios dev/prod RELEASE-CHECK: OK; prod sin DevPanel/puente de prueba física y con ayudas runtime. Dev **0.1.0+7507266**, Release ARM64 firmada, metadata/preset exactos, 115 hashes de fuentes cotejados, perfil vigente hasta 2026-10-28T14:59:31Z comprobado el 24 y herramientas sin cambios. [Build saneada](evidence/2026-10-24-i6a-native-build.json).

**Verificación nativa pendiente.** Respaldo e instalación fallaron con CoreDeviceError 4016 por conectividad/servicios no disponibles. Duplicación sin ventana accesible (cgWindowNotFound). Se solicitó cable/desbloqueo; no se declara instalada ni comprobada la app nueva. Renovar perfil antes de instalar desde el 27, comprobar vigencia efectiva de nuevo si se retoma después. Sin campaña física ni evaluación de rendimiento; PR #10 sigue borrador hasta completar y entregar informe.

Calibración propia del PO pendiente, Po-1 prueba funcional. H1/legibilidad completa requiere PO y T6.3 posteriores. T7.2 con panel cerrado; campaña P2 con preset no default fuera de configuración del gate; retención antes de T8 y diagnóstico de longitud/prefijo de portapapeles en próxima intervención específica del panel.
