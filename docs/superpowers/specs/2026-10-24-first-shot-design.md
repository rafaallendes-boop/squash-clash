# First Shot — propuesta de diseño

**Estado:** **dirección general aprobada por el PO el 2026-10-24**, con las decisiones de la §9 resueltas y dos condiciones (§9.1). Pendiente de la revisión técnica crítica de Codex; después Claude incorpora los hallazgos y escribe el plan S1–S4. **No autoriza implementación:** el PO revisa el plan final antes de autorizar S1.
**Fuentes:** spec v0.3 (`2026-10-20-squash-clash-design-v0.3.md`, incluidas D30–D35), decisiones del PO del 2026-10-24 sobre First Shot y la base de código cerrada en First Ball (main `a8da9c4`).
**Regla de lectura:** donde esta propuesta difiere de la spec, se indica en la §8 y **no rige hasta que el PO lo apruebe**. Nada de lo anterior se modifica en silencio.

---

## 1. Intención

First Shot responde a la pregunta de la spec: **¿el golpe se siente bien y su resultado se entiende?**

En palabras del PO: que el primer golpe sea **intuitivo y satisfactorio**, y que **la pelota vaya donde el jugador pretende**. No basta con que funcione: tiene que entretener. El criterio real es del PO: si el golpe no engancha o el PO no lo «proyecta» bueno, First Shot falla aunque todas las métricas den Pass.

**Prioridad absoluta del PO:** un golpe jugable en el iPhone lo antes posible, para comprobar si se siente bien, sin sacrificar calidad.

Referencias de sensación del PO (no de estética): *Wii Sports Tenis* y *Tennis Clash*. Lo que se toma de ellas en First Shot es el gesto con el dedo, el premio claro al momento justo y la diferencia nítida entre un golpe bueno y uno normal. La anticipación (moverse, adivinar una volea) es de First Rally.

## 2. Objetivos, no objetivos y anti-objetivos

**Objetivos**
- Apuntar al frontis con una mano y que la pelota llegue donde se apuntó.
- Distinguir y controlar a voluntad golpes **suaves** (drop) y **fuertes** (drive), sin sentir que el juego decide por el jugador.
- Timing **exigente pero aprendible**, con un único intento por pelota.
- Que un golpe **perfecto** se sienta claramente mejor que uno normal.
- Que tras 10–15 intentos el PO sienta que está aprendiendo a dominar el control.

**No objetivos (First Shot)**
- Rival, IA, movimiento del jugador, planificador completo, posición de espera W, volea, reglas de punto, saque, marcador, cansancio y capa aleatoria (spec §14).
- Controles B y C: quedan como opciones futuras, **no se implementan** (decisión del PO).
- Efectos de error **direccionales** E1, E2, E4 y E6 hasta que el PO valide sus filas (§7.6.3).

**Anti-objetivos**
- Golpes fuertes accidentales o una mira que se mueve sola al soltar.
- Que la latencia de procesamiento de frames decida injustamente el resultado del timing.
- Sacudidas de cámara, cámara lenta o efectos visuales que impidan seguir la pelota.
- Sobrediseño: frameworks genéricos de input, de habilidades o de audio.

## 3. Punto de partida (First Ball, ya existente)

- Simulación **P2-Analytic** a 120 Hz, determinista, con predicción **bit a bit** igual a la simulación (§8.2.1) y `Ballistics.SolveCenterTo` para resolver trayectorias a un punto del frontis.
- `BallSimulationHost` como única autoridad de la pelota, con eventos de contacto y reloj de simulación (`SimClock`).
- Lanzador con presets L1–L6 (legibilidad), posiciones `human.x/z` y `rival.x/z` y repetición `launcher.repeatSeconds`.
- Cámara fija y de seguimiento, ayudas visuales (sombra, estela, marcas, predicción), panel de desarrollo con presets JSON, métricas JSONL con retención (R1) y perfiles dev/prod.
- Input System 1.20 instalado. **No existe** todavía input táctil de juego, cuerpo del jugador más allá del marcador de posición, audio ni háptica.

