# Calibración experimental del gesto S1

**Histórico del 25, sustituido por la resolución autorizada del 26 al final — DETENIDO — GC-S1-01: el B elegido produce potencia fuerte al apuntar rápido y soltar sin movimiento. No apto para instalar ni para aceptar S1.** La elección provisional de fase A queda bloqueada por la reproducción posterior; ver el cierre y la evidencia sintética.

Autorización PO del 2026-10-25. Rama `firstshot/s1-gesture-calibration`, base main e11cc3aaa9dad7112d886e43d990732e0e198b43. S1 no aceptado; S2–S4/T8 no autorizados. Este registro se escribe y se commitea **antes de calcular B**. Las bandas siguientes son instrumentos experimentales de esta comparación; no modifican criterios de la spec ni del plan.

## Criterio previo a los resultados de B

Se comparan los mismos 40 intentos, incluidos ceros y conservadores. A usa **sin recalcular** la tabla aprobada de [PR #21](2026-10-25-first-shot-s1-power-calibration-proposal.md): 500/0/3/51, flick 250. No hay gestos medios registrados. Se declaran manos mezcladas, orden 30→20 y DPI sin regla como límites.

Bandas experimentales sobre P normalizada: **suave [0, 0,25), media [0,25, 0,60), fuerte [0,60, 1]**. Un cuarto de escala deja margen a variación fina; la banda media ocupa el centro y fuerte comienza claramente por encima de media escala, sin exigir 100 %. Cero se conserva en la banda suave para clasificar todos los intentos, pero una mediana suave cero no acredita control de intensidad. Estas bandas no son zonas de H2 ni criterios de aceptación de S1.

Requisitos por orden, fijados antes de B:

1. Orden: mediana suave < mediana fuerte.
2. Separación: distancia entre medianas ≥ 0,35 y cuartil 25 fuerte > cuartil 75 suave, incluyendo ceros; percentiles por interpolación lineal del índice (n−1)p.
3. Rango útil: mediana suave **> 0 y < 0,25**, mediana fuerte **≥ 0,60**. Se informará también saturación; más de 20 % de fuertes en 100 % indica escala demasiado corta. No hay evidencia para comprobar la banda media antes de una sesión futura.
4. Salidas conservadoras: como máximo 20 % por serie agregada suave/fuerte. Se exige apoyo temporal y no se reclasifica un rechazo como potencia medida para pasar el requisito.
5. Ningún fuerte accidental: buscar en los 138 registros un arrastre rápido seguido de liberación suave. Las trazas no etiquetadas solo permiten detectar **compatibilidad cinemática**, no intención; ausencia de casos representativos deja este requisito **NotEvaluated**, nunca Pass. Una contraprueba cinemática con P fuerte es un riesgo que impide dar por segura la elección. No se inventarán intenciones ni nuevos datos.
6. Simplicidad: si ambos cumplen y la diferencia práctica es ≤ 0,05 en medianas, ≤ 10 puntos porcentuales en rechazo y ninguna ventaja demostrada en seguridad, gana A.

Solo se implementará un método que resulte defendible con estos requisitos; si ambos se descartan o la seguridad no permite elegir, se documenta y se detiene antes de código/build/sesión, para decisión del PO. No se reajustan bandas para forzar un resultado favorable.

## B propuesto antes de calcular

Pico suavizado causal con **tau de suavizado 20 ms**, **decaimiento exponencial 120 ms**, escala **deadZone=10 / full=400 mm/s**, exponente 1. Cada intervalo positivo de duración ≤ 51 ms aporta velocidad escalar distancia/tiempo. Dentro de cada tramo de muestras consecutivas fiables, suavizado `s += (1−exp(−dt/20ms))*(v−s)`. Solo tramos con ≥ 3 muestras y ≥ 30 ms pueden aportar un pico. `q = max(q*exp(−dt/120ms), s)` mientras hay apoyo; fuera de un tramo fiable q solo decae con tiempo real y el suavizado se reinicia. En liberación se aplica el decaimiento hasta su timestamp y `P=clamp((q−10)/390,0,1)`. Sin tramo con apoyo, salida conservadora P=0. El decaimiento incluye las pausas aunque no lleguen muestras. No se exige que el soporte termine justo en la ventana final: esa es la diferencia experimental con A.

Se calculará el candidato fijo y una sensibilidad **predeclarada**, sin seleccionar nuevas constantes después: full ∈ {300, 400, 500}, deadZone ∈ {0, 10, 20}, decaimiento ∈ {60, 120, 180} ms, tau=20 ms; soporte 3/30/51 sin cambios. El objetivo es comprobar si alguna escala razonable de este B alcanza rango útil sin exponer fuertes al apuntar rápido y soltar suave. Se publican todas las combinaciones, no solo la favorable. No se amplía la búsqueda para hacer pasar el criterio.

## Estado

Criterio fijado en commit **23f63e7**, antes de calcular B. [Evidencia del reprocesamiento](../validation/evidence/2026-10-25-s1-gesture-calibration-offline.json), fuente original SHA-256 ab43e560f23fa9522fa4d00c0fa4ed842f26922b180d8f7a21f8aaac2aaf4b69. A se importa de las 40 filas de PR #21, con su precisión publicada de tres decimales porcentuales; no se vuelve a estimar. B se reprocesa en tiempo real de timestamps con aritmética double offline; no es medición nueva ni afirmación de identidad bit a bit con Unity.

| Método | Suaves P mín/med/máx (%) | Fuertes P mín/med/máx (%) | Conservadores suave/fuerte | Fuertes saturados |
|---|---|---|---|---|
| A 500/0/3/51 | 0 / 0 / 5,082 | 0 / 35,111 / 91,363 | 14/20 y 3/20 | 0/20 |
| B primario 400/10/120 ms | 0 / 0 / 8,002 | 0,662 / 63,037 / 100 | 0/20 y 0/20 | 2/20 |
| **B elegido provisional 400/0/120 ms** | **0,581 / 1,675 / 10,302** | **3,145 / 63,961 / 100** | **0/20 y 0/20** | **2/20** |

A supera orden/separación y falla rango/rechazos. La escala no recupera las 14 salidas conservadoras suaves: con los mismos filtros seguirían siendo cero. Incluso llevar full de 500 al mínimo vigente 300, con deadZone ya 0 y exponente 1, dejaría la mediana fuerte en aproximadamente 58,5 %, bajo la banda alta; no se propone ampliar rangos ni manipular criterios. Esto descarta **el candidato A evaluado y su recalibración de escala dentro del rango vigente**, no afirma que sea imposible cualquier otro filtro temporal aún no estudiado.

El B primario falla mediana suave positiva. De las **27 combinaciones predeclaradas**, cuatro cumplen los requisitos observables: full/dead/decay **300/0/60, 400/0/120, 400/0/180 y 400/10/180**. Se publican las 27, incluidos fallos. Se elige **400/0/120**, con menos saturación que 300/0/60 (2 frente a 4 fuertes) y memoria más corta que 180 ms. Mantiene tau 20, soporte 3 muestras/30 ms, hueco máximo 51. Cuartil 25 fuerte 51,199 % > cuartil 75 suave 5,812 %; separación de medianas 62,286 puntos. **No se declaran aprobados control medio ni ausencia de fuertes accidentales**: falta evidencia con intención conocida y fase física.

B reduce rechazos porque puede usar un tramo anterior fiable y decaer hasta la liberación, en vez de exigir soporte dentro de los últimos 80 ms; no mejora mágicamente el muestreo ni convierte un intervalo largo en información. Zona muerta 0 expone deriva/temblor; dos fuertes saturados y un fuerte de 3,145 % revelan variación amplia. Una mediana suave de 1,675 % apenas usa el extremo inferior: falta comprobar si el PO consigue tres intensidades naturales. Escala fija, sin normalización por jugador.

Búsqueda de riesgo en **los 138 gestos**: dos intervalos consecutivos >250 mm/s y velocidad neta final de 80 ms <100 identifican cuatro sustitutos cinemáticos, todos preview; no son casos con intención etiquetada. Timestamps y valores en JSON: con B elegido dan P **3,352 / 5,074 / 5,938 / 4,688 %**, ninguno fuerte. **No hay casos representativos con intención confirmada** de apuntar rápido y soltar suave; requisito de seguridad NotEvaluated. El mismo perfil de movimiento podría expresar un flick detenido o apuntado: el estimador no conoce esa intención. Las pruebas sintéticas deben declarar velocidades y pausas, sin venderlas como garantía para todas las intenciones. La cuarta serie física decidirá si esta memoria breve produce fuertes no deseados. No se cambia la escala tras observarla; si falla se informa y detiene.

**Decisión previa a implementación:** B provisional 400/0, tau 20 ms/decaimiento 120 ms, soporte 3/30/51; flick 250 sin cambio. Implementar solo este B nuevo; A existente se conserva para compatibilidad de defaults. Selector solo en panel dev; sin modificar interfaz del jugador. Claves nuevas compatibles y fijación en liberación opt-in, pendiente de TDD. Ningún contacto con el iPhone ni dato nuevo hasta aquí.


## Implementación y regresión antes de la build

Fuentes de producto **20271ff**. B nuevo es incremental: una actualización por muestra, coste y memoria constantes, sin recorrido del historial en cada frame. A existente conserva su fórmula; el selector vive en parámetros del panel dev, sin controles nuevos en la interfaz del jugador. No se mide H17 ni se afirma ausencia de coste por las exponenciales añadidas. La autoridad de pelota, reloj y predicción no se modifican.

TDD observado, con XML/log frescos y sin errores CS:

| Cambio | RED compilable por aserción | GREEN focal |
|---|---|---|
| Pico causal | 0/6, seis aserciones fallidas con esqueleto | 6/6; después dos casos de reinicio/entrada inválida |
| Fijación en liberación | 2/5, tres aserciones fallidas | 5/5 |
| Claves compatibles | 0/2, dos aserciones de catálogo ausente | 2/2 |
| Runtime, etiquetas y controles | 11/14, tres aserciones fallidas | 14/14 |

Regresión completa sobre producto 20271ff: **359/359 EditMode y 172/172 PlayMode**, XML posteriores al inicio, cero tests omitidos y sin errores CS. Incluye replay de los 40 registros originales contra B offline, tolerancia absoluta 1e-5 en P; no es medición física ni promesa bit a bit entre plataformas.

Claves agregadas a defaults: **power.method=0, power.peakSmoothingMs=20, power.peakDecayMs=120, aim.freezeOnRelease=0**. Ninguno de los 58 valores anteriores cambia (proyección hash 00a501ad7066 comprobada por prueba). Nuevo hash defaults **6095c0b38614**, 62 claves. P0-calib-1 extendido **fdd997f2b85a**, manteniendo su archivo de 41 claves intacto; proyección heredada 0d39e6016bd8 y SimulationKey 166a96e319c7 intactas. Tests comparan PowerEstimator y AimModel con configuración anterior y defaults nuevos; la opción de liberación apagada conserva Aiming sin flick.

[Preset experimental S1-calib-gesture](../validation/presets/S1-calib-gesture.json): solo nueve sobrescrituras, **hash efectivo calculado 6071903f8128**, B 400/0, tau 20/decaimiento 120, soporte 3/30/51, fijación al soltar activa y repetición 0. No sobrescribe P0/Po-1 ni promueve valores a defaults. Se cargará por importación temporal, sin cambiar last-active; no guardar ni seleccionar persistentemente.

Registros s1-gesture pasan a **schema 2**: freezeCause (None/Flick/Release), aimInvalid, powerMethod, powerSpeedMmPerS/powerSpeedBasis y edad del soporte. En B, netSpeedMmPerS se emite null porque el pico decaído no es velocidad neta. CoveredMs/n/gap describen el tramo que sostiene ese pico, que puede preceder los últimos 80 ms; no se reinterpretan como una ventana A válida. Posición, salto y timestamps brutos conservados. Protocolos medium y aim-soft añadidos solo dentro de DEV, con cierre automático; Tap/Canceled/SystemCanceled se conservan como intentos.

Respaldo previo a instalación recuperado de la app 5e218ea: P0-calib-1/Po-1/last-active, P0 seleccionado, archivo P0 SHA-256 2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd. Estado histórico antes del caso de riesgo ampliado: pendientes check-release dev/prod, build, vigencia del perfil, instalación, arranque con hash extendido y cuatro series. Sustituido por la detención GC-S1-01 al final. Sin cambios en ProjectSettings, Packages, scripts, spec ni planes.


## GC-S1-01: bloqueo encontrado antes de instalar

El caso inicial solo probaba arrastre a 400 mm/s durante 200 ms y pausa de 150 ms, que B resolvía sin fuerte. Al ampliar **el mismo requisito obligatorio**, manteniendo la banda fuerte predeclarada en 60 %, aparecen dos fallos. [Evidencia nueva](../validation/evidence/2026-10-25-s1-gesture-calibration-risk.json); perfiles **sintéticos**, no gestos nuevos del PO ni medición del iPhone:

| Arrastre vertical | Recorrido | Suelta tras pausa sin movimiento | P B | Resultado exigido |
|---|---:|---:|---:|---|
| 400 mm/s durante 200 ms | 80 mm | 150 ms | 28,649 % | No fuerte: Pass |
| 850 mm/s durante 100 ms | 85 mm | 150 ms | **60,472 %** | **No fuerte: Fail** |
| 1000 mm/s durante 60 ms | 60 mm | 150 ms | **68,060 %** | **No fuerte: Fail** |

Los tres recorridos caben en la altura reportada de pantalla (2622 px × 0,055217 mm/px ≈ 144,78 mm; no regla física). Los picos antiguos alcanzaban 851,869 mm/s en fuertes y 2109,687 en preview: no se usó una velocidad fuera de todo el rango registrado. Esto no demuestra que el PO hiciera estos perfiles ni que fueran accidentales en la sesión antigua. **Son contrapruebas del comportamiento requerido**, con velocidad neta cero durante los últimos 80 ms y soporte real sintético anterior. No se asume una tasa real del sensor por usar pasos sintéticos de 20 ms.

Con este B, la cola exponencial puede conservar suficiente pico para superar 240 mm/s (60 % de full 400) aun después de 150 ms sin movimiento. El algoritmo no recibe la intención de apuntar frente a golpear. La prueba de flick detenido y la prueba de apuntado rápido no se pueden declarar satisfechas en general solo eligiendo una pausa conveniente. La cobertura original de pruebas fue insuficiente: el caso de 400 pasaba, los dos ampliados fallan.

Reproducción final fresca: **10 casos focales, 8 Pass / 2 Fail**, por aserción de P<0,60, sin errores CS y XML de 2026-10-26T00:29:30Z (25 local). Se conservan los fallos, sin tocar producto, bandas ni constantes después. **La rama final no está verde**. Las suites completas previas 359/359 y 172/172 anteceden a esta ampliación; no se presentan como regresión completa verde del head final.

Por la instrucción del PO **«ante un defecto, detenerse»**, se detienen implementación adicional, build nativa, instalación y fase C. No se prueba otro decaimiento ni otra escala para conseguir un resultado favorable. **La elección B queda bloqueada, sin promover A ni aceptar el preset**. Propuesta para revisión de Claude/PO: decidir si la intención debe distinguirse con una regla cinemática adicional explícita y validable, o si se revisa el candidato temporal A en una autorización aparte; preservar los tres controles y el caso de riesgo. Este informe no autoriza ese rediseño, un selector del jugador, un nuevo criterio ni S2.

## Exportación y preservación al detenerse

check-release dev/prod terminó sus comprobaciones de módulos sin CS: dev incluye Core/Runtime/DevPanel, prod Core/Runtime y sin DevPanel. **Primera exportación no elegible como build limpia:** PlayMode dejó cambios transitorios de color temperature/antialiasing y archivos de Performance Test; metadata gitDirty=true. Se archivaron diferencias/evidencia privada y se restauraron GraphicsSettings/QualitySettings byte a byte a HEAD; los archivos temporales de Performance Test ya habían sido limpiados por el proceso. No se repitió la exportación después del defecto. No se ejecutó xcodebuild ni se generó/instaló una app nativa nueva. No se presenta este check como validación de una build lista para instalar.

La captura posterior al paro coteja directamente los bytes y los hashes de **P0-calib-1, Po-1 y last-active**, idénticos al respaldo. P0 sigue seleccionado; último lanzamiento recuperado **2026-10-26T00:26:52.7946760Z / 5e218ea dev / 9977d9ffd7e5 / cero pendientes**. No se cargó S1-calib-gesture, no se cambió repetición ni se requirió restaurar configuración. El nuevo hash P0 fdd997f2b85a es calculado, **no observado en el dispositivo**, porque no se instaló la app nueva. Defaults nuevos solo en esta rama, no promovidos a main.

El PO declaró para la prueba prevista **«Izquierda, mi dominante»** y teléfono conectado/desbloqueado. No se pidieron ni ejecutaron series. Las cinco preguntas permanecen **sin respuesta, no formuladas**: «¿Consigues las tres intensidades de forma voluntaria?»; «¿Apareció algún golpe fuerte que no buscabas?»; «¿Apareció algún golpe fuerte al apuntar rápido y soltar suave?»; «¿La mira queda donde quieres apuntar?»; «¿Se siente consistente entre intentos?». No inferir esas respuestas de validaciones anteriores. Las cuatro series, estadísticas físicas, intensidad media y aceptación son **NotEvaluated**.

También falló el acceso de la herramienta a Duplicación del iPhone (native pipe), sin manipulación de UI; no es el bloqueo que detiene la entrega ni motivo para cambiar código. No se llegó a verificar un perfil nuevo para instalar. Para cualquier instalación futura comprobar vigencia y renovar primero desde el 27; fecha de vencimiento previamente registrada 2026-10-28T14:59:31Z, no revalidada aquí.

Entrega en [PR #22 para revisar el bloqueo](https://github.com/rafaallendes-boop/squash-clash/pull/22), abierta/no borrador y sin fusionar, fuente de cierre 1eee39ef7d0de63c2745fe2cc06dd185bc02cfe1. Los dos RED son explícitos; no está lista para aprobar ni instalar. **S1 sigue sin aceptar; S2–S4/T8 no autorizados. Detenerse.**


## Resolución autorizada el 2026-10-26 (prevalece sobre el bloqueo anterior)

El PO comunica revisión independiente de Claude sobre head 41e9abf y autoriza hacer del riesgo una condición **eliminatoria**, sin alterar sus tres perfiles, la pausa inmóvil de 150 ms ni P<0,60. El historial de rechazo de B 400/0/120 permanece arriba. Se ensayan en C# solo los cuatro supervivientes del grid anterior; [evidencia nueva](../validation/evidence/2026-10-26-s1-gc-resolution.json).

| full / dead / decaimiento | Riesgo 400 × 200 ms (%) | Riesgo 850 × 100 ms (%) | Riesgo 1000 × 60 ms (%) | Supera los tres |
|---|---:|---:|---:|---|
| **300 / 0 / 60 ms** | **10,944** | **23,101** | **25,999** | **Sí, elegido** |
| 400 / 0 / 120 ms | 28,649 | 60,472 | 68,060 | No |
| 400 / 0 / 180 ms | 43,458 | 91,730 | 100,000 | No |
| 400 / 10 / 180 ms | 42,008 | 91,518 | 100,000 | No |

Por quedar un único superviviente, menor saturación y menor memoria no requieren desempate; no se escoge un candidato eliminado para saturar menos. No se prueban valores fuera de estos cuatro. **GC-S1-01 se resuelve para el candidato 300/0/60 según esta decisión**, no se afirma inmunidad universal a gestos accidentales. Algoritmo B, tau 20, soporte 3 muestras/30 ms/51 ms, bandas, casos y pausa intactos. [Preset](../validation/presets/S1-calib-gesture.json) hash **08b6fe90889b**; defaults6095c0b38614/P0fdd997f2b85a sin cambios desde el head revisado.

El costo aceptado por el PO se reproduce con flick sintético de 500 mm/s durante 100 ms y pausa posterior de 100 ms: P **53,959 %** con 400/0/120 frente a **31,267 %** con 300/0/60. No es medición nueva. Claude comunica 18/20 fuertes soltando a ≤54 ms del pico y gestos preview de apuntado esperando ≥333 ms; se registra esa declaración y la aceptación del PO, sin convertirla en frecuencia táctil ni una medición nueva. Se volverá a observar el compromiso con contacto real en S2 cuando se autorice; S2 no está autorizado aquí.

Nueva prueba del flick de 500 mm/s detenido 25 y 50 ms: **100 % y 71,945 %**, ambas en banda fuerte, GREEN directo con el candidato elegido. No se declara RED inexistente. Los diez casos focales originales mantienen inputs, pausa y aserciones; solo su factory usa la configuración del candidato elegido. Nueva prueba Ended al mismo timestamp produjo **RED 11/12**, fallo por Aiming frente a Frozen/Release; se corrigió el caso terminal sin inventar velocidad de dt cero ni una segunda fijación tras flick. Focal conjunto **23/23 GREEN**, sin CS; regresión y exportación limpia pendientes a continuación.

La fase C mantiene la mano literal **«Izquierda, mi dominante»** y cuatro series de diez. Antes de la cuarta se mostrará literalmente: «Si apuntas muy rápido, la mira se fijará en ese momento (rombo amarillo); es el comportamiento actual. Lo que evaluamos es la potencia al soltar.» No se cambian escala ni umbrales durante la sesión. Ante otro defecto se detiene. Cinco preguntas por separado; restauración P0 con hash fdd997f2b85a/cero pendientes y tres archivos idénticos. S1 sigue sin aceptar, S2–S4/T8 no autorizados.


Verificación posterior a la resolución, producto **8c0a682**: **371/371 EditMode y 172/172 PlayMode** completos, XML frescos/sin CS. Incluye replay de los mismos 40 con el nuevo candidato, comprobación de preset/hash y los diez focales sin cambio de entradas/aserciones (verificación textual frente a 41e9abf, salvo factory 300/0/60). Defaults siguen 6095c0b38614, P0 fdd997f2b85a. Antes de exportar se archivaron los cambios transitorios que PlayMode dejó en GraphicsSettings/QualitySettings y se restauraron byte a byte a HEAD. Sigue exportación limpia dev/prod y build nativa; la recuperación inicial del contenedor falló por conexión CoreDevice invalidada, por lo que no se instalará sin conseguir antes el respaldo nuevo.


Build e instalación de resolución: **8476dd8 dev**, exports dev/prod con módulos correctos y **gitDirty=false**, build nativa **Release ARM64/no Development** firmada. Verificación efectiva previa a instalar **2026-10-26T23:24:57Z / fecha local 26**, perfil válido hasta **2026-10-28T14:59:31Z**, equipo/dispositivo incluidos y codesign correcto; no se necesitó renovación el 26. App instalada y tres archivos cotejados directamente byte a byte iguales al respaldo nuevo. Copiado S1-calib-gesture como archivo separado, sin cambiar selección P0. **No se verificó todavía el nuevo arranque ni se cargó el preset:** iOS rechazó abrir por teléfono bloqueado (CoreDevice10002); Duplicación pide autenticación local del Mac. Se pidió al PO completarla sin compartir contraseña. Estado de preparación, sin series/juicios nuevos aún; no equivale a defecto del producto ni a aceptación de S1.


Preparación completada tras autenticación local del PO: arranque de8476dd8 con P0 **fdd997f2b85a/cero pendientes**, observado en registros y etiqueta UI. S1-calib-gesture cargado desde su archivo separado mediante DEV; se restauró **last-active** desde el respaldo antes de aplicar/relanzar, dejando la selección persistente P0 y la configuración S1 solo en memoria. Lanzamiento de sesión **2026-10-27T00:50:35.4162550Z (26 local) /08b6fe90889b/cero pendientes**; archivo experimental igual al versionado y los tres archivos originales cotejados directamente byte a byte idénticos. No se guardó ni sobrescribió P0/Po-1. DEV cerrado, filas de protocolo dejadas accesibles. Cero gestos registrados antes de empezar. Se mostró el recordatorio literal autorizado y se pidió únicamente la serie de **10 suaves con izquierda**, sin sustituir intentos ni adelantar los otros bloques. Estado en espera de datos, no inferir resultados ni las cinco respuestas.
