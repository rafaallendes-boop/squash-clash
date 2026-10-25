# R1 — retención de logs de First Ball

[PR #14](https://github.com/rafaallendes-boop/squash-clash/pull/14): **Claude aprobó b9a5c24 con corrección menor; PO autorizó fusión condicionada**. **R1 cerrado**: PR fusionada con merge commit `bb0f939947991d801afb45b0c0c32005c677b9ea`, head `3ccae954d291c287b98ac68a25133c819777e320`. Main sincronizada y worktree eliminado; ver cierre al final. La entrega inicial y su evidencia se conservan abajo como histórico. Autorización PO del 2026-10-24, rama `firstball/r1-log-retention`. [Aprobación y secuencia](2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24). T7.3 documental autorizado **solo después del cierre R1**, en su rama/PR separadas; no se inicia en esta entrega. Gameplay/T8 siguen sin autorizar.

## Política aplicada

`Core/Diagnostics/LogRetention.cs` contiene las constantes **30 archivos y 50.000.000 bytes (50 MB decimales)**. Cada carpeta se presupuesta por separado. `MetricsLog` llama a la poda una vez en su constructor, después de determinar el path actual y antes de escribir: misma política dev y prod. `SpikeSection.Initialize` la llama para spike, cuyo escritor/controles pertenecen al ensamblado DevPanel excluido de prod. No se crea un servicio periódico ni se poda en Write/Finish.

Se consideran solo nombres completos, con fecha/hora UTC válida e interpretación invariante:

- `metrics-yyyyMMddTHHmmssfff-*.jsonl`, sufijo no vacío.
- `P1-yyyyMMdd-HHmmss-fff.md` y `P2-yyyyMMdd-HHmmss-fff.md`, compartiendo presupuesto spike.

Orden descendente por timestamp UTC del nombre; empate por nombre ordinal ascendente. **mtime no participa**. Se conserva el prefijo más reciente hasta el primer límite que impediría añadir otro archivo. Al alcanzar el presupuesto no se rellenan huecos con archivos antiguos más pequeños. El archivo actual se excluye de borrado y, si existe/encaja en el patrón, cuenta en ambos límites. Si él solo supera 50 MB se conserva y se eliminan los demás elegibles.

Los límites se aplican **a archivos existentes al arrancar**, antes de la escritura: no se crea/reserva un archivo inexistente para la poda. Las escrituras posteriores pueden superar el presupuesto hasta el siguiente arranque (incluido el nuevo archivo de métricas), conforme a la ausencia de rotación/límite dentro de sesión. No se aplica un límite por archivo en curso.

Solo se enumeran archivos de primer nivel de la carpeta indicada. Archivos ajenos, fechas/nombres mal formados, extensiones adicionales/.tmp y carpetas anidadas se ignoran, no cuentan para el presupuesto ni se borran. presets/ y last-active no se recorren ni modifican; los llamadores de producto solo pasan metrics/ o spike/. No cambios de presets, tuning ni defaults.

Errores de enumeración/lectura de tamaño/borrado de E/S, acceso o seguridad no escapan: la poda devuelve el último error a **LastError** de MetricsLog/SpikeSection; continúa lo posible y conserva archivos cuyo tamaño no se pudo leer. Una carpeta todavía inexistente es normal. Spike también registra aviso; sus errores de guardado ahora alimentan LastError. Una escritura posterior exitosa limpia LastError conforme a la semántica existente de MetricsLog.Write. No se inventa éxito de poda ante un error: el presupuesto puede quedar sin satisfacer y el error queda disponible; el arranque no se aborta por ello.

## TDD y regresión

Fuente de producto **f3921bd**. Los 16 casos de R1 se ejecutan en **EditMode con directorios temporales**, archivos grandes dispersos y limpieza en TearDown; la prueba de conexión del spike carga por reflexión su componente de dev y llama Initialize sobre un root UI aislado, sin escena/runner/campaña.

1. RED válido **14 totales / 10 fallos**: todavía no había poda ni conexión en MetricsLog.
2. Política común GREEN: 11/14 pasan, tres RED pendientes de constructor metrics (dev/prod/error).
3. Metrics GREEN: 14/16 pasan; dos RED válidos del inicio spike (poda/protección y LastError).
4. **16/16 GREEN** al conectar spike.

Cobertura: menos de límite/carpeta ausente, >30 y mtime invertido, >50 MB con prefijo, límites exactos inclusivos, actual antiguo protegido/contado, actual >50 MB, ajenos/.tmp/presets/last-active intactos, nombres imposibles/mal formados, P1/P2 juntos, error E/S sin excepción, constructors dev/prod antes de escribir, LastError y ausencia de poda/rotación al escribir durante sesión. XML frescos, conteos positivos y sin errores CS; [evidencia TDD/suites](../validation/evidence/2026-10-24-r1-retention-tests.json).

**Suites completas GREEN: 286/286 EditMode y 136/136 PlayMode**, cero fallos/omitidos y sin errores CS. XML/logs frescos, 123 hashes de fuentes coincidentes con los exports. No hay experimento físico ni campaña nativa en R1; no se mezclan tests con los resultados del gate/H17.

## Release y alcance

**check-release.sh: RELEASE-CHECK OK**, exports limpios dev/prod del commit f3921bd, gitDirty=false, Player no Development e IL2CPP Release. Módulos Core/Runtime en ambos, DevPanel/puente de spike solo dev, puente térmico runtime ambos. Código nativo generado de retención/MetricsLog presente en ambos; metadata perfil/fuente y 123 hashes cotejados. [Evidencia release](../validation/evidence/2026-10-24-r1-retention-release.json). No compilación Xcode, firma, instalación iPhone ni renovación de perfil en R1. La build instalada e0f9071 no se reemplaza. Si una autorización futura exige instalar desde el 27, renovar antes y comprobar siempre vigencia. Unity/Xcode/herramientas sin cambios.

Spec/plan, escenarios, criterios, umbrales, defaults **0d39e6016bd8**, parámetros/catálogo, backend P2/física y datos de campañas anteriores intactos. No se corrigen ahora las otras observaciones dev de Claude I7a ni se ejecuta T7.3.

## Limpieza previa y entrega

Sin PR de limpieza, se verificaron worktrees antiguos **i4c-physics-decision** e **i4d-p2-analytic** limpios y ancestros fusionados de main. Antes de retirarlos se preservaron los artefactos ignorados en **Logs/archive-i4c-physics-decision** (sin artefactos previos, manifest) y **Logs/archive-i4d-p2-analytic** (siete paths de metadata/builds/caché/logs/settings/entorno ignorado). Worktrees eliminados mediante git worktree remove, **ramas remotas intactas**; manifest local no versionado. Caché R1 clonada desde el archivo ignorado I7a sin alterar ese respaldo.

Entrega del informe y PR **lista para Claude, sin fusionar ni cerrar R1**. La política de retención decidida por PO queda implementada para revisión; no se declara cerrada la observación previa a T8 hasta aprobación/cierre. **Detenerse aquí. T7.3 queda pendiente del cierre R1 y de su incremento separado.**

## Corrección tras revisión de Claude — 2026-10-24

Fuente corregida **b8713827a2b065e6ae70724bc8924f2890e90967**, sobre head revisado **b9a5c24dfa0949bf4c1b831caf940d208615f9f1**. `MetricsLog.RetentionError` conserva de forma independiente el error de retención del constructor durante toda la instancia; Write no lo modifica. Se mantiene LastError inicial por compatibilidad y una escritura correcta lo limpia. El constructor emite `Debug.LogWarning("[SquashClash] Retención métricas: …")` si existe RetentionError; Diagnóstico muestra el error de retención junto al de log, incluso después de recuperarse la escritura.

TDD temporal: el destino empieza bloqueado por un archivo, se construye MetricsLog con error, se reemplaza por una carpeta y se escribe un registro correctamente. **RED válido 16/17**: falla la aserción por falta de RetentionError, sin errores CS (reflexión para que la API ausente no sea un RED de compilación). **GREEN 17/17**: warning esperado, registro persistido, LastError null y RetentionError igual al error original. No cambia la política de poda.

**Suites completas frescas: 287/287 EditMode y 136/136 PlayMode**, cero fallos/omitidos y sin errores CS. **check-release.sh dev/prod OK** sobre b871382 limpio: metadata gitDirty=false, exports frescos Release/Player no Development, Core/Runtime ambos y DevPanel/puente spike excluidos de prod. RetentionError exportado en ambos; 123 hashes de fuentes probadas coinciden con los exports. [Evidencia de revisión TDD/suites](../validation/evidence/2026-10-24-r1-retention-review-tests.json), [evidencia de revisión release](../validation/evidence/2026-10-24-r1-retention-review-release.json). Sin instalación, Xcode build ni campaña.

**Condición PO:** comprobar el diff b9a5c24→head final antes de fusionar. Solo se permiten MetricsLog.cs, DiagnosticsSection.cs, LogRetentionTests.cs (o test nuevo de Diagnostics), docs/** y AGENTS.md. La corrección toca exactamente los tres archivos de código indicados, más documentación; no hay test nuevo ni metadatos adicionales. Spec/plan/escenarios/criterios/umbrales/defaults/física intactos, defaults **0d39e6016bd8**. Si cambia otro archivo, no fusionar y detenerse. Cumplidos RED, suites y release, PO autoriza marcar lista y **merge commit**, sincronizar main y eliminar worktree preservando ignorados; registrar merge/head. Solo tras cerrar R1 se inicia T7.3 documental separado y sin fusionar.

### Observaciones de Claude, sin cambio de código

- Si un archivo reciente supera el presupuesto de bytes restante, se borran él y los anteriores: prefijo más nuevo, sin huecos. Cubierto por `ByteLimit_KeepsNewestPrefixAndDoesNotFillHolesWithOlderSmallFiles`; irrelevante con los tamaños actuales, cientos de KB por sesión.
- **30 archivos equivalen aproximadamente a 30 aperturas que escriben métricas. Exportar los logs antes de cualquier campaña de análisis (First Fun).**
- El test dev/prod del constructor solo cambia la etiqueta del perfil. La exclusión real de prod se verifica con check-release, no con esa etiqueta.

## Cierre R1 autorizado por el PO

PR #14 lista y fusionada con **merge commit bb0f939947991d801afb45b0c0c32005c677b9ea**, head **3ccae954d291c287b98ac68a25133c819777e320**. Delta final desde b9a5c24 auditado: tres archivos de código autorizados, AGENTS.md y cuatro docs, ningún otro archivo. SHA-256 diff **189403f97dc2915632aafda436e82723b8866d4555dc4e35e2a9ba8b275cad67**; 123 hashes de fuentes coincidentes con suites/exports. Se cumplen RED, GREEN y release-check requeridos.

Main sincronizada; worktree R1 eliminado sin force, artefactos ignorados (siete paths) preservados en **Logs/archive-r1-20261024**, ramas remotas intactas. Cierre registrado en AGENTS y aprobaciones PO. La entrega inicial pendiente de revisión descrita arriba queda como histórico. **R1 cerrado**, retención previa a T8 satisfecha; T7.3 comienza por separado según autorización y se detendrá con PR lista sin fusionar. Gameplay/T8 sin autorizar.