## 4. Diseño propuesto

### 4.1 Modo práctica de First Shot

- El jugador **no se mueve**: su cuerpo está en una posición fija por lanzamiento (`human.x/z` del preset de lanzamiento). El jugador solo decide **dónde apunta, con qué fuerza y cuándo suelta**.
- Lanzamientos de práctica nuevos, separados de L1–L6: los **seis estandarizados de §6.3** (media altura de drive, media altura de revés, esquina trasera de drive, esquina trasera de revés, adelante de drive y adelante de revés), cada uno con una posición del cuerpo desde la que la pelota es alcanzable. L1–L6 se conservan intactos para la legibilidad.
- Personaje **diestro** por defecto; el drive y el revés tienen la misma calidad (spec §6.5.9).
- Repetición automática del lanzamiento como hoy (`launcher.repeatSeconds`), para encadenar intentos.

### 4.2 Gesto A′ (control táctil)

Hipótesis inicial aprobada por el PO: **arrastrar para apuntar y potencia por velocidad del dedo.**

**Mira**
- Vive en **coordenadas del frontis en metros** (§6.4). El desplazamiento del dedo en **mm físicos** se convierte con una ganancia fija configurable. Mover la cámara no mueve la mira.
- Posición inicial al apoyar el dedo: punto neutro a ~2,2 m de altura, alineado con un drive paralelo del lado de la pelota (§6.4).
- Siempre visible mientras se apunta. Roja si apunta al tin o por encima del out (§6.4), usando la huella de líneas existente.
- **El jugador siempre entiende dónde apunta** (pedido del PO): cuando la mira se congela, cambia de forma visible a un estado «fijada», y tras soltar permanece en el punto fijado hasta el contacto o el fallo. Así lo que se ve es exactamente lo que se usará.

**Congelamiento de la mira**
- Cuando la velocidad del dedo supera `v_flick`, la mira toma la posición que tenía ~50 ms antes y el resto del movimiento ya no la mueve (§6.2, A′). Los dos valores son configurables.

**Potencia (pedido del PO: no usar la última muestra)**
- La potencia `p ∈ [0; 1]` sale de una **velocidad filtrada en una ventana corta antes de soltar** (`power.windowMs`, rango de partida 60–100 ms, valor inicial 80 ms), calculada como desplazamiento en la ventana dividido por su duración, con las muestras reales del táctil y sus marcas de tiempo, no con una sola muestra.
- Conversión mediante una curva configurable con **zona muerta inferior** (soltar casi quieto = golpe suave intencionado) y **tope superior** (`power.vMinMmPerS`, `power.vMaxMmPerS`, exponente de la curva).
- La magnitud del flick, sin importar su dirección, define la potencia [decisión de Claude]. La dirección no influye en la mira gracias al congelamiento.
- Objetivo de control: poder ejecutar a voluntad un drop suave y un drive fuerte. Se valida con la métrica de potencia involuntaria de §6.3.

**Máquina de estados** (spec §6.5, reglas 1–8): un solo dedo; toque frente a arrastre (< 3 mm en < 250 ms); un intento por pelota; cancelación volviendo a un círculo de ~6 mm del origen; pérdida de foco cancela sin consumir el intento; bordes del sistema diferidos. Las reglas pendientes N1 y N2 se piden al PO en la §9.

### 4.3 Timing y contrato de contacto

**Instante del golpe (pedido del PO).** `r` es la **marca de tiempo del evento táctil** del Input System, convertida al reloj de simulación (§7.1.1), no el frame en que se procesa. Como el evento se procesa uno o dos frames después, el contacto se resuelve **retroactivamente**: el host conserva un historial corto de estados de simulación (≥ 100 ms) y, al recibir la liberación, vuelve al paso que contiene `r`, aplica el golpe y resimula hasta el presente. Es posible sin romper la autoridad única porque la simulación es determinista y la predicción coincide bit a bit. Además se mantiene una **compensación de latencia** configurable en ms (§7.1.5), que se calibra antes de medir H3.

