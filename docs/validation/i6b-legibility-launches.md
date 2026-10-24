# Validación I6b / T6.3

Fuente **2109af8**, suites **269/269 EditMode y 130/130 PlayMode**, cero fallos/omitidos, XML frescos y sin CS. Cuatro grupos con RED válido antes de comportamiento: pausa, catálogo, condiciones físicas L1–L6 y selector. Diez ejecuciones válidas y un GREEN fallido excluido, con fuentes/logs/XML cotejados en [manifiesto](evidence/2026-10-24-i6b-tests.json).

L1–L6 completos hasta segundo bote pasan las condiciones numéricas del plan con DefaultPreset y P2; contacto anterior al segundo bote comparado por Step/TimeInStep, sin relajaciones. Inicios estrictamente interiores y frontis como primer plano para gravedad 5/9,81/15. Selector probado con P1/P2, física aplicada/pendiente conservada, figuras colocadas y sin escribir presets. Estela/marcas congelan edades en pausa y durante pasos individuales; al continuar expiran con segundos reales activos. Sombra sin escala sigue experimental sin cambios.

Defaults **0d39e6016bd8**, 41 claves intactas, P2 por defecto/P1 configurable. No cambios de física, host, reloj, escenarios de spike, spec, criterios/umbrales ni settings. Po-1 funcional; PO aceptó P0-calib-1 y cerró Etapa 5, sin promoción a defaults. No H1, Etapa 7, gameplay, campaña ni evaluación de rendimiento.

Exports limpios dev/prod RELEASE-CHECK: OK, fuente limpia 2109af8. Prod sin DevPanel/puente Spike; puente térmico runtime en ambos perfiles. Dev **0.1.0+2109af8** Release ARM64 firmada, firma estricta/metadata/preset y 119 hashes verificados; perfil vigente hasta 2026-10-28T14:59:31Z comprobado antes de instalar el 24. Respaldo e instalación exitosos. Proceso automático pasó a segundo plano antes del ready gestionado; arranque funcional y L1–L6 con ambas cámaras/pausa pendientes del PO. Duplicación exige autenticación macOS: solicitado teléfono directo. [Build](evidence/2026-10-24-i6b-native-build.json), [auditoría pendiente](evidence/2026-10-24-i6b-native-functional.json). PR #11 borrador. Renovar perfil antes de instalar desde el 27, verificar siempre vigencia; herramientas intactas.

[Informe](../reviews/2026-10-24-first-ball-i6b-legibility-launches.md). PR para Claude, sin fusionar ni cerrar I6b antes de revisión/decisión del PO.
