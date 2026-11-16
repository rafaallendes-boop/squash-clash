# C1 — Tres ajustes de producto (16 nov 2026)

**Estado: RED registrado y continuación autorizada; implementación y compuertas pendientes.** Rama local `codex/product-tweaks`, desde `main` `22bb2d5`. Se leyó `AGENTS.md`; una sola instancia de Unity y ningún acceso al iPhone.

## Alcance y decisión del PO

Encargo del 16 nov: «Ocultar línea M2-4 en prod», «Recordar la dificultad», «Diestro fija la derecha». Se mantienen la tarjeta de pausa por interrupción y los interruptores independientes de sonido y música. Sin autorización de fusión, build nativa ni instalación.

## Cambios preparados

Solo pruebas aditivas en `ZFirstRunMenuTests` y `ZHandednessTests`, y soporte de preferencias de dificultad en `ZFirstRunFixture`. Se conservan todas las aserciones preexistentes. Ningún archivo de producto, Core, física, defaults, reloj, contrato, spec, escenario o criterio fue modificado. No se tocaron presets.

## RED observado y motivo de detención

Primera corrida filtrada con `scripts/test.sh playmode`: XML fresco, **4 ejecutados, 0 aprobados, 4 fallidos, 0 omitidos**, salida **1**. Evidencia resumida: [resultados](../validation/2026-11-16-product-tweaks-red.json). XML y log originales permanecen locales en `Logs/playmode-results.xml` y `Logs/playmode.log`, sin versionar.

| Ajuste / prueba | Resultado |
|---|---|
| Guardar dificultad | **RED válido por aserción**: al elegir Fácil, PlayerPrefs devuelve −1 en vez de 0. |
| Diestro fijo | **RED válido por aserción**: se observaron golpes a ambos lados y cambio de mano. |
| Ocultar línea técnica | **No válido como RED del ajuste**: antes de comprobar `display`, la prueba exige texto «M2-4», pero la fixture muestra «Apuntando · P 0% ». |
| Dificultad inválida | **No aprobado**: error en TearDown, `Invalid index 36 36 0`, por limpieza duplicada en la nueva prueba. |

Los dos últimos problemas son errores de preparación de las pruebas de Codex. Se registran y se detiene el trabajo conforme a §2 de `AGENTS.md`, sin corregir ni repetir para obtener Pass. No se declara GREEN ni se infiere éxito de las aserciones que no llegaron a ejecutarse.

**Reanudación autorizada por el PO (16 nov):** «Corrige solo la preparación de tus dos pruebas nuevas». Se retiró la exigencia del texto «M2-4» y la limpieza pasó a ejecutarse solo entre arranques, dejando el último al TearDown. No se cambiaron las aserciones de dificultad inválida ni ninguna prueba preexistente. Nueva corrida filtrada: **2 ejecutados, 1 aprobado, 1 fallido, 0 omitidos**, salida **1**. Visibilidad: **RED válido por aserción**, `display` esperado `None`, observado `Flex`. Dificultad inválida: **GREEN previo a implementar** en los cuatro valores; el producto actual ya arranca siempre en Media. Hay tres RED de producto válidos (uno por ajuste), pero no se puede declarar RED del caso inválido. El PO autorizó continuar: «Registra “dificultad inválida → Media” como guarda que ya pasa antes de implementar, no como RED». No se fabricó un RED. La corrida confirma que ya no hay errores de TearDown.

## Compuertas y pendiente

`ProjectSettings` restaurado con `git checkout -- ProjectSettings` tras PlayMode. Suites completas EditMode/PlayMode, AutoQA limpio PASS/0/nivel 1 y `scripts/check-release.sh`: **no ejecutados**. No hubo exportación, compilación nativa, instalación ni PR; las compuertas previas a la PR siguen pendientes.

Quedan implementación, GREEN y compuertas, autorizados por el PO tras aceptar la guarda que ya pasa. En el iPhone quedará probar visibilidad dev/prod, dificultad tras cerrar y abrir, y mano derecha en golpes de ambos lados; cualquier instalación requiere autorización propia. Revisión independiente de Claude coordinada por el PO y aprobación de fusión pendientes.
