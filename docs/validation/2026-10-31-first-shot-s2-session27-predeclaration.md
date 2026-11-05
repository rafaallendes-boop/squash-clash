# First Shot S2 — predeclaración de la sesión 2.7

Fecha: 2026-10-31, America/Santiago. **Documento para revisión de Claude en PR #30; sesión no autorizada ni ejecutada.** El PO comunica aprobación de fase 1 (2.0–2.6) en `c2e8ee7e38d611670136fde62c32be20297c5137`, producto `9131b1c`. `1bbaa39` y `c2e8ee7` solo modifican documentación. La PR permanece sin fusionar hasta que el PO juzgue la sesión 2.7, incluido el salto.

**Revisión de 2026-10-31:** Claude aprobó `31d7433` con ajustes documentales A1–A4 y menores, autorizados por el PO; incorporados en esta revisión. Los 13 literales se publican para revisión de Claude antes del primer bloque; no se ejecutó Unity ni la app.

Fuentes: [plan r2, Tarea 2.7 y D4.7](../superpowers/plans/2026-10-25-squash-clash-first-shot.md), [propuesta r2/D4](../superpowers/specs/2026-10-24-first-shot-design.md), [aprobación y límites del PO](../reviews/2026-10-25-first-shot-po-approvals.md), [informe de fase 1](../reviews/2026-10-30-first-shot-s2-m24-phase1.md). Esta predeclaración concreta las tareas experimentales futuras; no modifica esos documentos de producto, SL, defaults, bandas, criterios ni umbrales.

## 1. Estado y observaciones de Claude

La revisión comunicada no tiene bloqueos de fase 1: S2-CLOCK-01 correcto, fixtures solo adaptados en preparación, cobertura D4.6/N01–N12 presente, archivos protegidos intactos desde `8777f9b` y check-release dev/prod limpio. Los resultados **522/522 EditMode y 297/297 PlayMode** y los exports de `1bbaa39` son evidencia anterior aprobada, no ejecuciones nuevas de esta entrega.

Se registran sin código las tres observaciones:

1. `ArmSlingBlock(block, expectedGestureHash)` solo se usa en pruebas. DEV llama sin hash; el campo `expectedGestureHash` de su registro contiene el hash capturado al armar, no una referencia predeclarada. El protocolo usa comprobación externa/manual (§3); no presenta ese campo como prueba de igualdad con un esperado independiente.
2. El mapa puede añadir segmentos durante el vuelo en curso; se reinicia al comenzar el lanzamiento siguiente. La corrección de pausa/vuelo detenido no demuestra tamaño máximo, bytes o coste nativos del vuelo activo. Se observa sin añadir límites ni poda.
3. El perfil conocido vence **2026-11-03T18:29:44Z**. Antes de cualquier instalación futura el PO debe renovarlo y debe comprobarse el perfil efectivamente incorporado, aunque el perfil anterior todavía estuviera dentro de fecha.

**Límite actual:** solo escritura documental. Ningún build de Xcode, instalación, acceso al iPhone, sesión 2.7, cambio de código/tests/defaults/SL/spec/criterios/bandas/umbrales, S3–S4 o T8. Nada de lo escrito en futuro es una orden de ejecución actual. Ante otro defecto, registrar y detenerse; no implementar ajustes.

## 2. Requisitos previos a instalar y medir, bajo autorización futura

1. Obtener autorización expresa de build/instalación/acceso y de la sesión/protocolo revisado. La aprobación de 2.0–2.6 y la de este documento no la sustituyen. Fijar fuente de producto aprobada, head documental y build/profile; cualquier delta de producto vuelve a revisión. Las regresiones/check-release frescos que exija esa autorización se registrarán entonces; no se ejecutan ahora ni se atribuyen las del día 30 a una build futura.
2. **Renovación del perfil a cargo del PO, seguida de comprobación efectiva antes de cada instalación dev o prod.** Registrar vencimiento UTC, fecha de comprobación e identidad de build firmada; cotejar el perfil incorporado al `.app` y su correspondencia con la app/dispositivo autorizados, sin publicar identificadores personales, credenciales ni claves. No basta la fecha de un perfil en caché, un export limpio o la renovación declarada. Si falta renovación/comprobación o vence antes de la instalación prevista, no instalar.
3. Antes de instalar, con acceso ya autorizado, recuperar/exportar logs anteriores: la retención vigente es de 30 archivos/50.000.000 bytes por carpeta. Preservar evidencia fuera de esas carpetas antes de nuevas aperturas; no cambiar la política ni borrar datos para preparar la campaña.
4. Respaldar **`P0-calib-1.json`, `Po-1.json` y `last-active.txt`**. Guardar copia binaria, existencia, bytes y SHA-256 en ruta local ignorada, sin versionar datos del iPhone. Registrar qué instalación/proceso cubre el respaldo. Si falta alguno o P0 ya no corresponde al punto de control aprobado, registrar y consultar al PO; no recrearlo desde defaults ni sobrescribirlo para que coincida.
5. Preparar futura build dev Release ARM64/IL2CPP, no Development, con SQUASH_DEV; prod Release ARM64/no Development/sin ese define. Mantener las herramientas autorizadas. Cotejar los tres archivos **después de instalar y antes de preparar bloques** con el respaldo: byte a byte y SHA-256. Si difieren, no iniciar campaña; preservar ambos estados y detenerse. Mismo requisito para la instalación prod posterior, con respaldo nuevo y comparación también contra la referencia inicial.
6. Arranque nuevo, P0 seleccionado, cero pendientes y hash extendido realmente observado en la build instalada. Referencias de fase 1: 106 claves, defaults `915ae1e479b9`, P0 extendido esperado **`dd122831f592`**, proyección histórica P0 `8de3f2ecb217`, 41 valores heredados de defaults `0d39e6016bd8`, SimulationKey `166a96e319c7`. Son referencias aprobadas/calculadas, **no observación de un iPhone S2**. Leer log de arranque y configuración completa para confirmarlas; un nombre de preset no basta. Un P0 de 41 claves hereda el control 2; Po-1 se preserva, no se migra.

