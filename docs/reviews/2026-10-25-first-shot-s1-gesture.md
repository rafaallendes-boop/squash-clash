# First Shot S1 — gesto y mira

Fecha: 2026-10-25. Autorización: [registro del PO](2026-10-25-first-shot-po-approvals.md). Alcance: Tareas 1.0–1.4 del [plan aprobado](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Rama `firstshot/s1-gesture`, base `9a14f9b`; fuente inicial `b37078f`, corrección final de producto `340ca2c`. **Entrega técnica para revisión de Claude, con evidencia nativa y limitaciones de validación explícitas. No hay aceptación del control de S1 inferida del PO. S2–S4 no ejecutados.**

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
| Regresión inicial | — | 330/330 EditMode, 161/161 PlayMode, XML nuevos, sin errores CS |
| Corrección de borde de ventana | 6 ejecutadas, 5 fallos por aserciones; una salvaguarda ya pasaba | **336/336 EditMode, 161/161 PlayMode**, XML nuevos, sin errores CS |

Los esqueletos compilaron antes de observar cada RED. Un error de variable local CS0136 durante la escritura de 1.2 se corrigió y **no** se contó como RED. La primera regresión PlayMode fue 160/161: una prueba nueva comparaba la representación textual de 100 Hz frente a 99,9999999999962. Se corrigió a comparación numérica con tolerancia de coma flotante; los 137 casos previos habían pasado. La repetición completa dio 161/161.

**Adaptación declarada de pruebas heredadas por R2-3:** `ParamCatalogTests` conserva el valor esperado 41 sobre la proyección de claves First Ball; conserva además la equivalencia del conjunto completo con el catálogo. `EffectiveConfigTests` conserva el hash esperado `0d39e6016bd8` sobre esa proyección, y su aserción de SimulationKey no cambia. Cambian los sujetos de esas dos aserciones para expresar la decisión aprobada de ampliar el conjunto de claves; no se afirma que los archivos de tests previos estén byte a byte intactos. Pruebas nuevas comprueban por separado las 58 claves y el hash global nuevo, así como los 41 valores literales. No se modifican escenarios L1–L6, criterios ni umbrales físicos/de aceptación, ni se ajusta ninguna prueba de arranque para ocultar una regresión. La suite intermedia 328/330 se conserva en evidencia.

Commits: `51d7e5d` preparación, `190b17d` gesto, `66f9050` mira/potencia/configuración, `b37078f` adaptador/integración/medición; `340ca2c` corrección de bordes y reproducción de tres trazas nativas.

## Defaults y configuración

`DefaultPreset.json` conserva los **41 valores** heredados. Su proyección sigue dando `0d39e6016bd8`; SimulationKey física **`166a96e319c7`**, P2 por defecto. Se añaden **17 claves** de S1, sin anticipar claves de S2–S4: aim (6), power (7), gesture (3), input (1). Hay **58 claves**, hash global **`00a501ad7066`**. Rangos y valores iniciales corresponden a la tabla experimental del plan; no se presentan como calibrados.

El validador rechaza combinaciones incoherentes antes de publicar cambios; una importación valida el candidato completo y lo aplica atómicamente, sin pasar por estados intermedios. No cambia la fuente única de defaults. Un preset antiguo de 41 claves recibe las 17 nuevas desde Default al cargar, conservando su archivo original.

P0-calib-1 sigue sin promoverse a defaults. Archivo guardado SHA-256 `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`, 41 claves, solo `launcher.repeatSeconds=3` frente a defaults heredados. Su hash global extendido esperado es **`9977d9ffd7e5`**, y su proyección histórica **`8de3f2ecb217`**. El respaldo previo a instalar conserva P0-calib-1, Po-1 y last-active. **Arranque nativo comprobado a las 2026-10-25T04:51:39.2415910Z:** build 311cb11 dev, P0-calib-1 / 9977d9ffd7e5, cero pendientes, archivo SHA-256 citado. P0-calib-1, Po-1 y last-active recuperados tras instalar son idénticos byte a byte al respaldo previo. El pendiente de comprobar P0-calib-1 en la próxima apertura queda satisfecho en este arranque, antes de los ajustes temporales de validación.

## Compilación y separación dev/prod

Check-release final **dev/prod OK**, exports frescos desde la fuente probada. Dev contiene Core/Runtime/DevPanel; prod Core/Runtime. `Unity.InputSystem.TestFramework` no aparece en el código de registro IL2CPP de ninguno. El puente térmico de diagnóstico runtime sigue en ambos; el de spike solo en dev. El panel y `ShotOverlay` se excluyen de prod por el asmdef existente; adaptador/mira/gesto son runtime.

**Build de adquisición anterior a la corrección:** dev **0.1.0+311cb11**, árbol limpio, Release ARM64 y no Development, compilada/firmada e instalada (exit=0). Antes de instalar se verificaron firma estricta, compatibilidad del perfil con dispositivo/bundle y vigencia hasta **2026-10-28T14:59:31Z**; comprobación **2026-10-25T04:51:18Z**, sin renovación requerida hoy. Metadata/preset embebidos coinciden y **199 archivos** auditados conservan los hashes de las fuentes probadas. El commit de metadata incluye documentación y normalización de whitespace de dos folder meta; la fuente funcional de esa adquisición es b37078f. No se cambian herramientas ni versiones. No se realiza una nueva campaña H17 ni se reinterpreta su Fail p95 aceptado con desviación. La medición táctil de S1 no es una aceptación de rendimiento.

## Protocolo físico S1

Los controles dev son `2 min`, `Regla 50 mm`, `10 suaves` y `10 fuertes`. No dependen de abrir el panel. Antes de medir, comprobar P0-calib-1 activo y cero pendientes; desactivar temporalmente la repetición para evitar que el lanzamiento invalide un arrastre, aplicar/relanzar y cerrar DEV, sin guardar el preset. Al terminar, recuperar evidencia y restaurar P0-calib-1.

1. `2 min`: el PO apunta durante 120 segundos reales, con un dedo. Abrir DEV/pausa/foco interrumpen la ventana y se registra `complete=false`; no cuenta como completa. Se informan frecuencia de **eventos entregados**, intervalos p50/p95/máximo, muestras por frame, duplicados/desorden y antigüedad al procesar. No se infiere 120 Hz ni frecuencia del sensor.
2. `Regla 50 mm`: el siguiente gesto recorre una línea recta de 50 mm físicos medidos por el PO. Se guardan recorrido máximo calculado, DPI reportado/fallback, factor mm/px y resolución. `PhysicallyVerified` no se infiere de Screen.dpi: requiere confirmación externa del recorrido y queda consignado en evidencia.
3. `10 suaves`, después `10 fuertes`: los siguientes diez gestos de cada serie se registran con intención declarada y ordinal, incluidos Tap/Cancel/SystemCanceled, sin seleccionar solo los swings que salieron bien. Cada uno conserva PowerResult, salto al fijar la mira y muestras originales con tiempos de evento/recepción. Si se interrumpe una serie se declara; no se sustituye por datos simulados.
4. Preguntas del PO de propuesta §5: ¿apunto donde quiero?, ¿entiendo siempre dónde apunto?, ¿el número responde a suave/fuerte? El juicio y los datos medidos se incorporarán sin inventar respuestas ni ajustar defaults silenciosamente.

Los registros `s1-sampling`/`s1-gesture` llevan metadata habitual de build/preset/hash/semilla, con `metricsValid=false`: son evidencia de input/previsualización, no una campaña de aceptación. Los desconocidos/percentiles desbordados son JSON null, con contadores explícitos. El resumen de 120 s mantiene sus cifras finales aunque la app siga recibiendo eventos; el overlay muestra las cifras vivas.

## Resultado nativo y devolución del PO

[Evidencia completa de la captura y resumen por ventana/serie](../validation/evidence/2026-10-25-s1-touch-sampling.json): **105 registros S1**, 102 gestos, 1475 muestras originales de esos gestos y tres resúmenes de adquisición. Se conserva también la tanda parcial anterior; nada se reemplaza por datos sintetizados. Las cifras siguientes corresponden a la **build instalada 311cb11, anterior a la corrección del borde de potencia** descrita más abajo.

El PO declara que hizo todos los bloques varias veces, con **mano izquierda dominante** y **regla física**. No se realizó una prueba diestra declarada. Sobre la tarea: «Hice todos los bloques varias veces, pero como la pelota no sale, solo veo datos. Entonces la verdad no entendí bien la prueba, solo hice lo que veía en la pantalla». Es evidencia de una **dificultad de comprensión del protocolo**. No se convierte en aceptación de puntería o potencia ni en una valoración negativa de la precisión de la mira: esas tres preguntas no quedaron establecidas por separado. El alcance S1 debía explicarse antes de pedir las pruebas: desplazar despacio el dedo para ver la mira, acelerar para fijarla y comparar el número de potencia, sin esperar que la pelota reciba un golpe.

### Adquisición táctil

| Adquisición | Duración real | Estado registrado | Eventos | Razón de intervalos positivos* | Intervalos p50 / p95 | Edad al procesar p95 / máximo |
|---|---|---|---|---|---|---|
| 1 | 120,0004 s | complete=true | 877 | 36,33 Hz | 16,7 / 75,1 ms | 24,5 / 25,27 ms |
| 2 | 56,6578 s | system-cancel, incompleta | 381 | 34,48 Hz | 16,7 / 50,1 ms | 21,7 / 26,19 ms |
| 3 | 44,4756 s | system-cancel, incompleta | 337 | 45,25 Hz | 16,7 / 29,2 ms | 18,7 / 27,12 ms |

\* `Stats.DeliveredHz` = número de intervalos positivos del mismo touch / suma de sus duraciones; excluye el tiempo entre dedos/gestos, pero incluye huecos dentro de cada toque. **No es frecuencia del sensor.** En la primera ventana hubo 7,31 eventos por segundo de tiempo total, máximo dos por frame, máximo hueco de 1908,66 ms, cero duplicados/desorden/edades negativas y un intervalo en el bucket ≥1000 ms. La resolución de histogramas es 0,1 ms por borde superior; máximo y razón de intervalos no se redondean al bucket en el JSON.

La ventana de 120 s coincide con otras series y pulsaciones de los controles. `complete=true` confirma el tiempo de adquisición, **no** una prueba aislada de apuntado continuo ni cumplimiento del protocolo diestro del plan. La mediana de intervalos de 16,7 ms describe esta captura; no demuestra una garantía de 60 Hz o ProMotion/120 Hz. La edad observada al procesar tampoco equivale al error total del timestamp, a latencia perceptiva ni a una garantía de ±40 ms.

### Regla y cancelaciones

Se recuperan **19 gestos etiquetados ruler-50mm**, seis Swing y trece SystemCanceled. El PO aclara que a veces el dedo fallaba, marcaba menos o paraba antes; considera bien la lectura aproximada de 48–55 mm. Los registros contienen **48,599 mm** (SystemCanceled), **48,862 mm** (SystemCanceled) y **55,180 mm** (Swing); también lecturas menores y ceros, conservados en la evidencia. No se escoge una sola lectura favorable ni se calcula un error porcentual contra una referencia exacta no confirmada para cada intento.

Datos de pantalla: **1206×2622 px, Screen.dpi=460, DpiSource=Reported, 0,055217389 mm/px**; sin fallback en estos gestos. Se hizo una comprobación física aproximada, pero no se certifica precisión de 50 mm ni se promueve a PhysicallyVerified. La opinión «lo veo bien» del PO queda registrada sobre esa lectura aproximada, sin extrapolarla a aceptación global de S1.

La repetición **permaneció en 3 s**, hash efectivo 9977d9ffd7e5 y cero pendientes. La preparación de sesión que debía desactivarla temporalmente no quedó aplicada. Los **21 SystemCanceled** de toda la captura aparecen entre 1,1 y 5,2 ms después de un registro de lanzamiento; `GesturePreview.OnLaunched` cancela el gesto activo. Esto respalda la explicación de los cortes por relanzamiento. La bitácora actual no incluye un motivo independiente dentro de SystemCanceled; se registra la correlación, no una clasificación inventada por el analizador. También hay dos adquisiciones interrumpidas, que no se cuentan como completas.

### Series de potencia, observación original

| Serie etiquetada | Registros / resultados | Potencia mínima / mediana / máxima | Conservadores | Salto al fijar mira: mediana / máximo |
|---|---|---|---|---|
| Suaves | 10; 8 Swing, 2 SystemCanceled | 0 / 0 / 8,27 % | 5: cuatro short-coverage, uno few-samples | 0 / 1,213 m |
| Fuertes A | 10 Swing | 0 / 27,52 / 49,81 % | 3 sample-gap | 1,928 / 2,910 m |
| Fuertes B | 10 Swing | 26,37 / 39,53 / 45,64 % | 0 | 2,439 / 2,860 m |

Las estadísticas incluyen todos los ordinales; no se ocultan cancelaciones o ceros. Se conservan además los seis fuertes iniciales (cinco Swing, un SystemCanceled) y otro fuerte posterior de una serie incompleta. **PowerResult, motivo conservador, cobertura, número de muestras, hueco y salto de cada registro están en el JSON**, junto con las muestras de entrada. Hay separación numérica entre series etiquetadas, pero no prueba todavía de que el PO controle la potencia a voluntad: la tarea no se entendió y parte de los ceros es salida conservadora.

### Corrección del borde de ventana dentro de S1

La inspección de las tres trazas sample-gap de Fuertes A mostró que los huecos de **41,676 / 100,017 / 66,678 ms terminaban exactamente en la primera muestra del arrastre reconocido**, fuera del interior de W'. El inicio de W' ya tenía una muestra real: no necesitaba interpolarse a través de esa pausa previa. El estimador original la contabilizaba por usar comparación inclusiva en el borde y devolvía cero. Es un error de inclusión temporal, **no falta de frecuencia táctil dentro de esas tres ventanas**.

Corrección: considerar solo intervalos que intersectan el interior de W', con la misma tolerancia numérica ya usada para interpolación (0,1 ns). Un hueco que cruza el inicio de ventana y sí se necesita para interpolar sigue invalidando; un hueco anterior que termina en la muestra exacta inicial, o posterior que empieza en la liberación exacta, no la contamina. No se cambia W=80 ms, cobertura=30 ms, mínimo=4, hueco máximo=34 ms, curva ni ningún default/criterio.

TDD: **seis pruebas compilables, RED válido 1/6 Pass y 5 fallos por aserción** (dos bordes sintéticos y las tres trazas nativas); la prueba que conserva un hueco real que cruza el borde ya pasaba. GREEN: **6/6**, dentro de la nueva regresión completa **336/336 EditMode**. Los tres replays dan respectivamente **65,193 / 58,865 / 40,255 %**, huecos internos máximos **16,727 / 16,670 / 16,670 ms** y 7/5/6 muestras distintas. La evidencia guarda estos resultados en `offlineReplayAfterBoundaryFix`, separada de las observaciones nativas. La reproducción usa las muestras originales, DPI y parámetros de S1. **Los resultados recalculados son replay offline tras corregir el código; los ceros observados en 311cb11 siguen intactos en la evidencia.**

### Propuestas concretas para revisión de Claude

- Explicar y demostrar el propósito de S1 antes de otro bloque: la mira se mueve con arrastre lento, se fija al acelerar y la potencia se observa como número; el golpe jugable corresponde a S2 y sigue no autorizado. Pedir primero al PO que identifique la mira y su cambio de estado. No se solicita repetir ahora la sesión.
- Preparar y comprobar explícitamente repetición temporal=0 antes de adquirir datos; después restaurar P0-calib-1, sin guardar el ajuste como default o preset. La repetición de 3 s sigue siendo el preset aceptado, no un error de defaults.
- Conservar las salvaguardas de potencia. La primera corrección es el borde de ventana probado con trazas, no subir maxGapMs. Para los gestos cortos con cuatro muestras pero cobertura de 29,17 ms, proponer a Claude/PO **comparar 20 frente a 30 ms de minCoverage**, conservando mínimo de cuatro muestras y máximo hueco 34 ms. Es una propuesta de experimento; no se cambia el valor. No rescatar pocas muestras o tiempos inválidos mediante potencia fuerte ficticia.
- Examinar con el PO los saltos de mira de hasta 2,91 m antes de ajustar gain/lookback/umbral de flick. Son saltos de **mira**, no el salto visual retroactivo de pelota que corresponde a S2; no se suavizan ni se declaran aceptables por el PO sin su juicio.

### Estado de entrega

Tareas 1.0–1.3 implementadas y verificadas; Tarea 1.4 tiene evidencia nativa y devolución del PO, **con calibración física exacta y juicio de control no establecidos**. La duración completa de una adquisición y las series registradas no se presentan como aceptación. Se entrega a Claude para revisar código, corrección y límites del protocolo; S1 no se fusiona ni se acepta por inferencia, y S2–S4 siguen no autorizados. Al finalizar, P0-calib-1 conserva hash efectivo 9977d9ffd7e5/cero pendientes en los últimos lanzamientos recuperados; P0-calib-1, Po-1 y last-active siguen byte a byte iguales al respaldo anterior a instalar. Los valores originales de esta sesión no requieren reescribir ningún preset. La compilación final corregida y la entrega se registran a continuación.

## Fuentes oficiales

- [Unity Input System 1.20 — eventos](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/input-events.html): timestamps y flujo de eventos.
- [InputEvent.time](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.LowLevel.InputEvent.html#UnityEngine_InputSystem_LowLevel_InputEvent_time) y [StateEvent.GetState](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.LowLevel.StateEvent.html): datos del evento frente al estado de los controles.
- [InputSettings.disableRedundantEventsMerging](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputSettings.html#UnityEngine_InputSystem_InputSettings_disableRedundantEventsMerging); contraste con el código local de 1.20.1 citado arriba.
- [Unity 6000.3 — Screen.dpi](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Screen-dpi.html): puede ser desconocido; no sustituye la comprobación física.
- [Device.deferSystemGesturesMode](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/iOS.Device-deferSystemGesturesMode.html): cambio runtime, sin editar PlayerSettings.

## Build final y preparación de revisión

Tras la corrección 340ca2c: **336/336 EditMode y 161/161 PlayMode**, check-release **dev/prod OK** repetido con exports frescos. Módulos y exclusión del framework de pruebas conservados. Build dev **c6c68c1** limpia, Release ARM64 no Development, metadata y DefaultPreset embebidos cotejados, firma estricta verificada. **200 archivos** coinciden con las fuentes probadas. Perfil compatible y vigente hasta **2026-10-28T14:59:31Z**, comprobado de nuevo el **2026-10-25T14:30:45Z** antes de instalar; sin renovación requerida hoy.

La actualización **se instaló correctamente**, pero el lanzamiento fue rechazado por iPhone bloqueado (CoreDeviceError 10002 / FBSOpenApplicationErrorDomain 7). No se afirma arranque observado ni nueva campaña física en esta build. Los archivos P0-calib-1, Po-1 y last-active recuperados después de actualizar conservan los bytes del respaldo previo. La comprobación de P0-calib-1 al comienzo de S1 sí está satisfecha en 311cb11; distinguirla del arranque todavía no observado de la build corregida. No se requiere repetir la sesión física para conservar sus datos ni para la reproducción automatizada de las tres trazas.

[PR #17](https://github.com/rafaallendes-boop/squash-clash/pull/17) se entrega para revisión de Claude, **sin fusionar y sin declarar S1 aceptado por el PO**. Spec v0.3, propuestas, planes y ProjectSettings están byte a byte iguales a main; los 41 valores heredados y SimulationKey física siguen intactos. S2–S4/T8 no autorizados. Renovar el perfil antes de cualquier instalación desde el 27 y comprobar vigencia efectiva siempre.
