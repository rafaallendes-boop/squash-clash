# First Shot S2 fase1 — M2-4, tareas2.0–2.6

Base8dd142dff2fcdc5d52871ac3c89733b4aff88fd6, rama firstshot/s2-m24. AutorizaciónPO2026-10-30 en [registro](2026-10-25-first-shot-po-approvals.md); [plan/D4](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Trabajo en curso; no se declara completada ninguna tarea sin evidencia.

## Estado vigente — preparación de tanda 1 autorizada, en curso (2026-10-31)

PO autoriza en la misma rama/PR #30 exclusivamente preparar la tanda 1, sin pedir gestos ni iniciar bloques. Se corrigió el origen del mscorlib en el [anexo de hashes](../validation/2026-10-31-first-shot-s2-session27-hashes.md): copia archivada ignorada con SHA-256 26bb31ee63f0b5b8daa34728d475b2d7029cb19d41cd8f00228568c4a0148eef, verificada antes de regenerar exports. Los 13 literales no cambian.

Cotejo inicial Git/SHA-256: **506 archivos de Assets/ProjectSettings/Packages byte a byte idénticos a la fuente probada 9131b1c** y al manifiesto de regresión. Check-release dev/prod limpio y build dev Release ARM64/no Development autorizados, todavía por completar en este registro inicial. Exportación de logs/respaldo nuevo de los tres archivos en proceso; no hay instalación ni arranque S2 declarados. El único perfil en caché sigue creado el27oct/con vencimiento2026-11-03T18:29:44Z: el PO debe renovar antes de instalar; se solicita confirmación y horario previsto de tanda1/restauración para comprobar la vigencia efectiva del incorporado. No basta que la caché aún no venza.

Al finalizar se registrarán identidad/build, perfil/cotejos/respaldo y primer hash observado en app B=H(1,V,0)=dd122831f592; diferencia→registrar/detener, sin recalcular. Los otros H se cotejan desde la propia app antes de cada bloque futuro, sin scripts/tests temporales. Sesión FAM/02–13 y tandas2–4/PROD no autorizadas. Producto y restricciones intactos; PR#30 sinfusionar y paraClaude, detenerse al concluir preparación.

## Histórico — ajustes documentales A1–A4 de 2.7 entregados en f70cccc (2026-10-31)

Claude aprobó `31d7433` con ajustes documentales autorizados por PO, sin bloqueos de producto. Se incorporan en la misma PR #30: frontera dev en el evento auditado de cierre DEV (foco de app no equivale a panel cerrado), relojes Mac/dispositivo relacionados sin asumir sincronía, lanzamientos interbloque preparación; triada por intento solo FAM/CAN/NEG-A–D y excepciones al cierre en los demás; **13 hashes esperados ya calculados**, deltas/SHA-256 de entradas/manifiestos y cuerpos float32→double en el [anexo A3](../validation/2026-10-31-first-shot-s2-session27-hashes.md); cuatro tandas con exportación/restauración nueva/cotejo de tres archivos en cada cierre, precheck completo y vigencia del perfil renovado en todos los tramos. H(1,V,0) coincide con `dd122831f592`; cálculo fuera de Unity y sin la app, usando el respaldo P0 local de SHA-256 aprobado, no un JSON reconstruido. Los literales requieren revisión de Claude antes de medir y cotejo con Unity en preparación futura autorizada; cualquier diferencia obliga a registrar y detenerse. Auditoría documental: 13 manifiestos completos reconstruidos desde el propio Markdown/SHA-256 y deltas, B verificado, 25 bloques/206 recepciones y 16 preguntas originales conservados; no es una ejecución de tests de producto.

Se explicita `debug.slowMotion=1` como **1 = sin cámara lenta** y se informa Aim(Began)→C(3,2/2,2) descriptivamente como referencia verbal sin marcador. El [protocolo actualizado](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md) conserva los 25 bloques/206 recepciones previstas y las preguntas originales. PROD carece de DEV: su marcador app específico tras inicialización de práctica queda documentado para revisión, sin crear evento/control. No se ejecutan nuevas suites, check-release, builds/exports, instalación, acceso al iPhone ni sesión; producto/defaults/presets/SL/spec/plan/criterios/bandas/umbrales intactos. **PR #30 lista para Claude, sin fusionar hasta juicio del PO de 2.7; detenerse.**

## Histórico — fase 1 aprobada; predeclaración 2.7 entregada en31d7433 (2026-10-31)

PO comunica aprobación de Claude de **c2e8ee7**, producto **9131b1c**; 1bbaa39 y c2e8ee7 solo documentación. Fase 1 (2.0–2.6) aprobada sin bloqueos. Se autoriza exclusivamente predeclarar por escrito 2.7 en la misma PR #30: [protocolo propuesto](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md), con orden/tareas/SL, hash externo manual, instrucciones, preguntas literales, métricas, parada y restauración. Son 25 bloques/206 recepciones previstas, no ejecutadas ni contactos logrados. Ningún número de ese protocolo es un nuevo umbral de aceptación.

Observaciones de Claude sin cambios de código: (1) el overload de hash solo se usa en tests; DEV no proporciona un esperado y el campo del registro guarda el observado al armar; el protocolo no lo usa como validación automática; (2) el mapa puede sumar segmentos durante un vuelo activo y se reinicia en el lanzamiento siguiente; no se introduce poda/tope; (3) perfil conocido vence 2026-11-03T18:29:44Z: el PO deberá renovarlo y se comprobará el incorporado antes de cualquier instalación futura.

Esta actualización modifica solo Markdown; no hay nuevas suites, exports, Xcode build, instalación, acceso al iPhone ni sesión. Defaults/SL/spec/plan/criterios/bandas/umbrales intactos. Los resultados que siguen son evidencia anterior de fase 1, sin nuevas ejecuciones ni aceptación de S2 inferida. **PR #30 abierta/lista para Claude, sin fusionar hasta que el PO juzgue 2.7.** Se detiene a esta entrega documental.

## Histórico — alcance 2.0–2.6 verificado y entregado el 2026-10-30

PO autorizó el 2026-10-30 la corrección acotada del reloj en la misma PR #30 y completar 2.6. Claude verificó el mecanismo en head `8777f9bcd48cfe0924bf4851ae7606abe312095f`; la parada anterior fue correcta y se conserva como historia. S2-TIME-01 sigue como observación predeclarada para 2.7; SL1–SL6 congeladas, sin tuning.

- Corrección del reloj `bd84896`: protecciones verdes antes de implementar; RED original 2/2 reproducido → GREEN 2/2. Lista estable en 120 frames: detenido **5→5**, pausado **7→7**, pelota idéntica bit a bit. Una observación final pasiva de tamaño fijo permanece consultable. No se midieron bytes ni coste nativo.
- Fuente de producto final `9131b1c`: 65/65 focales de controlador, **522/522 EditMode y 297/297 PlayMode completos y frescos**, cero omitidos, sin errores CS. Cobertura D4.6 y M2S2-N01–N12 ligada a casos descubiertos/Passed en el [registro final 2.6](../validation/evidence/2026-10-30-s2-task26-final.json).
- Ocho fixtures First Ball/S1 fijan práctica=0 solo en preparación; aserciones y escenarios anteriores intactos. GraphicsSettings/QualitySettings restaurados; **514 archivos de fuentes** cotejados y **40 archivos protegidos** byte a byte idénticos.
- Defaults **106 claves / 915ae1e479b9**, P0 extendido **dd122831f592** calculados por Unity; 41 heredados/proyección **0d39e6016bd8**, SimulationKey **166a96e319c7**, 2/35 mm y N2 estricto intactos. Defaults byte a byte iguales a 8777f9b; de los 85 previos solo cambia `gesture.control`, más 21 claves ya aprobadas.
- **Check-release dev/prod OK**, exports frescos desde **1bbaa39 limpio / gitDirty=false en ambos**. Framework de pruebas excluido en ambos Players; DevPanel solo dev. Los 514 archivos de producto vuelven a coincidir tras los exports; árbol limpio y transitorios retirados/restaurados. Último ajuste posterior solo documental.
- PR #30 permanece abierta/lista para revisión, **sin fusionar**. No aceptación de S2 ni autorización de 2.7, Xcode build, instalación, acceso al iPhone, prod físico, S3–S4 o T8.

## Histórico — parada en S2-TIME-01

**Fase1 incompleta.** 2.0 y2.1 implementadas/probadas; en2.2 los14focales pasan pero SL5/SL6 dejan margen incompatible con V100 para un gesto de la duración mediana de mediosS1. Se aplica «registrar y detenerse» delPO antes de2.3, sin tuning ni sesión. El detalle y los límites de ese análisis están al final. No se declara regresión/cierre técnico completo después de añadir las siete claves2.2.

## Histórico — reanudación por el PO

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

## Histórico — parada S2-TIME-01 antes de 2.3

**Estado histórico de la primera parada, sustituido por la reanudación posterior del PO.** No se inicia 2.3–2.6, no se ajustan las recetas/velocidades/ventanas ni se permite apoyar antes de Recepción para rescatar el resultado. No hay build Xcode, instalación, acceso al iPhone, sesión 2.7 o recorrido funcional prod.

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

## Histórico — 2.6 parcial antes de la parada

Fuente de la integración parcial: `f1ae28f50b1f4f8e75361f03a742f4eba3fd29a5`; el commit posterior conserva el RED de retención y los registros de la parada, sin corregirlo.

Se conectó una sola entrega inmutable desde GesturePreview al controlador, manteniendo el host/P2 como autoridad. Objetivo delBegan/Pfinal/Ended original y contexto capturados, identidad de intento/dedo/lanzamiento, cierresD4.2 sin consumo frente a reserva atómica delSwing. Registro de O2/proyección observada/cadenas de reapoyo; una omisión previa conserva su versión cuando llega una orden histórica del mismo lanzamiento. V/T3puro y T3/T3-suave registrados por recepción. Cuerpo/timing/solver capturados al lanzar, claves de gesto al apoyar. Configuración próxima separada en el panel. Práctica y feedback residen en Runtime; DevPanel solo presenta ajustes/diagnóstico.

Ciclo de práctica y repetición única, marca factible separada de intención, anillo y notas, contacto/swish provisionales con [procedencia original](../../Assets/SquashClash/Runtime/Resources/Audio/PROVENANCE.md). No háptica/causalidadS3, rival, rally ni contacto humano nativo. Los nuevos recursos de audio y controles todavía no pasaron los exports dev/prod; el check estático focal no sustituye ese gate ni el recorrido físico deprod.

TDD observado: registro/snapshotsRED2/2→GREEN2/2; controladorRED38/35fallos, ampliado44/41→primerGREEN42/44. El test de tempo editaba un valor pendiente, sin cambiar el reloj efectivo; se corrigió su preparación para usar Configure delSimClock existente y Tick depreview, conservando la aserción de cancelación. En Ended idéntico, InputSystem redondeó dos ulps el tiempo al pasar su base interna: la prueba compara ahora **exactamente con TouchSample.TimeReal realmente entregado por el adaptador**, sin tolerancia para r ni cambiar el runtime para igualar el argumento deQueueStateEvent. ElGREEN posterior44/44 se observa dentro delRED45/1 al añadir sección de panel. RED46/2 añadió identidad de preset: demostró lectura incorrecta al procesar tras un cambio posterior aEnded; se capturan ahora preset/archivo/build/seed/pendientes/hash de lanzamiento alBegan. GREENfocal**46/46**. Valores/umbrales no cambiaron por estos fixes. Expectativas dehash106:RED518/5→suiteEditMode fresca**518/518**. [Resultados/huellas deXML](../validation/evidence/2026-10-30-s2-task26-partial.json).

**La suitePlayMode completa de2.6 no se ejecutó antes de la nueva condición de parada.** No se afirma compatibilidad global FirstBall: sigue pendiente comprobar/adaptar únicamente la preparación de fixtures que asuman arranque deFirstBall/S1preview, con practice.enabled=0 si procede, sin tocar aserciones ni escenarios; esa excepción está autorizada por el plan, pero todavía no aplicada en esta integración. No se ejecuta revisión final completa de la skill ni check-release tras parar. Todas las pruebas citadas son nuevas ejecuciones compiladas/con casos descubiertos; no XML viejo ni cero tests.

## Histórico — S2-CLOCK-01: retención sin límite por frame en pausa o vuelo detenido

**Severidad Media; bloquea el cierre por la orden de parada del PO.** Regresión introducida en la integración del mapa del reloj: en modoFirstBall (`practice.enabled=0`), sin nuevo lanzamiento, elhost sigue recibiendo frames después del fin del vuelo o mientras está en pausa. [AdvanceCore](../../Assets/SquashClash/Runtime/Ball/BallSimulationHost.cs) observa el reloj antes de retornar por!IsRunning; [ObserveClock](../../Assets/SquashClash/Runtime/Ball/BallSimulationHost.Shot.cs) llamaMark en ambos estados; [ClockMap.Mark](../../Assets/SquashClash/Core/Shot/ClockMap.cs) añade a unaList sin poda/coalescencia cada timestamp distinto. El anillo deBallState sí está preasignado/acotado: el defecto es la lista de segmentos del mapa, no el historial físico.

| Reproducción sintética | Segmentos antes | Frames nuevos | Segmentos después | Estado de pelota |
|---|---:|---:|---:|---|
| Vuelo detenido | 85 | 120 | 205 | Idéntico bit a bit |
| Vuelo pausado | 32 | 120 | 152 | Idéntico bit a bit |

[Reproducción y huella delXML](../validation/evidence/2026-10-30-s2-clock-01.json); testClockMap_StableStoppedOrPausedFlight_DoesNotRetainOneSegmentPerFrame en [ShotRollbackTests](../../Assets/SquashClash/Tests/PlayMode/ShotRollbackTests.cs). Dos fallos por aserción con compilación correcta. El test inspecciona la lista privada como diagnóstico, no añade un parámetro runtime ni un umbral de aceptación de gameplay. El crecimiento continúa aunque el estado y el contexto se mantengan; no se midieron bytes/asignaciones ni percentiles de coste y **no se declara un nuevoFailH17**. La pausa/repetición suspendida puede prolongar un lanzamiento indefinidamente: la retención no queda limitada por250ms de rollback ni por el horizonte de vuelo.

**Propuesta para decisión del PO, no implementada:** coalescer observaciones estables en pausa/vuelo detenido, conservando los cambios exactos de pausa/tempo/cámara lenta/descartes y la conversión de entregas tardías. Antes de modificar, predeclarar pruebas de retención, r original/expiración, paso manual y reanudación/rollback al presente, sin un segundo reloj ni umbrales nuevos. No se elige un límite nuevo ni se aplica ese ajuste ahora. Se conserva elRED y se detiene; el historial deS2-TIME-01 y su resolución no se borran.


## Corrección autorizada S2-CLOCK-01

Antes de modificar runtime: ClockMap 13/13 y protecciones PlayMode 16/16 verdes. Incluyen conversión de Ended tardío por r original, historial caducado, paso manual pausado, F02 y presente fraccional, además de una pausa sintética de un minuto: expira el historial antiguo, se reanuda y un contacto con historial nuevo permite rollback/predicción/avance del frame siguiente. RED de retención repetido: 2 tests/2 fallos por aserción, compilación correcta. Casos adicionales de coalescencia exacta: RED 16 tests/2 fallos (14 protecciones ya verdes).

`ClockMap.Mark` agrupa solo una continuación canónica exacta con contexto idéntico y sin descartes. Conserva la última observación suministrada en una estructura fija; no la acumula por frame. Las consultas la tratan como extremo final y, antes de una discontinuidad, se conserva el extremo del prefijo conocido. Así no se extiende hacia atrás un hueco descartado ni un intervalo inexacto. Pausa/reanudación, paso manual, tempo, cámara lenta, descartes y validación monótona conservan sus fronteras. El host sigue observando con vuelo detenido: F02 no se congela. No cambia `ObserveClock`, SimClock, el anillo ni su política de expiración.

Se comparte la normalización double a entero que ya existía, sin cambiar su cota ni introducir una tolerancia. Dos candidatos intermedios fallaron las aserciones existentes: primero retención detenida (24/25), después F02 (5 pasos resimulados frente a 6, 24/25). Se conservaron aserciones y datos. La versión con extremo consultable pasa **16/16 ClockMap y 25/25 rollback/Ended tardío**. No hay segundo reloj, parámetros/defaults nuevos de reloj, límite temporal ni poda por edad. La lista retiene cambios y observaciones inexactas; no se afirma un tamaño máximo ante cambios arbitrarios de contexto. [Evidencia completa y XML hashes](../validation/evidence/2026-10-30-s2-clock-01.json).

Estimación de Claude comunicada por PO: unos 40 bytes/segmento y 2,4 KB/s a 60 fps en el defecto anterior, además de recorrido lineal creciente. **No medida**: esta corrección demuestra retención estable en las pruebas sintéticas; no acredita reducción de bytes/costes, H17 ni rendimiento nativo.

## 2.6 — cobertura final y compatibilidad

Los casos añadidos protegen comportamiento ya implementado: Tap/temblor de 150 ms y dedo sostenido sin Ended; carga retenida P≈0,7; C-A/reapoyo/retorno y nueva salida; configuración/viewport y nuevo Began; Began tardío y Ended temprano/tardío/fuera de V/sin plan; historial caducado como NotEvaluable; plan visible en r original frente al posterior; varios cierres append-only; alternativas explícitas y nuevo contacto tras Swing; mismo M2-4 puntual/tardío con estado físico bit a bit igual. Los riesgos N04/N04b siguen pudiendo producir Swing/P positivo; ninguna intención humana se infiere de los datos sintéticos.

Para completar la guarda de hash predeclarada de D4.6 se añadió un overload `ArmSlingBlock(block, expectedGestureHash)`: esqueleto compilable → RED 1/1 por aserción → GREEN. Rechaza `wrong-hash` antes de iniciar bloque/temporizador o tocar la pelota, muestra remedio y registra la causa; el hash coincidente arma. La llamada S1 existente conserva su comportamiento. **No es una clave de configuración ni un nuevo default**; no se ha predeclarado ni armado una campaña 2.7. Fuente `9131b1c`, focal final **65/65**. El [JSON final](../validation/evidence/2026-10-30-s2-task26-final.json) cruza cada negativo N01–N12 y D4.6 con casos Passed de las suites finales; no suma esos cruces como pruebas independientes ni como resultados humanos.

Fixtures ajustados únicamente en preparación (`practice.enabled=0` y relanzamiento donde corresponde): DevPanelIntegrationTests, GesturePreviewTests, LegibilitySelectorTests, M24PreviewTests, PerformanceSessionTests, ReticleVisibilityTests, SlingControlSafetyTests y SlingPreviewTests. Se conservan las aserciones previas, incluidos L1–L6; no cambia ninguna receta. Primera suite con fixtures explícitos **278/278**, luego cobertura final y regresión **522/522 EditMode / 297/297 PlayMode**, sin CS/cero omitidos. XML frescos y logs preservados en `Logs/evidence/s2-tdd`; no se usa el verde de 2.5 para acreditar 2.6.

ProjectSettings transitorios restaurados antes del cotejo: 514 archivos de Assets/ProjectSettings/Packages/scripts iguales al manifiesto de la fuente probada; 40 archivos protegidos del registro anterior coinciden en SHA-256. Defaults, 21 claves nuevas y los 85 valores anteriores conservan el estado revisado por Claude; Las seis filas de recetas de SL1–SL6 permanecen byte a byte iguales a su predeclaración documental en 0c638da; el archivo C# conserva el estado revisado en 8777f9b. Spec, plan/propuesta, criterios/umbrales, vuelos/velocidades/tempo, V/A, compensación, N2/dirección y `pullFullMm` intactos.


## Gate final de entrega para revisión (2026-10-30, histórico)

`scripts/check-release.sh` terminó **OK**: exports dev y prod nuevos del head documental **1bbaa39**, ambos con `gitDirty=false`, Release/no Development según BuildScript intacto. Core/Runtime incluidos en ambos, DevPanel únicamente en dev, framework de pruebas de InputSystem excluido en ambos. No se invocó Xcode build ni se instaló. El [registro final](../validation/evidence/2026-10-30-s2-task26-final.json) conserva metadatos, SHA-256 de logs/evidencias y el manifiesto de 514 archivos cotejado después de exportar; no se sustituyen resultados de tests por un export.

Auditoría de preparación: **386 aserciones anteriores** de los ocho fixtures coinciden literalmente con 8777f9b. Las seis filas de recetas congeladas coinciden con 0c638da (origen documental; el C# se añadió después), y ese código no cambia desde el head revisado. Se verificaron 119 enlaces locales de los tres Markdown antes del ajuste final; los enlaces nuevos usan las mismas rutas existentes. Los 40 archivos protegidos conservan sus hashes. Ningún transitorio de tests/export se versiona.

**Entrega técnica de 2.0–2.6 para Claude/PO; PR #30 abierta/lista y sin fusionar.** No equivale a aceptación de S2 ni autorización de fase física. S2-TIME-01 y las observaciones de saturación/N04-N04b/ritmo/preferencia de tirón se conservan para la sesión 2.7 futura; no se ajustó ningún valor para hacer pasar pruebas. Sin Xcode build, instalación, acceso al iPhone, sesión 2.7, métricas nativas nuevas, S3–S4 o T8. Detenerse a la entrega.