**Plan del contacto sin movimiento.** Un **planificador estático** (subconjunto de §9.1, sin cinemática) toma la trayectoria predicha tras el bote y, desde la posición fija del cuerpo, calcula:
- `K`: tramo en que la pelota es legal y está dentro del alcance `R` (~1,3 m, configurable).
- `P*` y `T*`: el punto e instante ideales dentro de K, con preferencia por la altura 0,4–1,1 m y la separación de paredes ≥ 0,3 m (puntuación U de §9.1 sin términos de holgura ni de estocada).
- El **anillo de timing** se cierra en `T*` (ayuda completa por defecto).

**Tres resultados (decisión del PO), sobre el contrato T3:**

| Resultado del PO | Condición | Qué ocurre | Nota visible |
|---|---|---|---|
| **Perfecto** | \|Δ\| ≤ A (40 ms) y pelota en K | Contacto asistido en T\*/P\* (o en r si r > T\*), sin vector causal de timing. Máxima calidad. | Perfecto |
| **Válido** | A < \|Δ\| ≤ V y pelota en K en r | Contacto **en r**, con la pelota donde esté: adelantado o atrasado. Penalización **progresiva** que crece con la distancia geométrica al punto ideal (§7.6.2). | Adelantado / Atrasado |
| **Fallido** | \|Δ\| > V, o pelota fuera de K en r | Swing sin contacto, intento consumido. | ¡Al aire! / ¡Tarde! |

- `A` = 40 ms (spec, se mantiene). `V` = **ventana válida experimental**, configurable entre 80 y 120 ms, valor inicial 100 ms, con la opción «sin límite» (solo K, que es T3 puro de la spec) para comparar. **La ventana V es un cambio respecto de la spec** (ver §8, C1).
- La penalización del resultado Válido se aplica **solo a través de la geometría del contacto** (invariante §17.A.12) y solo con efectos **no direccionales**: la pelota sale más alta, más lenta y «más suelta». No hay desvío lateral hasta que el PO valide E1/E2 (ver §8, C2).
- Un único intento por pelota (D19). T3-suave queda disponible en el panel como control (§7.1.4).
- Casos de §7.1.3 sin rival ni movimiento: aplican 1–11 y 13; el 12 (error de ejecución) y el 14 (volea) no existen en First Shot.

### 4.4 Resolución del golpe (shot solver)

- Velocidad de salida `v = v_min + p·(v_max − v_min)` en m/s (§7.4.1), con energía fija.
- Política «se respeta el objetivo»: entre las dos soluciones balísticas al punto apuntado, el **arco más bajo** (§7.4.2), reutilizando `Ballistics`.
- Objetivo infactible con esa `v`: se sube la velocidad mínima necesaria hasta un tope; si no alcanza, se baja el objetivo al punto factible más cercano (§7.4.3). Se registran intención, ajuste y salida (§7.4.4).
- Orden: primero el error causal sobre P y v, después la resolución; nunca se corrige la salida después (§7.4.6).
- El golpe resultante se entrega al host como nuevo estado de la pelota: **una sola autoridad**, y la predicción posterior al contacto se recalcula desde ese estado.
- **Drop:** se logra con poca potencia y un objetivo bajo, con superficies consistentes. El estado «cortada» de §7.5 solo se añade si el PO juzga que el drop no «muere».
- La intención (paralelo, cruzado o drop) se **etiqueta después** a partir de la mira y la potencia, solo para métricas (ver §8, C3).
- Ajuste del solver visible **justo después de soltar** (C4): la mira se desplaza al punto factible y, si el objetivo pedido no era factible, una **indicación clara** lo señala (por ejemplo, la marca original atenuada unida al punto real).

### 4.5 Calidad del impacto (capa causal no direccional)

Desde First Shot, solo los efectos **no direccionales** (§7.6.3): la versión simétrica del timing («más suelta»), **E3** (estirado: contacto al límite del alcance) y **E5** (atorado: pelota demasiado cerca del cuerpo). E7 es la ausencia de vector. Magnitudes y topes configurables en el panel; cada vector se registra por separado con su causa. Capa aleatoria apagada (§7.6.4).

