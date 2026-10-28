# S1 — protección y trazabilidad de controles, SL-S1-04

Base aprobada técnicamente por Claude:cecd74c7ea24756528bd815065d2ab58a4cde067. [Incidente anterior y restauración](2026-10-27-first-shot-s1-sling-iteration-2.md), [autorización del PO](2026-10-25-first-shot-po-approvals.md). Misma rama/PR25, sin fusionar. La causa de las dos activacionesP2 sigue sin confirmar; no atribuirla alPO ni a autoarranque.

## Alcance autorizado

Registrar controlesDevBar/DEV y precondiciones/resultados de armarB0–B5. BloquearEscenario/P2/H17 desde selección/preparaciónS1-sling y mientras bloque o respuesta activos, con protección también en el callback. Fuera de protocolo conservar funciones. Rechazo deB0–B5 persistente y visible con causa/remedio, sin simular bloque activo; preset/hash/pausa visibles antes deB0. Solo texto opcional deP2. No cambiarD2, sus parámetros, cancelación, defaults, criterios, spec ni plan.

O1/O2 deClaude se conservarán para análisis de datos nuevos, sin alterar modelo: abandonos con candidato quieto≥120ms y<3 muestras; muestras con timeReal<lockObservedAt recibidas después de fijar y desplazamiento desde posición fijada. No recalcular las sesiones históricas como datos nuevos.

## Verificación y sesión previstas

TDD con esqueleto compilable y RED por aserciones antes de implementar; regresión completa y exports dev/prod limpios antes de builddevReleaseARM64/noDevelopment. Respaldar3archivos; comprobar perfil incorporado vigente antes de instalar (actualvence2026-11-03T18:29:44Z). PrepararS1-sling temporal sin guardar, aplicar/relanzar/verificarhash/juego sin pausa y selección persistidaP0. El PO pulsaB0 y confirma cuenta atrás antes del primer gesto. B0completo120s/identificación fijación-zona/cancelación guiada precedeB1–B5/16preguntas. Si no se entiende, detener y registrar. RestaurarP0/nuevo arranque/hash/cero pendientes/tres archivos byte a byte. S1sin aceptar/S2–S4/T8sin autorizar.


## Implementación y verificación antes de exportar

Auditoría `dev-control` con fases ui-activation/activation/result: nombre interno, reloj real, preset/hash aplicado y hash del lanzamiento, pausa/foco/DEV/pending, aceptado y motivo. La fase activation se escribe antes de ejecutar; result distingue aceptación/rechazo. Los campos de edición y despliegue registran cambios/resultado sin copiar valores del portapapeles. No interpretar cada fase como otro intento ni atribuir por estos registros una causa a las activaciones anteriores.

Escenario/physics-spike/performance-session deshabilitados desde selecciónS1-sling (incluso pendiente), bloque o respuesta activos; callback también rechaza antes de cambiar pelota/fase. Funciones fuera de protocolo conservadas yP2 rotulado «Benchmark P2 (dev)» sin cambiar nombre interno. B0–B5 identifica pausa/foco/reanudación/DEV/gesto/control/pendientes/repetición/respuesta/bloque desconocido; aviso persistente con remedio fuera deDEV, sin falso temporizador de inicio. Antes deB0, línea muestra preset/hash/Listo o motivo de indisponibilidad. El núcleoD2 y su registro de gestos no cambian.

RED compilable inicial:9/9 fallos por aserción; GREEN posterior de esos9. Hubo dos referencias de fixture aSpikeSection sobreAppBootstrap, corregidas aDevBar sin cambiar aserciones. RED adicional de geometría de aviso trasB0 interrumpido:9Pass/1Fail por desborde →10/10. Regresión fresca **411/411 EditMode y194/194 PlayMode**, sinCS. Aviso contenido y fuera del frontis en1206×2622; geometría anterior, indicador y temporizador conservados. [Evidencia automatizada](../validation/evidence/2026-10-27-s1-sl04-controls-implementation.json). XML/logs privados separados enLogs/evidence/s1-sl04-controls.

TransitoriosGraphicsSettings/QualitySettings dePlayMode preservados como patch ignorado y restaurados antes de exportar;35 archivos protegidos cotejados iguales, incluidos modeloD2/settings/indicador/defaults/preset/spec/planes. Ningún criterio, umbral, banda o valor nuevo. Pendientes exports/check-release limpios, builddev/perfil/cotejo y sesión nueva según gate. Respaldo previo deP0/Po-1/last-active ya recuperado y coincidente con el anterior; no instalación nueva todavía.


## Build e instalación — preparación física pendiente

Producto **4cec622b76811643c5cb6ce7a0299b3fa61e678a**, fuente limpia. Exports/check-release dev yprod frescos,gitDirty=false y módulos correctos; framework de pruebas InputSystem excluido de ambos Players. Builddev **ReleaseARM64/noDevelopment**,444 archivos fuente/config/scripts cotejados sin cambios. No se repiten las suites porque no hubo cambios de producto después de la regresión.

Perfil incorporado comprobado **2026-10-28T01:40:49.771614Z (27octubre local)** antes de instalar: creado2026-10-27T18:29:44Z/vence2026-11-03T18:29:44Z, renovación previa satisfecha, vigencia efectiva/firma/certificado/dispositivo/equipo/bundle correctos. Sin exportación de claves ni perfiles versionados. Instalación completada después de esa comprobación; **P0-calib-1,Po-1,last-active directamente byte a byte iguales al respaldo**. Selección persistidaP0. Datos de comprobación sin identificadores personales en la evidencia enlazada.