No desinstalar para limpiar datos. No promover un preset. Preparación temporal en memoria, sin Guardar/Guardar como/Importar ni escritura de presets para alterar el punto de control; selección `last-active` P0 repuesta/cotejada **antes** de aplicar/relanzar, según D4.7. Las lecturas/comprobaciones de este apartado son futuras, no se han realizado en esta entrega.

## 3. Configuración predeclarada y verificación del hash por bloque

### 3.1 Manifiesto independiente de condiciones

Base **B**: P0 aprobado de 41 claves superpuesto a los 106 defaults de la fuente aprobada; única diferencia histórica respecto de defaults: `launcher.repeatSeconds=3`. Se conserva ese 3: `practice.enabled=1` suspende su temporizador sin persistir cambios. Condiciones comunes: control 2, P2-Analytic, `pace.tempo=0.65`, `debug.slowMotion=1` (**1 = sin cámara lenta**), `practice.repeatSeconds=1.5`, `practice.observeMaxSeconds=4`, `timing.assistMs=40`, `timing.validMs=100`, `timing.latencyCompMs=0`, `timing.earlyPolicy=0` (T3, nunca T3-suave), notas activas. Los demás valores son B, incluidos 2/35 mm, N2 y ayudas existentes. Sin halo/lupa/zoom ni filtros nuevos.

**V100** significa `timing.validLimit=1`; **T3 puro**, `timing.validLimit=0`, conservando `validMs=100` almacenado. Cámara fija = `camera.mode=0`; seguimiento = 1 solo en el bloque expresamente indicado. La mano activa será **pulgar izquierdo dominante del PO**, declarada antes de empezar y estable; no cambia la mano diestra del personaje ni AX-1. Otra mano o condición requeriría nueva predeclaración, no un reemplazo de datos.

Las recetas son exactamente las de `PracticeLaunchDefinitions` aprobadas y la tabla congelada en `0c638da`:

| Receta / selector | Pelota inicial (x,y,z) m | Objetivo frontis (x,y) m | Velocidad horizontal de entrada m/s | Cuerpo nominal (x,z) m |
|---|---|---|---|---|
| SL1 / 0 | (2,6;1,2;7) | (2,6;2,2) | 15 | (3,2;5,5) |
| SL2 / 1 | (3,8;1,2;7) | (3,8;2,2) | 15 | (3,2;5,5) |
| SL3 / 2 | (0,35;1,3;7) | (0,35;1,5) | 18 | (1,3;8,3) |
| SL4 / 3 | (6,05;1,3;7) | (6,05;1,5) | 18 | (5,1;8,3) |
| SL5 / 4 | (2,6;1;3) | (2,6;0,7) | 6 | (3,2;1,6) |
| SL6 / 5 | (3,8;1;3) | (3,8;0,7) | 6 | (3,2;1,6) |

La velocidad de entrada no es la velocidad total de salida del solver. No se retocan estos números ni el cuerpo para conseguir contactos. No escribir coordenadas nominales redondeadas para sustituir las representaciones que aplica la receta.

Se define **H(s,m,c)** como el `ParamSet.Hash` del manifiesto B preparado con receta SLs, modo m y cámara c, y con el cuerpo que aplica esa receta. Es una definición del esperado **independiente del registro de armado**. Su canonicalización es la existente: claves ordenadas ordinalmente, `key=value\n`, double `ToString("R", InvariantCulture)`, SHA-256/primeros seis bytes en 12 hex minúsculos. No usar solo el hash de defaults, el nombre P0 o SimulationKey para comparar configuraciones S2 distintas.

Para que la preparación no dependa de la receta anterior, cada bloque dev se prepara cargando B desde P0 sin guardar, seleccionando/aplicando SL1 como ancla de preparación y luego la SL solicitada, modo y cámara. Esos lanzamientos se etiquetan **preparación, no medidos**. No hay gestos del PO durante esta operación. Conservar todas sus trazas aparte. La selección de receta coloca el cuerpo; la representación efectiva puede contener doubles procedentes de floats. El manifiesto técnico debe conservar esos valores completos, no solo lo redondeado que muestra un slider.

**Los 13 literales ya están calculados por A3**, con todas las entradas, SHA-256, cuerpos float32→double, deltas y manifiesto B completo en el [anexo de hashes](2026-10-31-first-shot-s2-session27-hashes.md). **H(1,V,0)=dd122831f592**, comprobación obligatoria satisfecha fuera de Unity. No se difiere el cálculo a la sesión ni se toma el hash observado al armar como referencia. Claude debe revisar estos resultados **antes del primer bloque**. El cotejo con Unity corresponde a la preparación futura, solo cuando esté autorizada; si no coincide con el literal de esa fila, registrar y detenerse, sin recalcular para hacer pasar el observado.

### 3.2 Procedimiento manual obligatorio antes de cada bloque

