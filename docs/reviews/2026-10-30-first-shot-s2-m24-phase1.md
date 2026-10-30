# First Shot S2 fase1 — M2-4, tareas2.0–2.6

Base8dd142dff2fcdc5d52871ac3c89733b4aff88fd6, rama firstshot/s2-m24. AutorizaciónPO2026-10-30 en [registro](2026-10-25-first-shot-po-approvals.md); [plan/D4](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Trabajo en curso; no se declara completada ninguna tarea sin evidencia.

## Histórico — parada en S2-TIME-01

**Fase1 incompleta.** 2.0 y2.1 implementadas/probadas; en2.2 los14focales pasan pero SL5/SL6 dejan margen incompatible con V100 para un gesto de la duración mediana de mediosS1. Se aplica «registrar y detenerse» delPO antes de2.3, sin tuning ni sesión. El detalle y los límites de ese análisis están al final. No se declara regresión/cierre técnico completo después de añadir las siete claves2.2.

## Estado vigente — reanudación por el PO

PO2026-10-30: **«Seguir sin cambiar SL»**; [registro literal/interpretaciónClaude](2026-10-25-first-shot-po-approvals.md). La parada y sus cálculos se conservan como historia. S2-TIME-01 pasa a observación predeclarada2.7, sin nueva sesión ni modificación de recetas. Se reanudan hashes92claves/2.2 y luego2.3–2.6 con TDD; aún no se declara fase completa. Otro defecto o incompatibilidad requiere nueva parada.

## Alcance y línea base

2.0primero/hashreales, luego2.1–2.6enorden/TDD. No Xcodebuild,instalación,acceso al iPhone,sesión2.7 ni prod físico. Bloques experimentales2.7pendientes dePO. TablaD4.2decisión aprobada, no propuesta;se conserva referencia histórica del plan y literal deaprobaciones. N04bcorregido solo como texto. CierrePR29 registrado aquí porremisión alregistro, sincommitaparte enmain.

Regresión inicial fresca:441/441EditMode y207/207PlayMode, sinCS/ceroomitidos. TransitoriosGraphicsSettings/QualitySettings restaurados byte a byte antes de implementar. Artefactos ignorados enLogs/evidence/s2-tdd, archivos y comienzo de ejecución verificados por scripts/test.sh. EntornoUnity preparado con caché local previa, pero XML/log de cada ejecución nuevos;sinreutilizar resultados anteriores.

## Progreso

Histórico inicial: 2.0 RED antes de cambiar defaults; resultados posteriores y parada se registran a continuación, sin alterar ventanas, parámetros ni SL.


## 2.0 — promoción de M2-4

RED válido:6testsEditMode/3fallos por aserción (valor por defecto, delta de valores, overlayP0) y1PlayMode/1fallo por ausencia deM24 enMain. GREEN6/6y1/1 tras cambiar exclusivamente gesture.control0→2 enDefaultPreset. FixturesA′seleccionan0explícitamente yS1-sling1, sin debilitar aserciones; expectativashash revisadas por promoción autorizada. Suites completas frescas447/447EditMode/208/208PlayMode,sinCS;transitoriosGraphics/Quality restaurados byte a byte. [Resultados y SHA de XML](../validation/evidence/2026-10-30-s2-task20.json).

85claves; defaults **1c8e598b099b**, P0legacy41claves **944668168805**, calculados porUnityenlos tests, no observados en iPhone ni instalados. P0/Po-1/last-active de un almacén temporal de prueba permanecen byte a byte iguales; no se accede al dispositivo ni se reescriben archivos del PO.41heredados/proyección0d39e6016bd8, P0histórico8de3f2ecb217 ySimulationKey166a96e319c7 intactos.2/35mm/N2estricto intactos. La app instalada anterior no cambia por esta promoción en la rama.


## 2.1 — mapa del reloj por lanzamiento

Esqueleto compilable/RED9/9 fallidos por aserción, luego RED12/12 al cubrir observaciones de frames, descarte explícito y paso manual pausado. GREEN12/12; suite completa EditMode459/459 sinCS. Primera implementación11/12 por un ulp bajo el entero24 después de pausa: se conservó la aserción, normalizando solo error de representación double, no ms de juego. [Evidencia](../validation/evidence/2026-10-30-s2-task21.json).

ClockMap observa el SimClock existente, no lo avanza. Segmentos reales/simulados en double; pendientes futuros extrapolados con tempo vigente, muestras anteriores al lanzamiento rechazadas. Pausa conserva posición; avance manual no se interpola como continuo. Como decisión técnica de interfaz, Mark admite un indicador opcional de pasos descartados del SimClock (llamadas originales de seis argumentos conservadas); el intervalo descartado es desconocido/no mapeable, no se deduce de diferencias menores entre reloj real y observación de frame. Captura de τ/cámara lenta/pausa invalida el tramo de gesto correspondiente. Ninguna integración de host2.5 aún.

## 2.2 — predeclaración histórica de recetas (commit 0c638da, antes de ejecutar P2)

El plan/spec dan las seis categorías SL pero no sus coordenadas numéricas. Se fijan estas recetas iniciales **[EXPERIMENTO]** para implementar esos casos, antes de ejecutar su comprobación; no se derivan de L1–L6 ni se reajustarán para hacer pasar el alcance o el tiempo. Velocidad de la tabla = componente horizontal de la entrada, a convertir con Ballistics.SolveCenterTo existente, no velocidad total de la salida2.4. Radio físico0,02m/gravedad/tempo permanecen los de configuración. Objetivo del frontis usa su centro físico desplazado por radio como ya hace Ballistics.CenterForContact.

| SL / categoría | Apoyo de pelota inicial (x,y,z) m | Objetivo frontis (x,y) m | Velocidad horizontal m/s | Cuerpo fijo (x,z) m |
|---|---|---|---|---|
| SL1 media altura drive | (2,6;1,2;7) | (2,6;2,2) | 15 | (3,2;5,5) |
| SL2 media altura revés | (3,8;1,2;7) | (3,8;2,2) | 15 | (3,2;5,5) |
| SL3 esquina trasera drive | (0,35;1,3;7) | (0,35;1,5) | 18 | (1,3;8,3) |
| SL4 esquina trasera revés | (6,05;1,3;7) | (6,05;1,5) | 18 | (5,1;8,3) |
| SL5 adelante drive | (2,6;1;3) | (2,6;0,7) | 6 | (3,2;1,6) |
| SL6 adelante revés | (3,8;1;3) | (3,8;0,7) | 6 | (3,2;1,6) |

Son valores iniciales de experimentación, no criterios de aceptación. Posición/punto de alcance se capturan al lanzar. Interpretación de implementación de M04: alcance euclídeo3D desde el centro del cuerpo de la cápsula existente (y0,9m de PlaceholderPlayers), independiente de la banda de altura preferida. Mano del personaje diestra; con AX-1 drive al lado de x menor que el cuerpo, empate exactamente sobre eje→drive, sin nueva franja de umbral. U suma altura preferida y separación de paredes con pesos iniciales iguales **[EXPERIMENTO]**, desempate por paso menor; no bonifica alcance ni añade estocada. Si estas concreciones o los vuelos revelan una incompatibilidad, registrar/detener y elevar al PO, sin ajustar las recetas congeladas.

## Parada S2-TIME-01 — incompatibilidad experimental antes de 2.3

**Estado: fase 1 incompleta y detenida por la condición explícita del PO.** No se inicia 2.3–2.6, no se ajustan las recetas/velocidades/ventanas ni se permite apoyar antes de Recepción para rescatar el resultado. No hay build Xcode, instalación, acceso al iPhone, sesión 2.7 o recorrido funcional prod.

2.2 alcanzó **14/14 focales EditMode sin CS**, después de RED12tests/10fallos y RED14tests/11fallos por aserción con esqueletos compilables. Incluye seis trayectorias P2 alcanzables, cuerpo fijo, lado/altura legal/banda preferida separados y fronteras exactas de eventos. El planificador no cambia P2 ni L1–L6. El cálculo estático usa intervalos balísticos delimitados por contactos y raíces acotadas de altura/esfera de alcance; por frame todavía no se conecta ningún controlador. La referencia corporal 3D y las recetas iniciales están predeclaradas en **0c638da**, antes del primer cálculo. [XML resumidos, entradas, tiempos y comparación](../validation/evidence/2026-10-30-s2-time-01.json).

Tiempos **reales** de la evaluación determinista en editor con P2, τ=0,65, reloj iniciado en0 al lanzar (no medición física ni replay de gestos):

| Casos | Primer frontis (s) | Primer bote (s) | k_in (s) | T* (s) | k_out (s) |
|---|---:|---:|---:|---:|---:|
| SL1 / SL2 | 0,715897 | 1,741539 | 1,757497 | 2,025641 | 2,031727 |
| SL3 / SL4 | 0,596581 | 1,441662 | ≈1,885072 | 1,897436 | ≈2,114405 |
| SL5 / SL6 | 0,764103 | 1,181003 | 1,209189 | **1,448718** | **1,694601** |

La mediana **ya medida** Began→Ended de los mediosB2 de S1 es **1,662776708s**; se cita su JSON, no se vuelve a medir ni simular el gesto. Caso reproducible de análisis **[EXPERIMENTO]**: un gesto de esa duración que empieza exactamente al lanzar SL5/SL6 acaba con **Δ=+214,059ms**. Con V100 y compensación inicial0, la suelta más tardía admitida es1,548717949s: para entrar enV, ese gesto exigiría Began≤**−0,114058759s**, antes de Recepción. En T3puro, el margen más tardío de Began es **+0,031824489s** respecto al lanzamiento. No se adopta esta cifra como umbral de reacción humana ni se modifica el propósito fijado al Began (§6.5.1).

**Es una incompatibilidad de la combinación de recetas iniciales, V100 y esa duración observada; no una afirmación de que todos los gestos sean imposibles.** En S1 hubo duraciones distintas; no se convierte una mediana en duración mínima, criterio de aceptación o Fail de la capacidad del PO. El resultado se eleva al PO **sin reajustar** SL5/SL6, velocidad, tempo, A/V, compensación, N2 o dirección. Debe decidir cómo resolver el margen antes de reanudar la integración. Esta entrega no elige cambiar vuelos, preparar el gesto fuera de la fase, ni otro control.

### Qué queda terminado y qué no se declara

- **2.0 terminado:** promoción en la rama con sus commits propios y suites frescas447/447EditMode y208/208PlayMode. Defaults de esa tarea1c8e598b099b/P0extendido944668168805, computados enUnity y no observados en iPhone. Los fixturesA′son explícitos y S1-sling conserva selección1; P0legacy41claves hereda2 sin reescribir archivos persistidos.
- **2.1 terminado como componente Core:** RED→GREEN12/12 y suiteEditMode459/459; aún no integrado al host. La evidencia conserva la primera pasada11/12 y su corrección de representación double, sin cambio de ventana.
- **2.2 en progreso, detenido al encontrar el margen:** focal14/14, recetas/alcance/tiempos conservados sin tuning. Se añadieron únicamente las siete claves body/legal previstas por el plan; hay92claves. Los únicos valores existentes cambiados frente a8dd142d son gesture.control0→2 de2.0; los41heredados, SimulationKey y2/35mm/N2 no se cambian. **El hash global después de estas siete adiciones todavía no se ha observado porUnity. No se atribuyen al estado actual los hashes85claves de2.0.**
- **Pendiente a la parada:** actualizar/verificar expectativas de hash/cantidad de claves contra el nuevo estado92claves, regresión completa nueva de2.2 y cierre de fase con PlayMode/check-release dev/prod gitDirtyfalse. Los tests antiguos que fijan el hash85claves todavía necesitan esa actualización; no se afirma que la suite actual completa esté verde. No se ejecutan verificaciones extra después de la condición de parada para simular un cierre completo.
- **2.3–2.6 no iniciadas.** ShotController/contacto/solver/rollback/práctica/vistas/audio no están implementados, ni hay una entrega jugableS2. TDD y casos M2S2-N01–N12 del resto siguen pendientes. La revisión final completa de la skill se difiere porque el PO ordena parar ante incompatibilidad; no se sustituye por una afirmación de aprobación propia.

Transitorios Graphics/Quality de PlayMode2.0 restaurados byte a byte. Sin cambios en specv0.3, P2, L1–L6, criterios, bandas, umbrales o presets del PO. Se conserva el ledger de trabajo y toda la evidencia ignorada; no se elimina trabajo al detenerse. **PR para Claude/PO sobre el bloqueo, sin fusionar; esperar decisión antes de continuar o ajustar.** S1 permanece aceptado, S3–S4/T8 y fase2física sin autorizar.

## 2.2 — cierre tras reanudación autorizada

Se conserva la parada histórica. PO«Seguir sin cambiar SL» convierteS2-TIME-01 en observación predeclarada2.7, sin tocar recetas0c638da. RED92claves:186tests/2fallos por aserciones dehash/cantidad antiguas, sinCS; GREEN186/186 al actualizar expectativas a los hashes calculados porUnity. Defaults**9af98129d12e**, P0extendido**7bbd2ccb77e8**,92claves;41heredados0d39e6016bd8/SimulationKey166a96e319c7/2-35mm/N2estricto intactos. Suites frescas**473/473EditMode y208/208PlayMode**,sinCS;transitoriosGraphics/Qualityrestaurados. [Evidencia](../validation/evidence/2026-10-30-s2-task22-resume.json). Ninguna nueva medición física ni ajuste deSL. Se continúa2.3–2.6.

## 2.3 — contrato de contacto

Esqueleto compilable/RED30tests-30fallos por aserción→GREEN30/30. Casos originales incluidos: A/V exactos, T3 puro, antes deK aunque dentro deA, excepción suave, segundo bote, plan visible enroriginal/cambio entre compensado y original, geometría autoritativa, cuantización/empate anterior/vecino legal/error firmado, asistencia futura y antigüedad del reloj. No se usa el instante de presentación paraΔ. NaN representa ausencia deEnded solo en la API pura; el controlador no fabrica una suelta. Cinco claves de timing según tabla aprobada; defaults**f0e57daf7ba3**,P0extendido**725be2d95a35**,97claves. Expectativas dehash:RED503/3fallos→suite fresca**503/503EditMode**,sinCS. [Evidencia](../validation/evidence/2026-10-30-s2-task23.json). Componente Core aún sin integración de juego; regresiónPlayMode dehost/controlador pendiente2.5–2.6.

## 2.4 — solver de velocidad total

REDcompilable12tests/11fallos por aserción (la comprobación heredadaSolveCenterTo/L1-L6 ya pasaba)→GREEN12/12. Se racionaliza la raíz baja en double y se redondea hacia arriba solo la representación float de la velocidad mínima física; no se cambia un umbral. Se verifica el primer contacto mediante P2 aislado: frontis antes de suelo/lateral, error≤5cm. Infactible: velocidad mínima hasta cap, después búsqueda acotada de16iteraciones para bajarY manteniendoX; sin solución devuelve motivo. Defaults100claves/**a768705c406b**,P0extendido**79f6091f375e** calculadosUnity;41heredados/SimulationKey/2-35mm intactos. REDdeexpectativas515/4→suite fresca**515/515EditMode**,sinCS. [Evidencia](../validation/evidence/2026-10-30-s2-task24.json). Sin contacto de juego integrado todavía ni medición nativa.

## 2.5 — host, historial y contacto retroactivo

REDcompilable15tests/14fallos por aserción→16/15 al añadir presente fraccional; GREEN16/16. Casos adicionales: contacto futuro dentro de un frame de varios pasos mostró28esperado/31real; RED19/1→GREEN19/19 corrigiendo el avance doble. Duplicados/reentrada reservan una vez. RED21/1 por mapa con cámara lenta anterior al nuevo lanzamiento; la captura toma ahora debug.slowMotion del snapshot vigente. La comparación del salto programado usa la trayectoria anterior en el mismo presente del frame, no la imagen del frame previo.

Anillo preasignado dimensionado por250ms reales/Hz/tempo; guarda BallState completo, botes de recepción y versión. Host sigue siendo la autoridad única/P2intacto. Launch/Replay incrementanLaunchId; orden vieja/historia caducada/segundo intento se rechazan con motivo. ApplyShot restaura el paso, pone velocidad y ejecuta pasos P2completos al presente del mapa, sin Advance otra vez ni callbacks intermedios. Predicción/publicación propias; noLaunched ficticio. Reset de botes de salida separado de recepción. El acumulador existente SimClock incorpora RestoreFraction para el presente fraccional tras vuelo detenido, sin segundo reloj ni cambio de descartes; prueba comprueba continuidad del siguiente frame.

Eventos reconciliados por identidad lógica dentro del lanzamiento (paso/superficie/ordinal); prefijo no reemitido. ObsoleteEffects cuenta contactos públicos no persistentes emitidos que pierden identidad canónica: no afirma borrar audio/vibración. Estela se vacía y marcas se reconstruyen desde contactos canónicos recientes sin callbacks. Salto enmetros/píxeles/diámetros visuales al mismo instante/cámara, sin suavizado; píxeles desconocidos sin cámara. Edad acepta timestamporiginal opcional en Apply/Schedule para que2.6 no la confunda con edad del paso; sin él se informa edad del contacto cuantizado. Se separan costos de reconstrucción/predicción/reconciliación; son costos editor, no aceptaciónH17 nativa.

Clave rollback.historyMs de la tabla2.5:250/rango100–500, RED7/1 antes de añadirla. Defaults**7c057e946adb**,P0extendido**d45c57c24b08**,101claves. REDdehashes/cantidades516/5; suiteEditMode fresca**516/516**. Una sustitución demasiado amplia alteró temporalmente la expectativa del rango en el test a101; se restauró100 antes del resultado definitivo, sin cambiar catálogo/criterio. Primera suitePlayMode228/229 detectó aislamiento de la cámara del test de salto (dosMainCamera después de otras suites); se conservó la aserción y se usa la misma cámara activa. Repetición completa fresca**229/229PlayMode**,sinCS;Graphics/Qualityrestaurados. [Evidencia](../validation/evidence/2026-10-30-s2-task25.json). Sin instalación ni medición física.