### 4.6 Retroalimentación (aprobada por el PO)

| Resultado | Sonido | Vibración | Visual |
|---|---|---|---|
| **Perfecto** | Seco y contundente | Firme | Destello breve en el contacto y texto «Perfecto» |
| **Válido** | Contacto más suave | Discreta | Nota discreta (Adelantado / Atrasado) |
| **Fallido** | Aire o roce («swish») | Ninguna | Nota (¡Al aire! / ¡Tarde!) |

- Sonido y vibración se disparan en el frame del contacto (§7.1.5). Sonidos propios para el frontis, las laterales y el tin (§12.4), y alternativa visual para cada evento.
- Vibración mediante un puente nativo mínimo a la háptica de iOS (impacto de intensidad variable), siguiendo el patrón del puente térmico existente. Audio con la configuración de menor latencia de Unity.
- Sin sacudidas de cámara, sin cámara lenta y sin textos grandes. Las notas son breves, no tapan la pelota y se pueden desactivar.
- Sonidos de partida: biblioteca **CC0** con la licencia registrada en el repositorio, o sonidos generados [decisión de Claude]. El PO los juzga de oído y se pueden sustituir sin tocar código.

### 4.7 Panel, métricas y depuración (solo dev)

- Parámetros nuevos en `DefaultPreset.json` (única fuente de defaults, D32) y en el catálogo: ganancia de la mira, `v_flick`, congelamiento, ventana y curva de potencia, `A`, `V`, T3/T3-suave, compensación de latencia, nivel de ayuda (completa, parcial, sin anillo), mostrar notas, alcance `R`, `v_min/v_max` y magnitudes y topes causales.
- Superposición de depuración: P\*, T\*, K, Δ, categoría, geometría del contacto, potencia calculada y vectores causales.
- Registro por golpe en `MetricsLog`: mira pedida (en tareas) y final, potencia, Δ, categoría, duración del gesto, cancelación, ajuste del solver, primer bote y vectores (§6.3, §17.E), con preset, build y semilla (§17.A.14).
- Todo el código del panel sigue fuera de prod (§17.A.10). La mira, el anillo, las notas, el sonido y la háptica son runtime y existen también en prod.

### 4.8 Arquitectura

Mismo patrón que First Ball: **lógica en C# puro y testeable en Core**, adaptadores finos de Unity en Runtime y controles en DevPanel. Sin abstracciones fuera de la necesidad inmediata.

- **Core/Shot** (nuevo): estimador del gesto (mira, congelamiento y potencia a partir de muestras con marca de tiempo), planificador estático (K, P\*, T\*), contrato de contacto (Perfecto/Válido/Fallido y casos de §7.1.3), solver y vectores causales.
- **Runtime**: adaptador táctil (Input System, mm físicos y marcas de tiempo), orquestador del golpe con contacto retroactivo sobre `BallSimulationHost`, vistas de mira, anillo y notas, y retroalimentación (audio, háptica y destello).
- **DevPanel**: parámetros, superposición y registro.
- **Pruebas**: cada caso de §7.1.3 aplicable con prueba automática (§17.A.5); determinismo con gestos grabados reproducidos (§17.A.2); invariante de timing sin penalización duplicada (§17.A.12); bots de desfase fijo para H3b; regresión completa de First Ball intacta.

## 5. Incrementos

Orden pedido por el PO, con un ajuste para que **el primer golpe jugable llegue al iPhone ya en el segundo incremento**:

