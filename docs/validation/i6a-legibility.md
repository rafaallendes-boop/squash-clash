# Validación I6a / T6.1–T6.2

Fuente **75072666956d6529ab120d640c3df6f40391b93d**, **262/262 EditMode y 120/120 PlayMode**, cero fallos/omitidos, XML frescos y sin errores de compilación. RED válidos y descartes documentados en [informe](../reviews/2026-10-24-first-ball-i6a-legibility.md) y [manifiesto](evidence/2026-10-24-i6a-tests.json). QA visual editor revisada, no aceptación H1.

Defaults **0d39e6016bd8**: 41 claves; las nueve adiciones expresas del plan son Immediate, los 32 valores anteriores idénticos y P2 sigue por defecto. Escala visual 1–2,5, radio físico 20 mm intacto. Sin simulaciones/colliders nuevos, motores/spec/escenarios/criterios/umbrales/settings intactos; no T6.3 ni gameplay.

Exports limpios dev/prod RELEASE-CHECK: OK; prod sin DevPanel/puente de prueba física y con ayudas runtime. Dev **0.1.0+7507266**, Release ARM64 firmada, metadata/preset exactos, 115 hashes de fuentes cotejados, perfil vigente hasta 2026-10-28T14:59:31Z comprobado el 24 y herramientas sin cambios. [Build saneada](evidence/2026-10-24-i6a-native-build.json).

**Instalada, arrancada y recorrido funcional confirmado por el PO.** Perfil/firma/fuentes verificados antes de instalar el 24. Respaldo e instalación/apertura automática exitosos; P2, cero colliders visuales y escenas PhysX locales, sin excepciones observadas. Arranque Po-1/config **fed535c0b990** y label/app/DEV corroborados por el agente. Ante clics/scroll rechazados por Duplicación, el PO completó el recorrido físico solicitado (escala/ayudas, predicción, seguimiento/fija y restaurar Po-1 sin guardar) y respondió «HIce los cambios, funcionó»; no se atribuye al agente observación directa de esas ayudas.

El retorno a ajustes iniciales al reabrir es compatible con descartar cambios no guardados: se espera Po-1 guardado, completado con las nuevas claves desde defaults (fija/escala 1/predicción off). El respaldo previo contiene floor restitution=0,60. El PO no identificó el encabezado/hash del último arranque, y la recuperación posterior falló timeout/4016: **sin verificación independiente de ese reinicio ni cotejo posterior de archivos**. No se cambió código ni se reinstaló. [Auditoría funcional](evidence/2026-10-24-i6a-native-functional.json).

**PR #10 lista para Claude, abierta y sin fusionar; entrega sin cierre de I6a.** Sin campaña ni evaluación de rendimiento. Renovar perfil antes de instalar desde el 27 y comprobar siempre vigencia efectiva.

Calibración propia del PO pendiente, Po-1 prueba funcional. H1/legibilidad completa requiere PO y T6.3 posteriores. T7.2 con panel cerrado; campaña P2 con preset no default fuera de configuración del gate; retención antes de T8 y diagnóstico de longitud/prefijo de portapapeles en próxima intervención específica del panel.