1. Sin contacto activo, confirmar el id de bloque, tarea, número de recepciones, SL, modo, cámara y mano contra §5. Leer **el esperado externo H**, ya fijado; si falta, no armar. Comprobar que build/profile y fuente coinciden con el manifiesto, y preparar B→SL1→SL solicitada sin guardar. Aplicar/relanzar y verificar los 106 valores efectivos y el cuerpo/snapshot, no parámetros pendientes o solo la vista «Próximo».
2. Cotejar el hash efectivo mostrado/registrado, el hash de recepción capturado y la referencia externa: **igualdad literal de 12 hex**, cero pendientes. La lectura de preparación de gesto usa la composición de lanzamiento + claves inmediatas de gesto; con configuración estable debe corresponder a H. Registrar esperado y observado en columnas distintas. Un hash del gesto real solo existe después de Began: se coteja al recuperar los datos, sin fabricar una captura anterior.
3. Verificar Jugando/sin pausa manual ni overlay de reanudación, foco de la app, control 2, P2, SL/plan, V100/T3 puro y T3 no suave, cámara, mano y tarea. Confirmar mira/anillo visibles y registrar precheck externo **aceptado/rechazado**, motivo/remedio, build, preset y hashes. **DevHasInputFocus solo informa foco/pausa de la app: abrir DEV no detiene PracticeMode.** Los lanzamientos mientras se configura y entre bloques siguen existiendo y se etiquetan preparación, fuera de los denominadores.
4. **Frontera dev registrada por la app (A1):** cuando el precheck está completo y el PO está listo, el **PO cierra DEV**. Identificar en el archivo del proceso la línea `kind=dev-control` cuyo payload tiene `control=dev-toggle`, `stage=result`, `accepted=true`, `reason=completed` y **`devOpen=false`**. Es el evento de cierre completado; no usar el evento `activation` anterior, una apertura (`devOpen=true`) ni un intento rechazado. Guardar archivo/proceso/número de línea y payload `timeReal`, junto al `ts` UTC de su envoltorio. Esta línea, no la hora del operador ni una frase posterior del chat, es el inicio de ese bloque. La **recepción 1 es el primer LaunchId cuyo lanzamiento empieza después de ese evento**, identificado por la siguiente nueva recepción `shot-plan` y su `launchReal`. El vuelo ya empezado al cerrar DEV queda preparación aunque termine después; también todos los lanzamientos entre bloques. Usar el orden de las líneas de la app para desempatar marcas con igual resolución; si orden e identidad no permiten reconstruirlo sin ambigüedad, registrar y detenerse. Contar las N recepciones sucesivas completas aunque se omitan/fallen; no elegir retrospectivamente la primera con éxito. Tras la salida completa de la N, el PO avisa «terminé [id]» y levanta el dedo. Solo después se recupera/prepara el siguiente bloque; los siguientes LaunchId vuelven a preparación. PROD no tiene DEV y utiliza la frontera específica declarada en §5.
5. Antes de pedir el bloque siguiente, correlacionar recepción/gesto/contacto y respuestas, comprobar hash del gesto y de lanzamiento en cada registro y conservar todos los datos. Si faltó feedback, queda «sin respuesta». Un hash distinto bloquea el avance; no cambiar el esperado para hacerlo coincidir. Conservar la configuración discrepante y detenerse para decisión.

**Relación de relojes (sin crear un reloj runtime):** `timeReal` y `shot-plan.launchReal` pertenecen al reloj real monótono del dispositivo/proceso; son la base para ordenar y medir intervalos. `MetricsLog.Record.ts` es UTC del dispositivo tomado al escribir la línea, no el timestamp original de Ended. Conservar el par `(timeReal del cierre DEV, ts de esa línea)` como ancla aproximada entre ambos dominios del mismo proceso; no sustituir los tiempos originales del gesto por UTC ni usar esa ancla como calibración de latencia.

El Mac anota sus horas **UTC secundarias**, zona local y el identificador de la línea del dispositivo con la que se relaciona cada nota. No se presume que ambos UTC estén sincronizados ni que tiempo de recepción/exportación sea tiempo del evento. En la futura preparación, anotar `M_s` al pedir al PO el cierre y `M_r` al recibir su confirmación inequívoca de haber cerrado; asociar luego el único cierre completado de ese intervalo lógico. Si se necesita describir el desfase entre UTC, ese intercambio solo lo acota aproximadamente: `ts_dispositivo − M_r ≤ desfase dispositivo−Mac ≤ ts_dispositivo − M_s`, con incertidumbre adicional de escritura/resolución/relojes; no es una medición precisa de offset o latencia. Preservar los valores sin corregir. Si falta el intercambio o su asociación, declarar relación Mac/dispositivo desconocida; la frontera válida sigue siendo la línea de la app. Reinicio o cambio de reloj obliga a otro segmento/ancla. No usar horas del Mac para decidir qué LaunchId entra o para desplazar r, A/V o Δ.

No usar los botones **S1 B0–B5** como si contaran recepciones S2 o protegieran estos H. No hay contador/cuenta atrás de estos bloques S2 conectado por esta entrega: el conteo y el armado son externos/manuales, por proceso + LaunchId. Si se propone conectar el hash esperado externo a DEV y añadir contador S2, **queda solo como propuesta** para autorización aparte, con TDD/Claude; no es requisito que se dé por implementado ni código autorizado ahora.