| # | Incremento | Qué entra | Qué prueba el PO en el iPhone |
|---|---|---|---|
| **S1** | **Gesto táctil y mira** | Adaptador táctil con marcas de tiempo y mm, máquina de estados de §6.5, mira en el frontis, ganancia, estado «fijada», potencia filtrada visible en la superposición, cancelación. La pelota del lanzador pasa sin golpe. | ¿Apunto donde quiero? ¿Entiendo siempre dónde apunto? ¿El número de potencia responde a mi intención, suave y fuerte? |
| **S2** | **Timing, contacto y golpe funcional** | Planificador estático, anillo completo, contrato Perfecto/Válido/Fallido con V, contacto retroactivo con la marca del táctil, solver, cuerpo fijo por lanzamiento, seis lanzamientos de §6.3, T3-suave en el panel, registro por golpe. **Un sonido de contacto provisional y las notas**, para que ya sea jugable. | **Primer golpe jugable.** ¿La pelota va donde apunto? ¿El timing es justo? |
| **S3** | **Sonido, háptica y calidad del impacto** | Tres niveles de sonido y vibración, sonidos de paredes y tin, destello, capa causal no direccional, **brazo y raqueta simples con swing procedural** (drive, revés y golpe al aire, sin rig ni assets de personaje), superposición de vectores y medición de la latencia toque→retroalimentación (§17.C). | ¿Un perfecto se siente claramente mejor que uno normal? ¿Entiendo por qué un golpe salió peor? |
| **S4** | **Calibración y validación** | Calibración de latencia, tareas en pantalla («drive paralelo fuerte»…), niveles de ayuda parcial y sin anillo, comparación T3 frente a T3-suave, bots de H3b, protocolo de sesiones y registro de validación para el PO. | Protocolo completo y decisión del PO: continuar a First Rally, ajustar (incluido añadir B o C) o volver. |

Cada incremento con rama, PR, TDD (RED→GREEN), revisión de Claude e instalación en el iPhone, como en First Ball. **El perfil vence el 2026-10-28T14:59:31Z**: renovarlo antes de instalar a partir del 27.

## 6. Criterios de evaluación (hipótesis, no umbrales)

Del PO, sin convertirlos en umbrales definitivos hasta validarlos:

| Pregunta del PO | Qué se observa o mide | Hipótesis de la spec |
|---|---|---|
| ¿La pelota va donde apunto? | Error en cm entre mira deseada y final; acierto de zona en el primer bote | H2 |
| ¿Distingo y controlo suave y fuerte? | Potencia involuntaria \|pedida − final\| en tareas de potencia | H2 |
| ¿El timing es exigente pero aprendible? | Dispersión de Δ y tasa de fallidos por sesión y por nivel de ayuda; juicio «el timing se juega» | H3, H3b, H3c |
| ¿Un perfecto se siente mejor? | Juicio del PO en sesión y valoración 1–5 | — (nuevo, juicio del PO) |
| ¿Tras 10–15 intentos siento que aprendo? | Tendencia de Δ y del error de mira dentro de la sesión; juicio del PO | H3 (extensión intra-sesión) |
| ¿Entiendo por qué falló? | El PO explica causa y categoría sin el panel | H4 |

## 7. Decisiones de Claude (revisables por el PO)

