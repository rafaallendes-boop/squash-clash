# First Shot S1 — gesto y mira

Fecha: 2026-10-25. Autorización: [registro del PO](2026-10-25-first-shot-po-approvals.md). Alcance: Tareas 1.0–1.4 del [plan aprobado](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Rama `firstshot/s1-gesture`, base `9a14f9b`; fuente de producto `b37078f`. **S1 no está aceptado todavía: falta validación física del PO. S2–S4 no ejecutados.**

## Resultado implementado

El gesto mueve una mira en metros sobre el frontis, con −X hacia la derecha de pantalla (AX-1). La altura inicial es 2,2 m; la X inicial es la de la pelota al apoyar, para la intención paralela. La mira cambia de círculo cian a rombo amarillo al confirmar el flick; el rojo indica tin/out o huella sobre esas líneas. La ganancia y la conversión a mm se fijan al apoyar. El número de potencia conserva `P`, velocidad neta, cobertura, tiempos distintos, hueco máximo y motivo conservador. **La pelota sigue el lanzador de First Ball: no hay contacto, solver, resimulación, audio de golpe ni háptica nuevos.**

El primer dedo fija el gesto. Los demás no se promueven. N2 usa el desplazamiento máximo y límites estrictos; N1 exige salir del círculo y volver. UI, pausa, pérdida de foco, cancelación del SO o relanzamiento cancelan el gesto sin inventar Ended. Los cambios de ganancia/potencia/DPI durante el gesto se aplican al siguiente; cámara lenta y cambios del contexto temporal lo invalidan. Se descartan movimientos/Ended sin un nuevo Began aceptado.

`TouchInputAdapter` copia id/fase/posición desde el payload `TouchState` de `InputSystem.onEvent`, conserva `InputEvent.time` en double y asigna secuencia por llegada. No sondea controles, no sintetiza Stationary en Update ni mezcla acciones/EnhancedTouch. `Screen.safeArea` excluye bordes; en iOS se difieren los gestos del borde inferior en runtime, restaurando el estado previo al desactivar el adaptador. No cambia ProjectSettings.

Hay una diferencia entre la descripción de la API y el código del paquete instalado: la API de `disableRedundantEventsMerging` menciona ratón, pero Input System **1.20.1**, `Touchscreen.MergeForward` (líneas 991–1010) también fusiona movimientos consecutivos del mismo dedo/fase/flags; `InputManager` línea 3460 lo ejecuta antes de onEvent. El adaptador desactiva esa fusión en runtime mientras está activo y restaura el valor anterior al desactivarse. La prueba con tres Moved en el mismo frame comprueba que llegan todos. Esto conserva los eventos que entrega Unity; **no demuestra que UIKit entregue todas las muestras del sensor**.

La potencia usa solo la ventana temporal y una muestra real anterior para interpolar su borde; conserva por separado el recorrido completo para evidencia. Un timestamp repetido no Stationary o fuera de orden mantiene la potencia conservadora durante el gesto, aunque salga de la ventana. No recorre toda la historia con cada evento. Los registros se serializan/escriben después de despachar el input, no desde onEvent; las escrituras de validación se activan con los controles de medición.

## TDD y regresión

[Resumen verificable, valores heredados y SHA-256 de XML](../validation/evidence/2026-10-25-s1-technical-checks.json). XML/logs completos preservados localmente en `Logs/evidence/s1-tdd` (ignorados, no versionados).

| Etapa | RED observado | GREEN |
|---|---|---|
| 1.0 preparación | No corresponde | PlayMode 137/137; check-release dev/prod OK; fixture excluido del Player |
| 1.1 gesto | 13 ejecutadas, 13 fallos por aserciones | EditMode completa 300/300 |
| 1.2 mira/potencia/validación/hash | 30 ejecutadas, 23 fallos por aserciones y 7 límites ya correctos | EditMode completa 330/330 |
| 1.3 adaptador/mira | 13 ejecutadas, 12 fallos por aserciones | 13/13 |
| 1.3 integración/protocolo | 9 ejecutadas, 9 fallos por aserciones | 9/9 |
| 1.3 ventana de arrastre largo | 2 ejecutadas, 1 fallo por aserción; conservación del error temporal ya pasaba | Específicas S1 24/24 |
| Regresión final | — | **330/330 EditMode, 161/161 PlayMode**, XML nuevos, sin errores CS |

Los esqueletos compilaron antes de observar cada RED. Un error de variable local CS0136 durante la escritura de 1.2 se corrigió y **no** se contó como RED. La primera regresión PlayMode fue 160/161: una prueba nueva comparaba la representación textual de 100 Hz frente a 99,9999999999962. Se corrigió a comparación numérica con tolerancia de coma flotante; los 137 casos previos habían pasado. La repetición completa dio 161/161.

**Adaptación declarada de pruebas heredadas por R2-3:** `ParamCatalogTests` conserva el valor esperado 41 sobre la proyección de claves First Ball; conserva además la equivalencia del conjunto completo con el catálogo. `EffectiveConfigTests` conserva el hash esperado `0d39e6016bd8` sobre esa proyección, y su aserción de SimulationKey no cambia. Cambian los sujetos de esas dos aserciones para expresar la decisión aprobada de ampliar el conjunto de claves; no se afirma que los archivos de tests previos estén byte a byte intactos. Pruebas nuevas comprueban por separado las 58 claves y el hash global nuevo, así como los 41 valores literales. No se modifican escenarios L1–L6, criterios ni umbrales físicos/de aceptación, ni se ajusta ninguna prueba de arranque para ocultar una regresión. La suite intermedia 328/330 se conserva en evidencia.

Commits: `51d7e5d` preparación, `190b17d` gesto, `66f9050` mira/potencia/configuración, `b37078f` adaptador/integración/medición.

## Defaults y configuración

`DefaultPreset.json` conserva los **41 valores** heredados. Su proyección sigue dando `0d39e6016bd8`; SimulationKey física **`166a96e319c7`**, P2 por defecto. Se añaden **17 claves** de S1, sin anticipar claves de S2–S4: aim (6), power (7), gesture (3), input (1). Hay **58 claves**, hash global **`00a501ad7066`**. Rangos y valores iniciales corresponden a la tabla experimental del plan; no se presentan como calibrados.

El validador rechaza combinaciones incoherentes antes de publicar cambios; una importación valida el candidato completo y lo aplica atómicamente, sin pasar por estados intermedios. No cambia la fuente única de defaults. Un preset antiguo de 41 claves recibe las 17 nuevas desde Default al cargar, conservando su archivo original.

P0-calib-1 sigue sin promoverse a defaults. Archivo guardado SHA-256 `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`, 41 claves, solo `launcher.repeatSeconds=3` frente a defaults heredados. Su hash global extendido esperado es **`9977d9ffd7e5`**, y su proyección histórica **`8de3f2ecb217`**. El respaldo previo a instalar conserva P0-calib-1, Po-1 y last-active. **Arranque nativo comprobado a las 2026-10-25T04:51:39.2415910Z:** build 311cb11 dev, P0-calib-1 / 9977d9ffd7e5, cero pendientes, archivo SHA-256 citado. P0-calib-1, Po-1 y last-active recuperados tras instalar son idénticos byte a byte al respaldo previo. El pendiente de comprobar P0-calib-1 en la próxima apertura queda satisfecho en este arranque, antes de los ajustes temporales de validación.

## Compilación y separación dev/prod

Check-release final **dev/prod OK**, exports frescos desde la fuente probada. Dev contiene Core/Runtime/DevPanel; prod Core/Runtime. `Unity.InputSystem.TestFramework` no aparece en el código de registro IL2CPP de ninguno. El puente térmico de diagnóstico runtime sigue en ambos; el de spike solo en dev. El panel y `ShotOverlay` se excluyen de prod por el asmdef existente; adaptador/mira/gesto son runtime.

Build dev **0.1.0+311cb11**, árbol limpio, Release ARM64 y no Development, compilada/firmada e instalada (exit=0). Antes de instalar se verificaron firma estricta, compatibilidad del perfil con dispositivo/bundle y vigencia hasta **2026-10-28T14:59:31Z**; comprobación **2026-10-25T04:51:18Z**, sin renovación requerida hoy. Metadata/preset embebidos coinciden y **199 archivos** auditados conservan los hashes de las fuentes probadas. El commit de metadata incluye documentación y normalización de whitespace de dos folder meta; la fuente funcional sigue siendo b37078f. No se cambian herramientas ni versiones. No se realiza una nueva campaña H17 ni se reinterpreta su Fail p95 aceptado con desviación. La medición táctil de S1 no es una aceptación de rendimiento.

## Protocolo físico S1

Los controles dev son `2 min`, `Regla 50 mm`, `10 suaves` y `10 fuertes`. No dependen de abrir el panel. Antes de medir, comprobar P0-calib-1 activo y cero pendientes; desactivar temporalmente la repetición para evitar que el lanzamiento invalide un arrastre, aplicar/relanzar y cerrar DEV, sin guardar el preset. Al terminar, recuperar evidencia y restaurar P0-calib-1.

1. `2 min`: el PO apunta durante 120 segundos reales, con un dedo. UI/pausa/foco interrumpen la ventana y se registra `complete=false`; no cuenta como completa. Se informan frecuencia de **eventos entregados**, intervalos p50/p95/máximo, muestras por frame, duplicados/desorden y antigüedad al procesar. No se infiere 120 Hz ni frecuencia del sensor.
2. `Regla 50 mm`: el siguiente gesto recorre una línea recta de 50 mm físicos medidos por el PO. Se guardan recorrido máximo calculado, DPI reportado/fallback, factor mm/px y resolución. `PhysicallyVerified` no se infiere de Screen.dpi: requiere confirmación externa del recorrido y queda consignado en evidencia.
3. `10 suaves`, después `10 fuertes`: los siguientes diez gestos de cada serie se registran con intención declarada y ordinal, incluidos Tap/Cancel/SystemCanceled, sin seleccionar solo los swings que salieron bien. Cada uno conserva PowerResult, salto al fijar la mira y muestras originales con tiempos de evento/recepción. Si se interrumpe una serie se declara; no se sustituye por datos simulados.
4. Preguntas del PO de propuesta §5: ¿apunto donde quiero?, ¿entiendo siempre dónde apunto?, ¿el número responde a suave/fuerte? El juicio y los datos medidos se incorporarán sin inventar respuestas ni ajustar defaults silenciosamente.

Los registros `s1-sampling`/`s1-gesture` llevan metadata habitual de build/preset/hash/semilla, con `metricsValid=false`: son evidencia de input/previsualización, no una campaña de aceptación. Los desconocidos/percentiles desbordados son JSON null, con contadores explícitos. El resumen de 120 s mantiene sus cifras finales aunque la app siga recibiendo eventos; el overlay muestra las cifras vivas.

**Estado físico actual: pendiente.** No hay frecuencia de iPhone, comprobación con regla, veinte resultados de flick ni valoración del PO declarados como medidos todavía.

## Fuentes oficiales

- [Unity Input System 1.20 — eventos](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/input-events.html): timestamps y flujo de eventos.
- [InputEvent.time](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.LowLevel.InputEvent.html#UnityEngine_InputSystem_LowLevel_InputEvent_time) y [StateEvent.GetState](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.LowLevel.StateEvent.html): datos del evento frente al estado de los controles.
- [InputSettings.disableRedundantEventsMerging](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputSettings.html#UnityEngine_InputSystem_InputSettings_disableRedundantEventsMerging); contraste con el código local de 1.20.1 citado arriba.
- [Unity 6000.3 — Screen.dpi](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Screen-dpi.html): puede ser desconocido; no sustituye la comprobación física.
- [Device.deferSystemGesturesMode](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/iOS.Device-deferSystemGesturesMode.html): cambio runtime, sin editar PlayerSettings.