Cada reapoyo tiene su id y enlace, pero no crea otra recepción de la cuota. Un Swing consume el intento de juego aunque no haga contacto; una cancelación no amplía K. Omisiones, inválidos, Tap, C-A, rechazos y fallos cuentan en la recepción que les corresponde. Extras antes/después, adelantamientos del PO e interrupciones se preservan como desviaciones, sin sustituir resultados. No repetir para completar diez contactos exitosos.

Reinicio/segundo plano: interrumpir el bloque, preservar parcial y motivo, restaurar/verificar P0 y abrir segmento de proceso nuevo; volver a realizar el precheck completo. No continuar automáticamente un bloque parcial ni reutilizar ids como globales. Su reanudación/repetición requiere decisión del PO y se registra como bloque adicional, sin borrar el parcial. Descansos solo entre bloques, con P0 restaurado si termina la sesión y con nuevo precheck al volver.

## 4. Explicación al PO y convenciones de la tarea

Antes de FAM, decir literalmente:

> Ahora sí hay golpe de pelota. Apoya en el frontis para elegir dónde; bajar el pulgar carga potencia. Debes soltar cuando toca golpear: el apoyo no golpea. Para cancelar, vuelve a la altura del apoyo o más arriba y suelta. Si debes corregir el punto, cancela y apoya otra vez; ese tiempo cuenta.

Pedir, por separado y antes de medir: «¿Puedes señalar la mira que fija dónde apuntas?», «¿Puedes señalar el anillo que indica cuándo soltar?» y «¿Puedes explicar cómo cargas y cómo cancelas?». Registrar respuestas literales; si no identifica/entiende los elementos, detenerse sin inventar una prueba entendida ni añadir ayudas. Esta sesión no usa una cuenta atrás S2 inexistente ni reutiliza como confirmación la de S1.

Referencia común **C**: buscar el centro del ancho del frontis, a altura media válida (referencia nominal x=3,2/y=2,2 m), antes de tensar. Es una **referencia verbal sin marcador**; no se añade ninguno ni se presume que el PO identificó un píxel/calibración exactos. Informar descriptivamente la distancia en metros **Aim(Began)→C = sqrt((Aim.x−3,2)²+(Aim.y−2,2)²)**, con n/p50/p95/máximo por bloque/condición y los casos sin Aim por separado. Esa distancia nominal es calculable cuando hay Aim y no se declara NotEvaluated por no haber marcador; se etiqueta siempre «referencia verbal sin marcador», sin interpretarla como precisión respecto de la intención humana real ni criterio de aceptación. Se separan intención humana, Aim, objetivo factible, error del solver e impacto. Si falta Aim, la distancia de ese caso no se inventa. No hay umbral nuevo de puntería.

**Gesto de golpe común G:** «Busca C al apoyar. Baja el pulgar para cargar y suelta según el anillo; el golpe exige frontis y un bote de recepción. Si tienes que corregir, vuelve, suelta y apoya otra vez dentro de la misma oportunidad. No arrastres para mover el objetivo después de apoyar». No se prescribe un mínimo de duración del gesto, una anticipación ni una potencia numérica nueva.

Antes de pedir feedback, dar estos ejemplos de la triada: fijación/orden querida y reconocida aunque falle puntería/timing → **OK**; apoyo no querido → **Apoyo sin querer** (`premature=true`); fijación no obtenida/identificada o fallo operativo → **No fijó / falló**, con motivo aparte. Ni OK aprueba puntería/timing/S2, ni un resbalón simulado demuestra una fijación espontánea involuntaria. No reinterpretar un cierre técnico a partir de esa respuesta.

## 5. Orden fijo de bloques propuestos [EXPERIMENTO]

Se pide **solo el siguiente bloque**, después de recuperar/cerrar el anterior y realizar §3. Cantidades de recepciones y orden son diseño de la sesión, no criterios de aceptación ni nuevos defaults. No hay duración mínima/máxima del gesto añadida. Cámara fija salvo CAM-S; misma mano/control en todos. Toda fila tiene su propio precheck y cierre, aunque comparta H con otra.