1. El cuerpo del jugador queda fijo por lanzamiento; no hay movimiento automático en First Shot.
2. La potencia usa la magnitud de la velocidad del flick, sin importar su dirección.
3. Los sonidos de partida son CC0 o generados, con la licencia registrada.
4. Ventana de potencia inicial de 80 ms y V inicial de 100 ms, ambos dentro de los rangos del PO.
5. S2 incluye un sonido provisional y las notas para que el primer golpe ya sea jugable; la diferenciación completa queda en S3.
6. El contacto retroactivo usa un historial de ≥ 100 ms de estados de simulación.
7. La marca de cantado y las reglas de línea completas se dejan para First Rally (la spec las lista en el panel de First Shot, §13.2); en First Shot solo se usa la huella para poner la mira en rojo.
8. **Contacto retroactivo sin suavizado visual.** La pelota dibujada salta al estado resimulado (unos centímetros a decenas de centímetros, según la latencia), para no separar lo dibujado de la simulación (autoridad única). Los sonidos y eventos de la pelota sin golpear entre `r` y el presente que ya no ocurren se descartan; los del golpe se disparan al resolverlo. El tamaño del salto se registra y el PO lo juzga en S2; si molesta, se revisa con datos.
9. **Las sesiones del PO se juegan en la build dev.** La selección de lanzamiento, el nivel de ayuda, T3/T3-suave y las tareas viven en el panel; prod solo contiene el juego (mira, anillo, notas, sonido y háptica).
10. **Los seis lanzamientos de §6.3 y la posición del cuerpo** los proponen Claude y Codex como valores [EXPERIMENTO] en el preset de práctica. El PO los valida jugando en S2, sin un paso de aprobación aparte.
11. **Las tareas de S4** (zonas del frontis, rangos de potencia suave/media/fuerte y zonas del primer bote) se proponen al iniciar S4 y el PO las aprueba antes de medir H2. No bloquean S1–S3.
12. **Latencia en S2:** la compensación existe desde S2 (inicial 0 ms) y se registra la latencia del sistema estimada. El juicio de S2 sobre el timing es preliminar; la evidencia de H3 solo cuenta tras la calibración de S4 (§7.1.5).
13. **Modelo de contacto del cuerpo fijo:** punto ideal a una distancia lateral configurable del centro del cuerpo, del lado del drive o del revés según el lado de la pelota, con el pie delantero hacia el frontis. Adelantado o atrasado se mide a lo largo de la dirección del golpe; estirado (E3) y atorado (E5) por la distancia al cuerpo respecto de `R`. Todo configurable, sin validación previa del PO.
14. C1 queda como opción del panel (V con ±100 ms por defecto, o T3 puro). N1 y N2 se implementan como reglas fijas, según la decisión del PO.

## 8. Contradicciones y cambios respecto de la spec (requieren aprobación explícita del PO)

| # | Spec actual | Propuesta | Por qué |
|---|---|---|---|
| **C1** | T3 (§7.1.3): si la pelota está en K, **siempre hay contacto** (adelantado o atrasado), y «no se escalan ventanas a mano». El fallo solo ocurre fuera de K. | Añadir la **ventana válida V** (80–120 ms, inicial 100 ms): con \|Δ\| > V el golpe es Fallido aunque la pelota esté en K. Opción «sin límite» = T3 de la spec. | Decisión del PO de tres resultados. Introduce una regla de tiempo además de la geométrica; conviene medirla contra T3 puro. |
| **C2** | Desde First Shot, solo efectos **no direccionales** (§7.6.3); el timing actúa solo por la geometría (D26, §17.A.12). | El resultado Válido penaliza **altura, velocidad y soltura**, no la dirección lateral. «Precisión» se interpreta como precisión de altura y profundidad. | El PO pidió «penalización progresiva de precisión y calidad». El desvío lateral exige validar las filas E1/E2. |
| **C3** | `v_min(intención)` (§7.4.1): la velocidad mínima depende de la intención pedida. | Con A′ no se elige la intención antes de soltar: un único `v_min/v_max`, y la intención se **etiqueta después** a partir de la mira y la potencia. | Con A′ la intención surge del gesto; no hay selector. |
| **C4** | «El ajuste se ve **antes** de soltar» (§7.4.5). | Con A′ la potencia solo se conoce al soltar: el ajuste del solver se muestra **justo después** de soltar (la mira se desplaza al punto factible) y se registra. | Imposible mostrarlo antes con potencia por flick. |
| **C5** | Experimento de controles con A′, B y C en bloques alternados (§6.3); H2 compara las tres. | Solo A′; B y C se añaden únicamente si A′ falla. H2 se evalúa sobre A′. | Decisión del PO del 2026-10-24. |
| **C6** | H3 mide la mejora **entre sesiones**. | Se añade la observación **dentro de la sesión** (10–15 intentos), sin umbral. | Criterio del PO. |

## 9. Decisiones del PO (2026-10-24)

