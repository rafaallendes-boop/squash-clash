# First Ball E1 — revisión interna de código

Fecha: 2026-10-21. Revisión independiente de lectura, solicitada mediante `requesting-code-review`, sobre el rango `40b22ca..0a7a0af`. No es la revisión de Claude ni sustituye la validación del PO.

## Resultado

Sin hallazgos Critical, Important o Minor que justifiquen cambios antes de integrar E1 en el iPhone. El revisor inspeccionó código, scripts, asmdefs, escena, configuración y evidencia existente; no modificó archivos ni ejecutó Unity, tests o builds, porque el agente principal controlaba el pipeline.

Comprobó la limpieza de exportaciones por perfil, generación/importación síncrona de metadata, guard de producción frente a `SQUASH_DEV` persistente, defines por build, combinaciones Development/IL2CPP, separación de regresión y experimentos, XML fresco y total positivo, y arranque UI Toolkit con versión/perfil/FPS. Confirmó que los ajustes de Physics son solo configuración estática autorizada por T1.2.

Los XML leídos mostraban EditMode 3/3 y PlayMode 2/2; el filtro sin coincidencias mostraba total=0. Las ejecuciones corresponden al agente principal; el revisor no las repitió.

## Límites y resoluciones

| Elemento considerado | Resolución del agente principal |
|---|---|
| Pausa durante vuelo, presets, cambios durante vuelo y reset/predicción PhysX | Fuera de E1 por autorización expresa. Se verifican en sus tareas posteriores; no se añaden interfaces ni tests de sistemas inexistentes. |
| Panel funcional | Todavía no existe. `devpanel=absent` es el resultado esperado en ambos perfiles durante E1. La exclusión del panel real se comprobará cuando se incorpore. |
| Compatibilidad, firma, pantalla y FPS en dispositivo | Se prueban mediante builds e instalación reales; no se infieren de esta revisión. Véase el [informe de E1](../validation/e1-first-installation.md). |
| Cambios generados por primera exportación iOS | Inspeccionados por el agente principal y registrados en `62ffdf3`: componentes de Volume por defecto, recursos runtime/prefiltros URP y entrada de batching iOS. No alteran gameplay. |
| Espacios al final en YAML serializado por Unity | No son fallo funcional; no se reescriben masivamente archivos generados para eliminar ese formato. C# y shell se comprobaron por separado. |
| Estado documental de la última suite | Se actualiza con el informe definitivo de dispositivo; no dejar la suite como pendiente después de verificarla. |
| Remoto, otro Xcode y decisión de física | Permanecen fuera de la autorización. |

Después del rango revisado, la integración encontró una suposición de ruta: Unity genera `SquashClash.app`, no `Squash Clash.app`. Se corrigieron dos rutas en los scripts (`ea0a1e1`) y se ejecutaron `build-ios.sh dev` y `build-ios.sh prod` completos, ambos con código 0, firma válida e instalación/lanzamiento verificados. No cambió el código C# después de la revisión. Las comprobaciones de dispositivo posteriores son del agente principal; no amplían retroactivamente el rango de la revisión independiente.

## Revisión de Claude pendiente

I1 utiliza `main` como bootstrap y no tiene remoto. Claude puede revisar el estado local con `git log --oneline`, `git diff 40b22ca..HEAD`, el plan r2 T1.1–T1.3 y el [informe con evidencia](../validation/e1-first-installation.md). Debe centrarse en scripts, perfiles, metadata y arranque; no exigir funcionalidades de E2 o posteriores. El cierre de I1 requiere la revisión de Claude y la validación del PO conforme al flujo acordado.