| Orden / id | Receta / modo / hash de condición | Recepciones previstas | Tarea e instrucción específica al PO, además de G cuando corresponda |
|---|---|---:|---|
| 01 FAM | SL1 / V100 / H(1,V,0) | 6 | Familiarización: 1 apoyo y retirada sin carga; 2 cargar y cancelar por retorno; 3 suave, 4 medio y 5 fuerte buscando C y el anillo; 6 apuntar a un punto, cancelar y reapoyar para corregir antes de soltar si queda oportunidad. Registrar lo ocurrido aunque no coincida; no repetir hasta entender o acertar. Comprensión exigida antes según §4. |
| 02 SL1-V | SL1 / V100 / H(1,V,0) | 10 | G, potencia media voluntaria. «Son diez recepciones con ventana V100; omisiones y cancelaciones también cuentan». |
| 03 SL1-T | SL1 / T3 puro / H(1,T,0) | 10 | Misma tarea, receta, mano y referencia; «Ahora T3 puro, sin límite V; sigue buscando el anillo». |
| 04 SL2-T | SL2 / T3 puro / H(2,T,0) | 10 | G, potencia media; primera condición de SL2. |
| 05 SL2-V | SL2 / V100 / H(2,V,0) | 10 | Misma tarea; cambia solo la condición de ventana respecto de SL2-T. |
| 06 SL3-V | SL3 / V100 / H(3,V,0) | 10 | G, potencia media; posición de cuerpo propia de SL3. No recolocar para rescatar el alcance. |
| 07 SL3-T | SL3 / T3 puro / H(3,T,0) | 10 | Misma tarea y receta que SL3-V. |
| 08 SL4-T | SL4 / T3 puro / H(4,T,0) | 10 | G, potencia media; primera condición de SL4. |
| 09 SL4-V | SL4 / V100 / H(4,V,0) | 10 | Misma tarea y receta que SL4-T. |
| 10 SL5-V | SL5 / V100 / H(5,V,0) | 10 | G, potencia media. «Este vuelo puede dejar poco tiempo; no apoyes antes de la recepción ni cambies el gesto para que el ensayo salga bien. Conservaremos también las omisiones». |
| 11 SL5-T | SL5 / T3 puro / H(5,T,0) | 10 | Mismo vuelo/tarea; observar S2-TIME-01, sin alargar A/V, ralentizar ni cambiar SL. |
| 12 SL6-T | SL6 / T3 puro / H(6,T,0) | 10 | G, potencia media; misma advertencia de tiempo de SL5. |
| 13 SL6-V | SL6 / V100 / H(6,V,0) | 10 | Misma tarea y vuelo; observar todos los intentos, sin ajustar. |
| 14 POT-S | SL1 / V100 / H(1,V,0) | 10 | «Busca C y el anillo; intenta un golpe voluntariamente suave en cada recepción». No pedir solo ritmo cómodo ni interpretar Tap/C-A como una potencia suave lograda. |
| 15 POT-M | SL1 / V100 / H(1,V,0) | 10 | Misma tarea, potencia media voluntaria en las diez recepciones. |
| 16 POT-F | SL1 / V100 / H(1,V,0) | 10 | Misma tarea, potencia fuerte voluntaria; no imponer esfuerzo máximo ni corregir `pullFullMm=35`. Separar fuerte solicitado de saturación realmente registrada. |
| 17 CAN | SL1 / V100 / H(1,V,0) | 5 | «Carga, vuelve a la altura donde apoyaste o más arriba y suelta para cancelar; después reapoya y busca un golpe medio si queda tiempo». Conservar primer apoyo, cancelación y reapoyo; si no alcanza el tiempo, no prolongar la recepción. |
| 18 NEG-A | SL1 / V100 / H(1,V,0) | 3 | Apoyo sin tirón y retirada; incluir una duda breve, sin exigir medirla con el dedo. No se pide Swing ni P=0 mediante tirón intencional. |
| 19 NEG-B | SL1 / V100 / H(1,V,0) | 3 | Apoyo y pequeño temblor/rozamiento, sin intención de cargar; registrar desplazamiento real y cierre, sin forzar que haya quedado en la zona muerta. |
| 20 NEG-C | SL1 / V100 / H(1,V,0) | 3 | Imitar un resbalón descendente tras apoyar, sin buscar un golpe. Puede dar Swing/contacto: es el riesgo N04 conocido, no una prueba de detector de voluntad ni estimación de frecuencia espontánea. |
| 21 NEG-D | SL1 / V100 / H(1,V,0) | 3 | Imitar un resbalón mayormente lateral con componente descendente pequeña, sin buscar carga. Registrar la trayectoria y si corresponde al caso N04b; no exigir al PO milímetros exactos ni forzar P=0. |
| 22 RIT | SL1 / V100 / H(1,V,0) | 10 | «Busca C y golpea a ritmo cómodo; puedes corregir cancelando y reapoyando. No hay secuencia de intensidades que debas seguir». Preguntar qué intentó realmente. Es práctica repetida, sin rival ni rally implementado. |
| 23 CAM-F | SL1 / V100 / H(1,V,0) | 10 | G, potencia media, cámara fija; referencia para observación de cámara, separada de la comparación principal. |
| 24 CAM-S | SL1 / V100 / H(1,V,1) | 10 | Misma tarea con seguimiento. «La mira puede cambiar de píxel al moverse la cámara, aunque el objetivo del muro quede fijo». No se retoca cámara/zoom ni se mezcla con CAM-F. |
| 25 PROD | SL1 / V100 / H(1,V,0), profile prod | 3 | Recorrido funcional independiente: observar lanzamiento, buscar C, golpear y reconocer nota y sonido sin DEV. Tres recepciones conservadas aunque falte contacto; no repetir hasta lograrlo ni inferir funcionamiento de símbolos/export. |

**Total propuesto: 203 recepciones dev + 3 prod = 206**, no resultados ni una muestra garantizada de 206 contactos. Doce bloques principales suman 120 recepciones (60 V100/60 T3 puro). El orden V→T en SL impares y T→V en pares alterna la primera condición; no es aleatorización ni elimina aprendizaje/fatiga. Potencia suave→media→fuerte y cámara fija→seguimiento tienen sesgos de orden predeclarados. FAM, negativos, CAN, RIT, cámara y prod se informan por separado, sin inflar la comparación principal.

### 5.1 Tandas predeclaradas (A4)

| Tanda | Bloques en el orden anterior | Recepciones previstas | Inicio y cierre |
|---|---|---:|---|
| Tanda 1 dev | 01 FAM + 02–13 | 126 | Precheck completo de tanda y de cada bloque; fronteras dev-toggle independientes. Al cerrar: exportar logs, restaurar P0 en **proceso nuevo** y cotejar byte a byte los tres archivos según §8. |
| Tanda 2 dev | 14–22 | 57 | Nuevo precheck completo desde P0 verificado; conservar el orden y las cuotas. Misma exportación/restauración nueva/cotejo al cerrar. |
| Tanda 3 dev | 23–24 | 20 | Nuevo precheck completo, fija→seguimiento por separado. Misma exportación/restauración nueva/cotejo al cerrar. |
| Tanda 4 prod | 25 PROD | 3 | Requisitos de instalación prod y nuevo precheck completo. Frontera app específica en §5; exportar/restaurar P0 en proceso nuevo/cotejar los tres archivos al cerrar. |