El intento inicial de abrir la build fue rechazado por dispositivo bloqueado; Duplicación pide autenticación local, solicitada alPO. **Arranque4cec622/P0, preparación temporalS1-sling7d1a61c39766/cero pendientes/sin pausa y traspaso deB0 todavía pendientes.** No usar los últimos lanzamientos42265ca del contenedor como arranque de la versión nueva. No se ha iniciado sesión ni generado datos físicos nuevos de4cec622; O1/O2 por evaluar cuando existan. PR#25 en borrador/sin fusionar mientras sigue la entrega. El PO debe confirmar cuenta atrás antes del primer gesto; no omitir esa frontera al continuar.


## Continuación del 28 de octubre — P0 verificado, preparación interrumpida

Tras la respuestaPO «listo!», nuevo arranque porCLI completado: **2026-10-28T09:57:01.3934910Z**,4cec622 dev/P0-calib-1/**e458bfd5ce9e**/cero pendientes; pantalla normal con botónPausa y sin informe. Los tres archivos directamente byte a byte iguales al respaldo,444 fuentes intactas. [Evidencia nueva de preparación](../validation/evidence/2026-10-28-s1-sl04-preparation.json).

Se abrióDEV y la pestañaPresets. La auditoría nativa contiene esas dos activaciones con sus fases activation/result, tiempos reales, preset/hash/pausa/foco/DEV; **no son cuatro intentos**. Cero activacionesP2/H17/Escenario, bloques, muestreo o gestos de4cec622. S1-sling todavía no cargado, sin cambios en selecciónP0 ni pendientes.

Durante el desplazamiento del panel, la herramienta perdió el proceso deDuplicación (procNotFound); al reabrirla,macOS pidió nuevamente contraseña local. No se establece la causa de esa pérdida ni se atribuye a la app de squash. Se solicitó alPO reautenticar allí, sin compartir la contraseña. **Preparación temporalS1-sling y confirmación de cuenta atrás aún pendientes; no pedir gestos ni considerar B0 intentado.** No nuevas suites/build/instalación/código/ajustes para esta continuación. PR#25 en borrador/sin fusionar; S1sin aceptar/S2–S4/T8sin autorizar.


## S1-sling preparado y traspaso al PO — 28 de octubre

Tras nueva autenticación, se reanudó la app en el punto anterior; se navegó el panel con sus flechas visibles. S1-sling existente coincide byte a byte con el archivo de la rama. Cargado sin guardar; **last-active originalP0 repuesto antes de aplicar/relanzar**. Lanzamiento verificado **2026-10-28T10:06:22.0058220Z**,4cec622 dev/**S1-sling/7d1a61c39766/cero pendientes/repeat0**. Auditoría registra preset-load y apply-relaunch con fases y estado de pausafalse. P0/Po-1/last-active directamente iguales al respaldo;444 fuentes intactas.

Estado observado antes de entregar: **«S1-sling · 7d1a61c39766 · Listo»**, botónPausa, DEVcerrado yEscenario/BenchmarkP2/H17 deshabilitados. FilaB0 dejada visible enParámetros para su próxima apertura, **sin pulsarla**. Cero bloques, muestreo y gestos4cec622 hasta la captura prepared-S1-ready. La vista deDuplicación se amplió solo para lectura, sin cambiar cámara/parámetros ni medir píxeles desde esa vista. [Evidencia de preparación actualizada](../validation/evidence/2026-10-28-s1-sl04-preparation.json).

Se pidió alPO desbloquear/continuar si aparece aviso, comprobarbuild/preset/hash, abrirDEV→B0 y **confirmar aquí desde el Mac la cuenta atrás antes del primer gesto**, para no salir de la app del iPhone. Tras enviar esa confirmación puede practicar con la izquierda sin esperar otra respuesta: apuntar/pausar/fijar/tensar/reducirP/cancelar siguiendoDENTRO ysoltar, hastaCompletado sinDEV/salir. Si no ve o no entiende el límite, detener; noB1. Confirmación/120s/identificación/cancelación guiada todavía pendientes, no aceptación inferida. No interactuar con el teléfono mientras elPO practica; el estado privado conserva esa frontera.


## Repreparación solicitada por el PO tras salir — 28 de octubre

Literal delPO:

> tuve que salir por lo que se reinició. Dejé duplicar abierto para que hagas toda la configuración nuevamente y haga la prueba .

Captura previa: P0/e458bfd5ce9e/cero pendientes desde11:50:21.106494Z; **no se había armadoB0 ni hay bloques/gestos/muestreo4cec622**, por lo que no existe una práctica completada ni un bloque interrumpido que evaluar. Se conserva el registro previo, sin sustituir intentos. No es defecto por volver al preset persistido al reiniciar.

Nueva carga deS1-sling desdeDEV, last-activeP0 original repuesto **antes** de aplicar/relanzar. Lanzamiento **2026-10-28T11:53:11.3744040Z**,4cec622 dev/S1-sling/**7d1a61c39766/cero pendientes/repeat0**. Pausafalse en auditoría, DEVcerrado/líneaListo/botónPausa/3experimentos deshabilitados/B0accesible, sin activarlo desdeCodex. Tres archivos directamente byte a byte iguales al respaldo y444fuentes sin cambios. Captura re-prepared-S1-ready e historial añadido a la [evidencia de preparación](../validation/evidence/2026-10-28-s1-sl04-preparation.json). Sin código/suite/build/instalación nuevos. Se mantiene la confirmaciónPO de cuenta atrás antes delprimer gesto, guía/cancelación/B0completo antes deB1; S1sin aceptar, PRsin fusionar/S2–S4/T8sin autorizar.
