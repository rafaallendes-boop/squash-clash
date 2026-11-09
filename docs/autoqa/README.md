# AutoQA MVP — estado de la POC

La implementación está detenida por **AUTOQA-MVP-02**, diferencia de Scheduled entre las tres repeticiones de la POC SL5. **El MVP no está disponible ni aprobado.** No existe todavía `scripts/autoqa.sh`, el catálogo, el oráculo, la guardia o los artefactos `autoqa-summary/1` y `autoqa-attempt/1`.

El [informe de continuación](../reviews/2026-11-08-autoqa-mvp.md) contiene la resolución de AUTOQA-MVP-01, hashes/márgenes declarados, RED/GREEN acotado, tabla de propuestas de alineación, evidencia primaria y parada. El [estudio aprobado](../reviews/2026-11-06-autoqa-technical-study.md) se conserva intacto; las [decisiones posteriores del PO](../reviews/2026-10-25-first-shot-po-approvals.md#continuación-del-mvp-de-autoqa--decisiones-del-po-2026-11-08-chile) prevalecen donde lo precisan.

La POC usa Main real, GameView 1206×2622, filtros reales y toques virtuales. Sus archivos están en `Assets/SquashClash/Tests/AutoQA/`. Es Editor determinista con bucle orquestado, no ejecución nativa ni medición de percepción. Sus instrucciones y evidencia están en el informe. No reintentar para buscar un resultado favorable ni continuar con el MVP antes de resolver la parada con el PO.

Las propuestas compatibles de Claude se aceptan documentalmente como futuro contrato: `counts`, `canary.observed`, lista `gameMetrics` con `{path,sha256}`, `executionError.kind`, líneas `RUN_DIR=`/`RESULT=`, `gate:` en informe, gate siempre definido, margen inválido `NOT_EVALUATED`/`scenario-invalid-margin` y precedencia de salida `3 > 4 > 1 > 2`. Su implementación y TDD están pendientes. La plantilla única del operador y `OPERATIONS.md` también están pendientes; no se modifica la rama operadora #37 ni se publica una segunda plantilla.

El hash inesperado será integridad/nivel 3/salida 3. El canario ejecutado que no da FAIL será nivel 3; un canario ausente porque la corrida no llegó a ejecutarse por error no constituye por sí solo integridad. Esta documentación no sustituye una regla de triaje ejecutable.

La reversión de esta POC consiste en revertir el commit `bfe1474` que añade el ensamblado y fixture; no tiene migraciones ni cambios de producto o presets. No verifica digitalizador, UIKit, IL2CPP, audibilidad, háptica percibida, visual, latencia toque→imagen, sensación ni diversión.