**Todas las tandas, incluida la restauración final y cualquier instalación necesaria, deben caer dentro de la vigencia del perfil renovado por el PO y comprobado.** Registrar inicio/vencimiento efectivo UTC del incorporado y tiempos de cada tanda. Verificar de nuevo antes de cada tanda que sigue siendo el mismo perfil válido para todo el tramo previsto; si no permite realizarla, no iniciar ni mantenerla más allá de su vigencia. Se necesita renovación/comprobación y autorización correspondientes antes de otro tramo, sin trasladar una tanda parcial como si estuviera completa. No se fija aquí una fecha de ejecución ni se presume continuidad entre tandas/días/procesos. La recepción de restauración o espera entre tandas es preparación, no muestra del bloque siguiente. Cada cierre de tanda aplica §8, aunque no se vaya a instalar otra build.

Si el PO no desea o no puede terminar una tanda, conservar el parcial y detener/interrumpir según §7; no acortar y presentar la tanda como completa. La inclusión de CAM-F/CAM-S y PROD en este documento no autoriza su ejecución o instalación.

PROD usa la misma fuente de producto, no necesariamente el mismo gitHash documental de build. Antes de instalar prod: exportar dev y restaurar/verificar P0 (§8); respaldo nuevo y requisitos §2. Con DEV inexistente, comprobar build/profile, P0 y H en logs de arranque/recepción **antes** de pedir PROD, y hashes de gestos al recuperar. Sin una lectura fiable previa no se hace el recorrido ni se supone que hereda el estado temporal de dev. El precheck/recuento externo y los juicios se conservan fuera de la app; no crear controles prod ni editar archivos para configurar la campaña. **Frontera específica PROD:** dado que no hay DEV, no se inventa un dev-toggle. Tras el precheck de build/perfil/P0 ya realizado, el PO abre el proceso nuevo cuando está listo. El marcador es el `shot-plan` del lanzamiento SL1 que realiza `PracticeMode` al terminar la inicialización de `AppBootstrap`, con H(1,V,0): en la fuente aprobada es el segundo LaunchId del proceso. **No usar el primer shot-plan transitorio**, que `ShotController.Initialize` emite sobre el lanzamiento inicial First Ball antes de inicializar práctica. Identificar ambos por orden/LaunchId y conservarlos como preparación; si esa secuencia de arranque no coincide, registrar y detenerse. La recepción 1 de PROD es el primer LaunchId que empieza **después** de esa línea; las tres siguientes recepciones consecutivas constituyen la cuota, sin seleccionar por éxito. Los dos lanzamientos iniciales quedan preparación y permiten cotejar el hash/identidad registrados antes de la cuota; si no se puede completar ese cotejo antes de su primer lanzamiento contado, registrar/interrumpir, sin mover retrospectivamente la frontera. Conservar marcador, orden de líneas, LaunchId y horas secundarias igual que en dev. Esta excepción de PROD queda explícita para revisión de Claude antes de ejecutar; no añade ningún evento ni control.

## 6. Preguntas literales y métricas previstas

### 6.1 Feedback y juicios

**Triada por intento solo en FAM, CAN y NEG-A–D (A2).** Después del terminal del intento y sin intervenir durante el contacto activo, preguntar «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?». Conservar respuesta/explicación e id/ordinal; si no puede asociarse, dejar «sin respuesta». En NEG-C/NEG-D registrar además que el resbalón fue **solicitado para simular el riesgo**, separado de cualquier apoyo involuntario declarado espontáneamente. No ampliar una recepción para obtener feedback ni forzar respuesta si falta tiempo.

**En V/T, potencia, RIT, CAM-F/CAM-S y PROD no se pregunta la triada durante los intentos.** Al cierre del bloque pedir: «Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad?». Asociar solo lo que el PO señale inequívocamente; el resto queda **«sin respuesta», nunca OK por defecto**. «Ninguna excepción» es una respuesta global literal, no diez OK individuales fabricados. En los bloques V/T no se interrumpe el ritmo para pedir feedback por intento. Las preguntas de tarea y las tres de 2.7 se hacen al cerrar, como se indica a continuación.

Después de cada bloque, preguntar: «¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer?». En RIT recordar que no se pidió una secuencia de intensidades. No convertir un «sí» ambiguo en confirmación de varios apartados.

Para cada condición de los bloques principales y tras CAN/RIT/CAM-F/CAM-S, recoger **por separado**, con contexto de bloque, las tres preguntas originales:

1. «¿la pelota va donde apunto?»
2. «¿el timing es justo?»
3. «¿el salto es aceptable?»

La segunda respuesta es preliminar: no se calibró latencia/H3; la tercera necesita juicio del PO antes de fusionar S2, nunca se deduce de percentiles. Tras PROD, preguntar una a una: «¿Viste el lanzamiento?», «¿Pudiste dar un golpe?», «¿Viste la nota del resultado?» y «¿Oíste el sonido?». Registrar respuesta y si hubo contacto observado, sin sustituir una omisión o fallo funcional.

Al cierre de la tanda 3 dev, conservar **las 16 preguntas originales de S1 literalmente**, con respuestas nuevas, individuales y ligadas a esta sesión, sin copiar las respuestas [del día 30](evidence/2026-10-30-m24-po-answers.json):