| # | Decisión del PO |
|---|---|
| 0 | **Swing visible aprobado:** brazo y raqueta simples con swing procedural en S3, sin assets complejos ni modelado de personaje. |
| C1 | **Ventana V aprobada solo como hipótesis experimental:** ±100 ms inicial, configurable entre ±80 y ±120 ms, con **T3 puro disponible para comparar**. V no es definitiva hasta la validación del PO en el iPhone. |
| C2 | **Penalización sin desviación lateral**, causal y progresiva, que afecta a altura, velocidad y profundidad según corresponda. |
| N1 | Cancelar voluntariamente el gesto **no consume el intento**. |
| N2 | Un toque accidental sin arrastre **no ejecuta swing ni consume el intento**. |
| C3 | Aprobada la clasificación de la intención después del gesto. |
| C4 | Aprobado mostrar el ajuste del solver después de soltar, con **indicación clara** cuando el objetivo pedido no sea factible. |
| C5 | A′ confirmado como único control inicial; B y C, alternativas si la experiencia no funciona. |
| C6 | Confirmada la evaluación del aprendizaje durante los primeros 10–15 intentos. |
| §7 | Aceptadas todas las decisiones de Claude, incluidos el cuerpo fijo por lanzamiento, los sonidos provisionales, los seis lanzamientos de práctica y las reglas completas de línea diferidas a First Rally. |

### 9.1 Condiciones del PO

1. **El contacto retroactivo conserva el determinismo y la autoridad única de la pelota.** Codex revisa especialmente las marcas de tiempo de iOS, el historial y la resimulación. El salto visual se **mide** y el PO decide en S2 si es aceptable. **No se introducen suavizados** que oculten inconsistencias físicas sin una propuesta explícita.
2. **V y T3 puro se pueden comparar en el panel de desarrollo.** El resultado experimental no se convierte en regla definitiva sin probarlo.

Metodología confirmada: TDD, una PR por incremento, revisión cruzada y aprobación del PO antes de fusionar cada etapa.

### 9.2 Preguntas originales (resueltas arriba)

0. **Representación del swing.** La spec (§7.6.5, H4) pide que, sin el panel, la nota y la **animación del contacto** (estirado, atorado, adelantado) basten para entender un fallo. Hoy el jugador es una cápsula. Recomendación de Claude: un brazo con raqueta simple y un swing procedural sin rig (drive, revés y golpe al aire) en S3, sin assets de personaje. Alternativa: solo nota y superposición, con H4 evaluada con menos información. ¿Cuál prefiere el PO?
1. **C1, ventana válida V.** ¿Se aprueba añadir V como regla experimental (inicial 100 ms) y compararla con T3 puro, o se prefiere T3 puro, donde el fallo solo ocurre si la pelota sale del alcance?
2. **C2, penalización del resultado Válido.** ¿Se acepta que, hasta validar E1/E2, la penalización sea de altura, velocidad y soltura, sin desvío lateral?
3. **N1, cancelación.** ¿Cancelar (volver al origen y soltar) **no** consume el intento? La spec lo propone y está pendiente.
4. **N2, toque accidental.** ¿Un toque corto sin arrastre **no** ejecuta swing ni consume el intento?
5. **C3 y C4, intención y ajuste del solver con A′.** ¿Se aprueban la intención etiquetada después de soltar y el ajuste mostrado justo después de soltar?
6. **C5 y C6.** Confirmación formal de que solo se usa A′ y de la observación dentro de la sesión.
7. **Decisiones de Claude de la §7.** Revisar, sobre todo la 1 (cuerpo fijo) y la 7 (línea de cantado diferida).

## 10. Después de la aprobación

1. Codex revisa técnicamente esta propuesta, en especial el contacto retroactivo, las marcas de tiempo del táctil en iOS con el Input System 1.20 y la háptica nativa, citando documentación oficial.
2. Claude incorpora la revisión y escribe el plan de First Shot por incrementos S1–S4.
3. El PO autoriza S1. Las decisiones aprobadas se registran en la spec cuando el PO lo autorice, como en T7.3.

Pendientes heredados que siguen abiertos: comprobar P0-calib-1 en la próxima apertura, renovar el perfil antes de instalar desde el 27, la nota de corrección «sin frames perdidos» en dos documentos de I7a y las observaciones de Claude sobre el DevPanel.