1. «¿Puedo colocar la mira donde quiero?»
2. «¿Puedo apuntar bajo sin activar accidentalmente la potencia?»
3. «¿La mira permanece estable después de fijarse?»
4. «¿Cuántas fijaciones involuntarias ocurren?»
5. «¿Cuánto tiempo necesito para fijar la mira?»
6. «¿Entiendo claramente cuándo quedó fijada?»
7. «¿Puedo conseguir golpes suaves, medios y fuertes voluntariamente?»
8. «¿La potencia responde de forma consistente a la distancia?»
9. «¿Puedo alcanzar la potencia máxima cómodamente?»
10. «¿Existen limitaciones por el borde inferior de la pantalla?»
11. «¿Puedo cancelar cuando realmente quiero?»
12. «¿Ocurren cancelaciones accidentales?»
13. «¿La zona interfiere con los movimientos normales?»
14. «¿Cuánto demora el gesto completo, desde apoyar el dedo hasta soltar?»
15. «¿Se siente natural o demasiado mecánico?»
16. «¿Podría utilizarlo repetidamente durante un rally rápido?»

Añadir, por separado: «Con el golpe real, ¿sigues prefiriendo tirar hacia arriba o adelante? ¿Por qué?». Es juicio de preferencia, no comparación de direcciones: solo se usa el tirón descendente aprobado. «No recuerdo» no es cero; estimaciones subjetivas de duración no son mediciones; ninguna respuesta de práctica acredita rally/partido, aceptación de otra fase o calibración.

### 6.2 Registro y análisis descriptivo, sin criterios nuevos

La hoja externa por bloque contendrá esperado/observado de hash, build/profile/fuente, preset/archivo, modo/cámara/mano/tarea, frontera de la app (archivo/proceso/línea, timeReal/ts y devOpen) y horas Mac secundarias con asociación/incertidumbre según §3.2, cierre y zona local, tanda/proceso/segmento, rango de LaunchId y cuotas previstas/observadas, precheck/motivo, interrupciones/adelantamientos/ausencias y preguntas/respuestas literales. Enlazar las trazas originales por proceso + LaunchId + attemptId/versión; no usar solo el ordinal visible ni sobrescribir una omisión reconciliada.

| Grupo | Descripción prevista y denominadores/límites |
|---|---|
| Completitud | Recepciones previstas, comenzadas, completas e interrumpidas; todos los gestos, reapoyos, cierres, omisiones, Swing, programados, aplicados y sin contacto, con motivo. Separar preparación/extras y todos los registros sin feedback. Un reconciliado conserva sus versiones y representa la misma recepción. |
| Tiempo / S2-TIME-01 | Por SL/modo: lanzamiento→frontis/primer bote/K exacto/T*, Began y Ended original/corregido, duración Began→Ended, anticipación desde lanzamiento y `T*_real−Began`, Δ, categoría y fronteras A/V/K. En SL5/6 informar todas las omisiones/tempranos/tardíos y cuántos llegaron a V/T3 puro. No convertir medianas de S1 en duración mínima ni «poco tiempo» en nuevo Fail humano. |
| Potencia | P, distancia descendente estimada en mm/DPI o fallback, distancia máxima/lateral, banda solicitada/declarada y clasificación con **las bandas vigentes**, saturación P=1. Informar conteos sobre todas las recepciones/gestos y sobre Swing/contactos por separado. Tap/C-A no se eliminan para mejorar bandas; banda no aplicable sin Swing queda identificada. Advertencia histórica de saturación fuerte 50% frente al 20% existente conservada, sin nuevo corte ni tuning. |
| Cancelación/riesgo | Primer objetivo y error si la referencia es conocida, costo temporal de cancelar/reapoyar, enlaces, N2/C-A, terminal y consumo. N04/N04b: desplazamiento, P y contactos posibles, intención declarada separada; resbalones solicitados no estiman tasa accidental espontánea. |
| Apuntado/solver/cámara | Error nominal Aim(Began)→C (3,2/2,2), n/p50/p95/máximo, etiquetado **referencia verbal sin marcador**, sin umbral ni equivalencia con intención real; casos sin Aim separados. Aim del Began inmutable, proyección realmente usada/ObservedAt/lockObservedAt/O2, objetivo usado/factible, SpeedRaised/TargetLowered, velocidad total, contacto/impacto y error del solver. La distancia nominal a C se informa como referencia verbal; no demuestra precisión frente a la intención real del PO ni se confunde con tolerancia del solver, movimiento de cámara o salto. Fija/seguimiento separados. |
| Rollback/salto | Edad de orden original, cobertura/caducidad del historial, paso/fracción/error de cuantización firmado/vecino legal, pasos resimulados, eventos/efectos obsoletos. Salto p50/p95/máximo en metros, píxeles y diámetros, mismo instante/cámara, sin suavizado. Si no hay píxeles/cámara, no convertir el cero serializado en medida: usar HasJumpPixels y declarar ausencia. |
| Costes / A09 | Planificador, solver, reconstrucción, predicción y reconciliación registrados: n, p50/p95/p99/máximo por parte y condición, con unidad ms y datos originales. No sumar percentiles para inventar el percentil del frame del golpe; no repetir un coste de planificación de lanzamiento en cada gesto como muestra independiente. El frame completo y asignaciones por parte solo se informan si existe evidencia nativa correlacionable; si no, **NotEvaluated/no disponible**, sin añadir instrumentación en esta entrega ni inferirlos de promedios agregados. Una necesidad de medirlos mediante código requiere propuesta/autorización aparte. No se declara H17 aprobado ni se hereda su desviación de First Ball. |
| Contexto | Foco/pausa/tempo/cámara lenta/descartes/pendientes, térmico/batería/ruta de audio y condiciones disponibles; desconocido explícito cuando falte. No correr Escenario/P2/H17, calibración H3, experimentos DSP/S4 ni medir trabajo CPU restando esperas. Segmentar cambios de contexto. |

Para costes y saltos **crudos** se predeclaran percentiles por rango más próximo: ordenar las n muestras y tomar el elemento de posición ceil(p·n), con p=0,50/0,95/0,99 según corresponda; sin interpolación y sin estimar percentiles si n=0. El máximo es el observado. No se cambia el método por cubetas de los agregados de frames existentes ni se los mezcla con percentiles crudos; se identifica el método al citar cada serie. Correlacionar versiones append-only y usar una sola muestra final por orden aplicada, manteniendo los originales y las reconciliaciones. Planificador: una muestra por lanzamiento, no por cada cierre; un coste de parte no ejecutada/desconocida no es una medición de 0 ms por aparecer un cero por defecto. Informar n por métrica/condición y ausencias/NotEvaluated, que no cuentan como aprobado. No hay umbral nuevo de tamaño muestral, salto, rendimiento, puntería, potencia o juicio. Todos los intentos se conservan y las exclusiones de comparación se justifican sin borrarlos. Datos nuevos separados de S1/311cb11/otras sesiones; sin replay humano ni recalcular mediciones antiguas como nuevas.

## 7. Parada y conservación

- **Antes del bloque:** sin autorización, perfil renovado/comprobado, respaldo/cotejo, esperado externo, identidad/hash/snapshot, cero pendientes/Jugando/foco o comprensión confirmada: no empezar. Registrar qué falta y el remedio; no saltar al siguiente.
- **Durante la futura sesión:** ante otro defecto distinto de las observaciones conocidas, imposibilidad de ver/entender mira/anillo, discrepancia de fuente/configuración, contacto/estado inesperado del sistema, fallo de persistencia/recuperación o evidencia insuficiente para reconstruir el bloque: conservar y detenerse. No arreglar, instalar otra build o ajustar sobre la marcha.
- Pausa/foco/DEV/viewport/configuración/reinicio interrumpen comparabilidad del tramo afectado. Conservar el parcial y todos sus cierres; no promover un dedo sostenido ni fabricar Ended. Si el PO pide parar o se fatiga, detenerse y restaurar; no insistir en completar la cuota.
- S2-TIME-01 continúa como **observación conocida** de SL5/6, no bloqueo previo ni motivo para retocar SL/V/A/tempo. N04/N04b pueden producir Swing, y saturación puede reaparecer: registrar y elevar juicio sin declararlos resueltos ni contarlos como nuevos fallos de infraestructura. Una manifestación técnicamente distinta se registra como defecto nuevo y se detiene. Ante duda, no atribuir causa ni hacer cambios.
- Terminar/interrumpir implica exportar evidencia y restaurar P0. Si una operación de restauración no puede comprobarse por bloqueo/falta de acceso autorizado, marcarla **pendiente**, preservar evidencia y detenerse; no afirmar éxito ni continuar bloques/instalaciones. No confundir parar la campaña con abandonar silenciosamente la restauración.

## 8. Restauración de P0, recorrido prod y entrega futura

**Al terminar cada una de las cuatro tandas**, antes de cambiar a prod y ante parada/fin de una sesión interrumpida: no pedir más gestos; recuperar logs y archivos disponibles **antes de nuevas aperturas/retención**, incluidos registros que lleguen después del evento de fin del bloque. Conservar integridad/hash de originales y separar resúmenes/desviaciones. No descartar el último intento ni etiquetar un parcial como completo.

Restaurar selección P0 y cualquier estado temporal solo con el acceso autorizado. Cotejar P0-calib-1, Po-1 y last-active byte a byte contra el respaldo correspondiente y contra la referencia inicial; no guardar los valores temporales de campaña. Abrir **proceso nuevo**, observar P0 y su hash extendido S2 real (referencia `dd122831f592` para la fuente aprobada), 106 claves/41 heredadas, cero pendientes, control2 y selección P0; registrar UTC/build/log y proyecciones. Comprobar de nuevo los tres archivos **después del arranque**, no solo antes. Volver a P0 no significa volver a los defaults: se conserva repeat3, suspendido por práctica según la configuración heredada.

Solo después de restauración dev comprobada, y de autorización propia de la instalación prod con perfil renovado/comprobado y respaldo/cotejo nuevos, podría realizarse PROD (§5). Su observación es funcional y separada. Tras PROD se repite restauración en proceso nuevo/cotejo, sin reinstalar dev por inferencia. Si el PO decide una reinstalación posterior, necesitaría su autorización y los mismos requisitos.

El informe futuro debe conservar todos los bloques y parciales, preguntas/respuestas literales, conteos/metadatos/métricas y limitaciones, restauración observada o pendiente y juicio explícito del PO sobre el salto. **La PR #30 sigue sin fusionar hasta ese juicio; ni completar cuotas ni aprobar fase 1 autoriza el merge.** Después se entrega para Claude/PO y se detiene. Este documento no inicia 2.7, no cierra S2 y no autoriza S3–S4/T8.
