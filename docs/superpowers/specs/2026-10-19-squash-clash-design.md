# Squash Clash — Product & Game Design Specification

- **Fecha:** 2026-10-19
- **Versión:** 0.1 (borrador para revisión del PO y de Codex)
- **Autor:** Claude, a partir del brief y del brainstorming con el Product Owner
- **Alcance de este documento:** visión completa del producto, con detalle de implementación solo para el primer milestone (**First Playable Rally**)

## Cómo leer este documento

Cada decisión lleva una etiqueta:

| Etiqueta | Significado |
|---|---|
| **[APROBADO]** | Lo decidió el Product Owner (en el brief o en el brainstorming). No se cambia sin consultarle. |
| **[PROPUESTA]** | Decisión de diseño de Claude. Se puede discutir o cambiar en la revisión. |
| **[SUPUESTO]** | Supuesto técnico o de contexto que todavía no se ha verificado. |
| **[PENDIENTE]** | Lo tiene que decidir el PO. Mientras no lo haga, rige el valor por defecto indicado. |
| **[FUTURO]** | Queda fuera del First Playable. Se documenta para que la arquitectura no lo bloquee, pero no se implementa todavía. |

Roles del proyecto **[APROBADO]**:
- **Product Owner (PO):** exjugador de squash de alto rendimiento. Define la estrategia y los entregables, prueba en el iPhone, valida la física y las tácticas, y decide cuándo se pasa a producción.
- **Claude:** brainstorming, especificación, roadmap, plan de implementación y code review.
- **Codex:** revisa esta spec e implementa en Unity/C#.

Flujo **[APROBADO]**: el PO, Claude y Codex revisan esta spec. Las decisiones de arquitectura marcadas como [PROPUESTA] (por ejemplo, simulación propia de la pelota o núcleo en C# puro) se confirman o se cambian en esa revisión, **antes** de escribir el plan de implementación. El plan se escribe sobre la versión revisada.

---

## 1. Resumen ejecutivo

Squash Clash es un juego móvil de squash para iPhone en vertical. Las partidas duran entre 3 y 5 minutos, a 7 puntos. Combina controles de un solo gesto (apuntar a la pared frontal, soltar con fuerza y en el momento justo) con la profundidad táctica del squash real: mover al rival, cansarlo, variar el ritmo y terminar el punto con algo distinto.

La sensación que se busca la describió el PO. Un rally largo, en el que tú mueves al rival y él te mueve de vuelta, termina con un golpe distinto que define el punto. Después quedas apoyado en la pared, cansado y satisfecho, con ganas de jugar otra vez.

El primer milestone, **First Playable Rally**, solo busca comprobar si el juego es entretenido: cancha con geometría simple, tres golpes (drive, cruzado y drop), saque, cansancio, partido a 7 contra dos IAs con estilos distintos (jugador de fondo y jugador de shot) y un panel de desarrollador para ajustar todo desde el iPhone. Fuera de este milestone quedan el arte final, la progresión, el modo online y la monetización.

---

## 2. Visión y objetivos del producto

### 2.1 Visión
Un juego de squash al que puedes jugar en el metro en 4 minutos, que un jugador de squash reconoce como *su* deporte y que alguien que nunca jugó entiende en el primer rally.

### 2.2 Objetivos
1. **[APROBADO]** Partidas de 3 a 5 minutos, a 7 puntos, sin reloj. Esto reemplaza los 90–180 s del brief original (ver §16.1).
2. **[APROBADO]** Que dé ganas de volver a jugar, al estilo de Tennis Clash o Clash Royale.
3. **[APROBADO]** Que se reconozcan los estilos reales del squash y que el estilo táctico importe.
4. **[APROBADO]** Fácil de aprender y difícil de dominar.
5. **[APROBADO]** Validar el gameplay antes de invertir en arte, progresión o monetización.

### 2.3 No-objetivos (por ahora)
- Lanzamiento comercial. No hay presión por monetizar ni por publicar **[APROBADO]**.
- Simulación fiel al 100 %. La física es semirrealista y la diversión manda **[APROBADO]**.
- Arte final en el First Playable **[APROBADO]**.

### 2.4 Anti-objetivos (cosas que lo harían fracasar aunque funcione)
- **[APROBADO] Pay-to-win o presión constante para ir a la tienda.** Es lo que el PO rechaza de Tennis Clash. Si algún día hay compras, no pueden dar ventaja competitiva.
- **[APROBADO] Errores que se sienten al azar.** Cada error tiene que tener una causa que el jugador pueda entender.
- **[APROBADO] Una IA que devuelve todo perfecto.**
- **[APROBADO] Un aspecto visual genérico, "hecho por IA",** cuando llegue la etapa de arte.
- **[PROPUESTA] Que el jugador se sienta pasivo** porque el personaje corre solo (ver §16.3).

---

## 3. Público objetivo

**[PROPUESTA]**
1. **Primario (fase de validación):** el propio PO y su círculo de jugadores de squash. Si un jugador de alto rendimiento no lo reconoce como squash, fallamos.
2. **Secundario (si el juego avanza):** jugadores de squash y pádel aficionados que buscan un juego de su deporte en el móvil.
3. **Terciario:** jugadores casuales de juegos deportivos móviles (público de Tennis Clash) que nunca jugaron squash.

Consecuencia de diseño: los controles y las ayudas visuales tienen que servir al público terciario sin que el primario los encuentre simplones. Por eso las ayudas (línea de trayectoria, etc.) se pueden ajustar o quitar.

---

## 4. Principios de diseño

Los diez principios del brief son **[APROBADO]**. Los que salieron del brainstorming **[PROPUESTA]** los concretan:

1. **La causa del error se ve.** El jugador sabe *por qué* falló: llegó tarde, pegó mal ubicado, estaba cansado o intentó un golpe difícil.
2. **El riesgo se ve antes de pegar.** La mira cambia de tamaño según lo difícil que es el golpe en ese momento.
3. **Un solo vocabulario de gestos.** Todos los golpes usan el mismo gesto. Lo que cambia es dónde apuntas y con cuánta fuerza.
4. **Las mismas reglas para la IA y para el humano.** La IA usa el mismo sistema de golpes, de errores, de movimiento y de cansancio. Nunca hace trampa.
5. **El estilo se reconoce por cómo juega, no por un número.**
6. **El cansancio es drama, no castigo.** Hace que los rallies largos importen y empuja a tomar decisiones arriesgadas.
7. **Todo lo que afecta la sensación se puede ajustar desde el iPhone** sin compilar de nuevo.

---

## 5. Core gameplay loop

### 5.1 Loop del rally (segundos)
1. El rival pega. La pelota viene hacia tu lado de la cancha.
2. Tu personaje corre solo hacia el punto de impacto **[APROBADO]**.
3. Arrastras el dedo para poner la mira en el frontis y sueltas en el momento justo, con la fuerza que quieres **[APROBADO]**.
4. El golpe sale bien o mal según el timing, la posición, la dificultad, el cansancio y la técnica **[APROBADO]**.
5. Tu personaje vuelve a la T (asistido) o a donde le indiques **[APROBADO]**.
6. Se repite hasta que alguien falla o gana el punto.

### 5.2 Loop del partido (minutos)
1. Saque, rally, punto. Repetir.
2. **Entre puntos [PROPUESTA]:** una pausa breve de 2–3 s con el personaje apoyado en la pared, respirando, y el marcador visible. Se recupera parte de la energía. Se puede saltar tocando la pantalla.
3. El partido termina cuando alguien llega a 7 con 2 de diferencia.

### 5.3 Loop de sesión (revancha)
1. Pantalla final: resultado, duración, rally más largo **[PROPUESTA]** y un botón grande de **Revancha** **[APROBADO]**.
2. Revancha: arranca otro partido contra el mismo rival en menos de 2 toques.
3. **[FUTURO]** Progresión, recompensas, ranking.

### 5.4 Formato de partido
- **[APROBADO]** Un solo game a 7 puntos, con punto en cada rally (PAR).
- **[APROBADO]** **Sin tope de puntos:** con 6-6 se juega hasta que alguien saque 2 de diferencia, como en el squash real.
- **[APROBADO]** Sin límite de tiempo.
- **Objetivo de duración [APROBADO]:** 3–5 minutos. Con 7–13 rallies, eso implica rallies promedio de **15–25 s** y **12–20 golpes** **[PROPUESTA: valores de partida a validar con métricas]**.

---

## 6. Sistema de controles

### 6.1 Gesto principal: apuntar a la pared frontal — **[APROBADO: opción A]**

El PO eligió la opción A en el boceto del brainstorming:

| Parte del gesto | Qué controla |
|---|---|
| **Posición del dedo mientras arrastra** | Una **mira sobre el frontis**. Horizontal = dirección (paralelo o cruzado). Vertical = altura del impacto en el frontis. |
| **Velocidad del dedo en el momento de soltar** | **Fuerza** del golpe. Un movimiento lento sale suave y un *flick* rápido sale fuerte. |
| **Momento de soltar** | **Timing.** Se compara con el momento ideal de impacto. |

- **[PROPUESTA]** El arrastre empieza en cualquier punto de la mitad inferior de la pantalla. La mira se mueve en forma *relativa* al desplazamiento del dedo (no se pone en el punto exacto que toca el dedo), así el pulgar no tapa el frontis.
- **[PROPUESTA]** La sensibilidad de la mira y la curva de velocidad a fuerza se ajustan en el panel.
- **[PROPUESTA]** El gesto se puede empezar en cuanto el rival pega. Antes de eso, la entrada de golpe no hace nada.

### 6.2 Ayudas visuales durante el gesto **[PROPUESTA]**
1. **Anillo de timing** alrededor de la pelota. Se cierra hacia el momento ideal de impacto y cambia de color en la ventana buena.
2. **Mira con tamaño variable:** su radio muestra la zona de error esperada *antes* de pegar (ver §7.4). Una mira grande significa que el golpe es arriesgado.
3. **Línea punteada de trayectoria:** muestra el recorrido previsto de la pelota tras rebotar en el frontis. Su largo se ajusta en el panel (completa, parcial u oculta). **[FUTURO]** Se acorta a medida que el jugador sube de nivel.
4. **Líneas del frontis siempre visibles:** tin, línea de saque y línea de out.

### 6.3 Interpretación contextual
**[APROBADO]** El sistema interpreta la intención según el gesto, la posición, la altura de la pelota y la situación del rally.
**[PROPUESTA]** La intención (punto en el frontis + fuerza) se clasifica en un tipo de golpe con las reglas de §7.2. Después se valida si el golpe es *posible* desde la posición actual. Si no lo es, se ejecuta la versión posible más cercana. Ejemplo: un drop pedido desde lo más profundo de la esquina trasera sale como un drop defensivo, más alto y lento, y la mira ya lo anunciaba con su tamaño grande. Nunca se reemplaza el golpe por otro distinto sin aviso.

### 6.4 Recuperación hacia la T
- **[APROBADO]** Asistida por defecto, con la opción de modificarla tácticamente.
- **[PROPUESTA]** Después de tu golpe, el personaje vuelve solo a la T.
- **[PROPUESTA]** **Tocar el suelo** (sin arrastrar, en la mitad de la cancha que se ve) mientras el rival tiene la pelota mueve tu posición de espera a ese punto, limitado a la zona central (ver §9.3). El ajuste dura hasta tu siguiente golpe. Después vuelve a la T por defecto.
- **[PROPUESTA]** Para distinguir el toque del arrastre: si el dedo se mueve menos de N píxeles y se suelta en menos de M ms, cuenta como toque. Ambos umbrales se ajustan en el panel.

### 6.5 Saque **[PROPUESTA]**
- Mismo gesto. El personaje está en el cuadro de saque y la pelota se lanza al tocar. Se apunta al frontis, entre la línea de saque y la de out. Con el timing se le pega a la pelota en el aire.
- Sin presión de tiempo para empezar a sacar, salvo un tope de 10 s (ajustable) para no frenar el partido.

### 6.6 Principios de pantalla
- **[APROBADO]** Sin sobrecargar la pantalla. **[PROPUESTA]** El First Playable no tiene botones durante el rally. El HUD solo muestra el marcador, la energía de ambos jugadores y un botón de pausa en una esquina.
- **[PROPUESTA]** Diestro por defecto, con la opción zurdo en el panel. El jugador **[PENDIENTE]** decide si la lateralidad del personaje (de qué lado tiene el drive y el revés) se modela en el MVP (ver §18).

---

## 7. Sistema de golpes

### 7.1 Catálogo

| Golpe | First Playable | Notas |
|---|---|---|
| Drive (paralelo) | **[APROBADO] Sí** | Largo, pegado a la pared lateral. |
| Cross court (cruzado) | **[APROBADO] Sí** | Hacia la esquina trasera contraria. Lo ideal es que toque la pared lateral después del bote. |
| Drop | **[APROBADO] Sí** | Suave y bajo, muere adelante. |
| Globo | [FUTURO] | Alto y suave, va por encima del rival. |
| Boast | [FUTURO] | Se pega primero a la pared lateral. **[PROPUESTA]** Se ejecuta llevando la mira más allá del borde del frontis, hacia la pared lateral. |
| Volea | [FUTURO] | Impacto antes del bote. Se activa por contexto cuando la pelota pasa por la zona de alcance antes de botar (ver §16.5). |
| Kill | [FUTURO] | Bajo y fuerte. |
| Nick | [FUTURO] | No es un golpe aparte: es el *resultado* de un kill o un drop que cae en la unión entre la pared lateral y el suelo. |

### 7.2 Clasificación de la intención **[PROPUESTA]**
A partir del punto en el frontis (altura `h`, posición lateral `x`) y la fuerza `p` (0–1):

- **Dirección:** paralelo o cruzado según `x` respecto de la línea que une el punto de impacto con la pared frontal. Es un continuo, no dos categorías cerradas: hay ángulos intermedios, y la pelota puede ir al medio (algo que el PO quiere conservar).
- **Tipo de golpe en el First Playable:**
  - `h` en la banda baja y `p` baja → **drop**.
  - `h` en la banda baja y `p` alta → **drive bajo** (en el futuro será un **kill**).
  - `h` en la banda media → **drive / cruzado**, según la dirección.
  - `h` en la banda alta → **drive alto** (en el futuro, con `p` baja, será un **globo**).
- Los límites de las bandas y de la fuerza son parámetros del panel.

### 7.3 Resolución del golpe **[PROPUESTA]**
1. El sistema calcula la velocidad inicial que lleva la pelota desde el punto de impacto hasta el punto apuntado en el frontis, con la fuerza pedida (un *shot solver* sobre la misma física que usa la simulación).
2. Se aplica el **error** (§7.4) desplazando el punto efectivo en el frontis y ajustando la fuerza.
3. La simulación toma el control a partir de ahí: lo que pase después (pelota suelta al medio, tin, out) es consecuencia física del golpe real, no una decisión aparte.

### 7.4 Calidad del golpe y modelo de error

**[APROBADO]** La calidad depende del timing, la posición, la técnica, la dificultad y el cansancio. Los errores deben ser comprensibles y nunca aleatorios hasta el punto de frustrar.

**[PROPUESTA] Modelo:**

- Cada golpe tiene una **zona de error** en el frontis, centrada en la mira. Su radio es:
  `radio = base(tipo de golpe) × f_timing × f_posición × f_dificultad × f_técnica × f_brazo`
  - `base`: el drop tiene menos tolerancia en altura que el drive (cerca del tin, un error pequeño ya es un fallo).
  - `f_timing`: depende de la nota del timing (§7.5).
  - `f_posición`: distancia entre el punto de contacto real y el ideal para ese golpe. Llegar tarde o estirado lo aumenta.
  - `f_dificultad`: dificultad del golpe pedido desde esa posición (por ejemplo, un cruzado desde la esquina trasera o un drop desde el fondo).
  - `f_técnica`: atributos del personaje (§10).
  - `f_brazo`: cansancio del brazo (§9.4). Pesa poco por sí solo.
- **El error tiene dirección, no solo tamaño [PENDIENTE: validación del PO].** Pegar tarde abre la pelota hacia el medio de la cancha. Pegar temprano la cruza de más. Una mala posición la sube o la deja corta. Dentro de la zona hay una variación aleatoria acotada, pero el *sesgo* lo da la causa. Así un jugador experto puede leer su error.
- **La zona se ve antes de pegar** (radio de la mira, §6.2). Lo aleatorio queda dentro de un riesgo que el jugador vio y aceptó.
- **El golpe al marco y la pelota fallada** solo pueden ocurrir cuando se juntan **cansancio alto y mala posición**. Nunca por azar con buena posición **[APROBADO en concepto por el PO; umbrales en el panel]**.

### 7.5 Timing **[PROPUESTA]**
- Notas: **Perfecto / Bueno / Temprano / Tarde / Fallo.**
- Valores de partida (ajustables): Perfecto ±50 ms, Bueno ±120 ms, Temprano/Tarde hasta ±250 ms.
- Fuera de ±250 ms:
  - **Demasiado temprano:** el personaje ejecuta el swing en el primer instante en que la pelota está a su alcance, con la peor nota de "Temprano". No se pierde el punto de inmediato: sale una pelota mala.
  - **Nunca soltó, o soltó cuando la pelota ya había pasado:** no hay golpe y la pelota sigue su curso (normalmente es punto perdido por segundo bote).
- **[PROPUESTA]** La nota se muestra brevemente (texto pequeño o color) para que el jugador aprenda. Se puede desactivar en el panel.

---

## 8. Física y reglas

### 8.1 Cancha **[PROPUESTA: medidas reglamentarias WSF]**
| Elemento | Medida |
|---|---|
| Largo | 9,75 m |
| Ancho | 6,40 m |
| Línea de out en el frontis | 4,57 m |
| Línea de saque en el frontis | 1,78 m |
| Tin (parte superior) | **[PENDIENTE]** 0,48 m (estándar) o 0,43 m (profesional). Por defecto: 0,48 m. |
| Línea de out en la pared trasera | 2,13 m |
| Línea de out en las paredes laterales | Diagonal del out del frontis al out trasero |
| Short line | 5,49 m desde el frontis |
| Cuadros de saque | 1,60 m × 1,60 m |

### 8.2 Modelo de física **[PROPUESTA]**
- **Simulación propia de la pelota**, no el Rigidbody/PhysX de Unity. Motivos:
  1. El *shot solver* (§7.3), la predicción para el movimiento automático (§9.1) y la línea de trayectoria (§6.2) tienen que calcular *exactamente* la misma trayectoria que luego ocurre. Con PhysX eso no se garantiza.
  2. Permite tests automáticos de rebotes y reglas (necesarios para la code review).
  3. Facilita llevar el juego a Android y, en el futuro, a modo online.
- Paso de tiempo fijo, independiente del framerate (valor de partida: 120 Hz).
- Parámetros por superficie (frontis, laterales, pared trasera, suelo): coeficiente de restitución (rebote) y fricción tangencial.
- Gravedad configurable.
- **[PROPUESTA] Sin efecto Magnus ni spin en el MVP.** El "corte" del drop se representa con una restitución y velocidad menores. **[FUTURO]** Spin como parámetro del golpe.
- **[PROPUESTA]** "Calor" de la pelota (la pelota de squash rebota más cuando se calienta): fuera del MVP, pero queda como posible parámetro.

### 8.3 Velocidad **[APROBADO por el PO como valor de partida]**
- La pelota va más lenta que la real, **alrededor del 60–70 %**, para que se pueda leer en un teléfono vertical.
- Se implementa como una **escala de tiempo de la simulación** y/o una escala de velocidad de los golpes (ambas en el panel). Así se ajusta sin tocar las medidas de la cancha.

### 8.4 Reglas en el First Playable
**[PROPUESTA, basada en las reglas WSF con simplificaciones explícitas]**
- **Golpe válido:** la pelota llega al frontis antes de tocar el suelo. Antes puede tocar las paredes laterales o la trasera. Debe impactar el frontis por encima del tin y por debajo de la línea de out, y nunca tocar las líneas de out de las paredes laterales o la trasera.
- **Fin del punto:** la pelota toca el suelo dos veces, toca el tin, sale fuera, o no llega al frontis.
- **Saque válido:** desde un cuadro de saque, con al menos un pie dentro (en el MVP el personaje se coloca solo). La pelota impacta el frontis entre la línea de saque y la de out, y su primer bote (si el receptor no volea) cae en el cuarto trasero del lado opuesto. **Un solo intento** de saque, como en el squash actual.
- **Rotación del saque:** quien gana el punto saca. El sacador cambia de cuadro con cada punto que gana. Si gana el que recibe, pasa a sacar y elige cuadro (en el MVP, por defecto el derecho).
- **Puntuación:** PAR a 7, diferencia de 2, sin tope **[APROBADO]**.
- **Sin interferencia en el MVP [APROBADO]:** no hay let ni stroke. Los personajes no chocan ni se bloquean, y la IA siempre despeja el camino. Es la simplificación menos fiel al squash real (ver §16.6).
- **Golpear al rival con la pelota:** imposible en el MVP. La pelota no colisiona con los personajes.

### 8.5 Trayectorias en pantalla vertical
**[APROBADO]** Hay que evaluar especialmente cómo representar las trayectorias en vertical. **[PROPUESTA]**
1. **La sombra de la pelota en el suelo** siempre visible. Es la señal más importante de profundidad y altura.
2. **Estela corta** de la pelota (unos pocos frames) para leer la velocidad.
3. **Marca de impacto** breve en las paredes, para leer dónde pegó el rival.
4. **Línea punteada** de tu golpe (§6.2).
5. **[FUTURO]** Indicador de dónde va a botar la pelota del rival, como ayuda opcional para principiantes.

---

## 9. Movimiento y posicionamiento

### 9.1 Movimiento automático **[APROBADO]**
- **[PROPUESTA]** Cuando el rival pega, el sistema predice la trayectoria (con la misma simulación) y elige un **punto de contacto objetivo**: el lugar y el momento óptimos para pegarle a la pelota tras su bote (en el MVP no hay voleas). Prefiere puntos a la altura de la cintura y con la pelota al lado del cuerpo.
- El personaje se mueve hacia ese punto con aceleración y velocidad máximas que dependen de sus atributos y del cansancio.
- **Si no llega a tiempo:** pega desde el mejor punto alcanzable. Eso aumenta `f_posición` y, por lo tanto, el error. Si la pelota queda fuera del alcance (envergadura más estocada), no hay golpe.
- **Tiempo de reacción:** el personaje empieza a moverse un poco después del golpe del rival. Ese retraso depende de la anticipación y del cansancio, y es igual para humano e IA.

### 9.2 Recuperación a la T
Ver §6.4. La velocidad de recuperación también depende de los atributos y del cansancio.

### 9.3 Límites al reposicionamiento manual **[PROPUESTA]**
El toque en el suelo solo puede mover la posición de espera dentro de una zona alrededor de la T, configurable (valor de partida: radio de 2 m). Evita que el jugador se esconda en una esquina y deja el ajuste en el terreno táctico: un paso atrás contra un jugador de fondo, un paso adelante contra uno de shot.

### 9.4 Cansancio **[APROBADO en concepto por el PO]**
- **Se reinicia en cada partido** **[APROBADO]**. **[FUTURO]** En torneos, la opción de acumularlo entre partidos.
- **Una barra de energía por jugador** (0–100). **[PROPUESTA]** Consumo por distancia recorrida, estocadas largas y golpes fuertes. Recuperación parcial entre puntos y mínima durante el rally.
- **Efectos, en orden de peso (la cadena de causas que describió el PO):**
  1. **Piernas:** menos aceleración y velocidad máxima, y más tiempo de reacción.
  2. **Llegar tarde:** consecuencia natural del punto 1. Empeora la posición y el timing, y por eso el error.
  3. **Brazo:** menos fuerza máxima y un aumento pequeño de `f_brazo`.
  4. **Golpe al marco o pelota fallada:** solo con cansancio alto *y* mala posición (§7.4).
- **[PROPUESTA] Cansancio visible:** respiración, un personaje más encorvado y animaciones más lentas, además de la barra.

---

## 10. Sistema de jugadores

### 10.1 Pilares y subatributos **[APROBADO]**
| Pilar | Subatributos |
|---|---|
| Mental | Concentración, resiliencia, consistencia |
| Físico | Velocidad, explosividad, agilidad, resistencia, alcance |
| Técnico | Precisión, potencia, drives, drops, voleas, globos, amagues |
| Táctico | Anticipación, posicionamiento, lectura del rival, selección de golpes |

- **[PROPUESTA]** Escala de 0 a 100. Los datos de cada personaje se configuran fuera del código (archivos de configuración editables).
- **[APROBADO]** Los atributos modifican capacidades, pero no garantizan victorias. **[PROPUESTA]** Por eso cada atributo tiene un efecto *acotado*. Ejemplo: la diferencia entre 50 y 100 en velocidad no supera un X % de velocidad máxima (valor en el panel).

### 10.2 Atributos activos en el First Playable **[PROPUESTA]**
No todos los subatributos tienen efecto en el MVP. Solo se conectan:

| Subatributo | Afecta a |
|---|---|
| Velocidad, explosividad | Velocidad máxima y aceleración (§9.1) |
| Resistencia | Consumo y recuperación de energía (§9.4) |
| Alcance | Radio de golpe y estocada (§9.1) |
| Precisión | `f_técnica` general (§7.4) |
| Drives, drops | `f_técnica` por tipo de golpe |
| Potencia | Fuerza máxima |
| Anticipación | Tiempo de reacción (§9.1) |

Los demás existen en los datos pero no tienen efecto. Se documentarán a medida que se conecten.

### 10.3 Estilos de juego

El PO describió estos estilos reales del squash **[APROBADO como referencia]**. La correspondencia con los arquetipos del brief es **[PROPUESTA]**:

| Estilo (PO) | Arquetipo del brief | Cómo juega | Debilidad que se puede explotar |
|---|---|---|---|
| Físico | Speedster | Llega a todo y alarga el rally | Hay que ganarle el punto varias veces. Exige paciencia y precisión. |
| Fondo / contraataque | — (nuevo) | Pelota larga y pegada a la pared, espera tu error | Le cuesta tomar la iniciativa. Se le gana llevándolo adelante. |
| Shot player | Powerhouse | Busca terminar rápido con drops, kills y nicks | Arriesga mucho y falla más en rallies largos. |
| Alto en la T | Sniper | Domina el centro, volea, llega en 2 pasos | Globos y esquinas traseras: le cuesta arrancar y volver. |
| Mágico | Magician | Amagues, golpes vistosos y cambios de ritmo | Sus golpes difíciles a veces salen mal. |
| Equilibrado | All-Rounder | Sin puntos débiles claros | Sin una receta fija. |

### 10.4 Personajes en el First Playable **[APROBADO en el alcance]**
- **Humano:** un solo personaje, **All-Rounder**, con atributos medios.
- **IA:** dos estilos, **Fondo** y **Shot player** (ver §11).
- **[PROPUESTA]** En el MVP, ambas IAs tienen **los mismos atributos físicos y técnicos** que el humano. La diferencia está *solo en la táctica*. Así se prueba si la IA se reconoce por su forma de jugar sin que los números intervengan. Los atributos distintos por estilo se agregan después.

### 10.5 [FUTURO]
Más personajes, progresión de atributos, diferencias físicas (altura y envergadura visibles), estilos desbloqueables, personalización cosmética. **[APROBADO como principio]** Los atributos se ganan jugando, nunca pagando.

---

## 11. Inteligencia artificial

### 11.1 Principios
- **[APROBADO]** Se desplaza, golpea, varía direcciones, comete errores coherentes, recupera posición y tiene niveles de dificultad. No devuelve todo perfecto.
- **[PROPUESTA]** **Las mismas reglas que el humano:** el mismo movimiento, el mismo *shot solver*, el mismo modelo de error y el mismo cansancio. La IA solo decide *qué* intentar y *cuándo*. La ejecución pasa por los mismos sistemas.

### 11.2 Estructura **[PROPUESTA]**
1. **Percepción:** ve la pelota con el mismo retraso de reacción que el humano (según su anticipación y su cansancio). Su predicción de la trayectoria lleva un pequeño error que depende de la dificultad.
2. **Decisión:** elige el tipo de golpe, la dirección, la altura y la fuerza. Pondera según su **estilo** y la **situación**: su posición, la del rival, su energía, la energía del rival y el marcador.
3. **Ejecución:** genera una intención equivalente al gesto humano (punto en el frontis + fuerza + timing). Su nota de timing sale de una distribución que depende de la dificultad.
4. **Recuperación:** vuelve a la T o a una posición de espera según su estilo y el golpe que hizo.

### 11.3 Estilos en el First Playable **[APROBADO: Fondo y Shot player]**

| | Jugador de fondo | Jugador de shot |
|---|---|---|
| Golpe preferido | Drive y cruzado largos | Drop y drive bajo |
| Margen sobre el tin | Amplio | Pequeño (apunta más cerca del tin) |
| Cuándo ataca | Solo con una pelota muy suelta al medio | Desde el medio de la cancha con cualquier pelota razonable |
| Posición de espera | Un poco detrás de la T | En la T o un poco adelante |
| Rally típico | Largo y paciente | Corto: gana o falla rápido |
| Con cansancio alto | Aumenta un poco el riesgo | Aumenta mucho el riesgo y adivina (se anticipa) |

- **[APROBADO, observación del PO]** Cansados, los jugadores intentan definir antes y adivinan. **[PROPUESTA]** Con poca energía, ambas IAs suben su probabilidad de intentar un golpe ganador y empiezan a anticipar hacia el lado más probable. Eso abre una oportunidad táctica para el humano: un cambio de dirección contra una IA cansada.
- **[PROPUESTA]** La IA reacciona a la posición del humano: busca la esquina contraria a donde está el humano o lo deja lejos de la T.

### 11.4 Dificultad **[APROBADO: niveles; PROPUESTA: 3 niveles]**
| Parámetro | Fácil | Medio | Difícil |
|---|---|---|---|
| Retraso extra de reacción | Alto | Medio | Bajo |
| Error de predicción | Alto | Medio | Bajo |
| Distribución del timing | Más "Temprano/Tarde" | Mayoría "Bueno" | Muchos "Perfecto" |
| Calidad de decisión | A veces elige mal el golpe | Elige razonablemente | Elige bien y explota tus debilidades |

- La dificultad **nunca** cambia las reglas físicas ni los atributos de forma oculta.
- Todos los valores se ajustan en el panel.

### 11.5 [FUTURO]
Más estilos (Físico, Alto en la T, Mágico), IA que aprende tus patrones durante el partido, IA con personalidad (gestos, celebraciones).

---

## 12. Cámara y experiencia visual

### 12.1 Cámara **[APROBADO: 3D detrás del personaje, ligeramente elevada, vertical]**
- **[PROPUESTA]** Cámara situada detrás y por encima de la pared trasera de vidrio, mirando hacia el frontis. En pantalla, el frontis ocupa el tercio superior y se ven las paredes laterales, el suelo y a ambos jugadores.
- **[PROPUESTA]** Seguimiento lateral suave del personaje humano, con límites para que nunca se pierda de vista el frontis. La posición, la altura, el ángulo y el campo de visión se ajustan en el panel.
- **[PROPUESTA]** Cuando un personaje tapa la pelota o al otro jugador, se vuelve semitransparente.
- **[PENDIENTE de evaluación en el prototipo]** Cámara fija o con seguimiento. Se prueban ambas desde el panel.

### 12.2 Estética
- **[APROBADO]** First Playable: geometría simple y personajes provisionales.
- **[APROBADO]** Milestone separado y posterior: dirección de arte, con un estilo **cartoon 3D** propio que no se vea genérico. **[PROPUESTA]** Para la dirección de arte (paleta, tipografía, HUD, menús y referencias de estilo) se pueden usar herramientas como Claude Design o frontend-design. Para el modelado 3D de personajes y cancha harán falta otras herramientas o un artista. Se decide en ese milestone.
- **[PROPUESTA]** Aunque el First Playable sea geometría simple, su legibilidad tiene que estar resuelta: buen contraste entre la pelota, las paredes y el suelo, y líneas de la cancha claras.

### 12.3 Feedback
**[PROPUESTA]** Sonido simple del impacto (pared, raqueta, tin) y vibración háptica breve al pegar y al ganar el punto. El sonido es muy importante en el squash para leer el golpe.

---

## 13. Panel de desarrollador

**[APROBADO]** Panel interno solo para versiones de desarrollo, sin backend.

### 13.1 Acceso y restricción **[PROPUESTA]**
- Se abre con un gesto oculto (por ejemplo, tres dedos mantenidos 1 s) y desde un botón de pausa.
- **No se compila** en builds de release: el código del panel queda fuera mediante un flag de compilación de desarrollo.

### 13.2 Parámetros ajustables
| Grupo | Parámetros |
|---|---|
| Pelota | Escala de velocidad, escala de tiempo, gravedad, restitución y fricción por superficie |
| Personajes | Velocidad, aceleración, alcance, consumo y recuperación de energía, efecto del cansancio |
| Golpes | Bandas de altura, curva de fuerza, radios base de error, pesos de cada factor, sesgo direccional del error, ventanas de timing, umbrales de golpe al marco |
| Controles | Sensibilidad de la mira, umbral toque/arrastre, largo de la línea de trayectoria, mostrar u ocultar la nota de timing |
| IA | Estilo, dificultad, retraso de reacción, error de predicción, distribución del timing, pesos de decisión |
| Cámara | Posición, altura, ángulo, campo de visión, fija o con seguimiento |
| Reglas | Altura del tin, puntos para ganar |

### 13.3 Herramientas de depuración
- **[APROBADO]** Reiniciar el rally, cámara lenta, visualización de trayectorias, posiciones de los jugadores, métricas básicas de rendimiento.
- **[PROPUESTA]** Además:
  - **Modo práctica:** lanzador de pelotas (la "IA" solo alimenta pelotas a una zona elegida), para probar los golpes sin rally.
  - **Superposición de depuración:** zona de error, punto de contacto objetivo, punto de impacto real en el frontis y causa del error.
  - **Pausa y avance frame a frame.**

### 13.4 Presets **[PROPUESTA]**
- Guardar y cargar presets con nombre en el iPhone.
- **Exportar un preset como texto** (copiar al portapapeles o compartir). Así el PO puede enviarle a Codex los valores que le gustaron y estos pasan a ser los valores por defecto del código.

### 13.5 Métricas **[PROPUESTA]**
Visibles en el panel y guardadas en un registro local por partido:
- FPS promedio y mínimo.
- Duración del partido y número de rallies.
- Duración y número de golpes por rally (promedio y máximo).
- Distribución de notas de timing.
- Causas de los puntos terminados (tin, out, segundo bote, ganador).
- Energía promedio al final de cada rally.

Estas métricas sirven para comprobar los criterios de §17.

---

## 14. Definición y alcance del MVP (First Playable Rally)

### 14.1 Dentro **[APROBADO por el PO en el brainstorming]**
1. Cancha con medidas reglamentarias, geometría simple, cámara vertical detrás y arriba.
2. Golpes: drive, cruzado y drop, con la mira en el frontis.
3. Saque con el mismo gesto.
4. Desplazamiento automático, recuperación asistida a la T y ajuste tocando el suelo.
5. Cansancio con la cadena de causas, reiniciado en cada partido.
6. Partido a 7, PAR, diferencia de 2, sin tope.
7. Un personaje humano All-Rounder. Los pilares existen en los datos.
8. Dos IAs (Fondo y Shot player) con 3 niveles de dificultad.
9. Pantalla final con resultado y revancha.
10. Panel de desarrollador con presets.
11. Funciona en el iPhone del PO.

### 14.2 Fuera
Globo, boast, volea, kill, nick; más personajes; progresión; ranking, ligas, torneos, online; tienda, anuncios y compras; dirección de arte y arte final; menús completos; tutorial; Android; let y stroke.

### 14.3 Flujo mínimo de pantallas **[PROPUESTA]**
1. **Inicio:** botón Jugar, selector de rival (Fondo o Shot) y dificultad.
2. **Partido.**
3. **Resultado:** marcador, duración, rally más largo, botones Revancha y Volver.

---

## 15. Roadmap incremental

**[PROPUESTA]** Cada milestone termina con algo que el PO puede probar en su iPhone.

| # | Milestone | Contenido | Cómo lo valida el PO |
|---|---|---|---|
| **M0** | Preparación | Instalar Unity 6 LTS con el módulo iOS (Unity Hub ya está instalado). Crear el repositorio git y GitHub con la misma estructura de docs que RallyIQ. Crear un proyecto Unity vacío y compilarlo al iPhone. | Abre una app vacía en su iPhone. |
| **M1** | Cancha y pelota | Cancha, cámara, simulación propia de la pelota, panel básico, trayectorias de depuración, tests automáticos de rebote y reglas. | Lanza pelotas desde el panel y juzga si los rebotes se sienten de squash. |
| **M2** | Golpear | Gesto, mira, timing, *shot solver*, modelo de error y modo práctica con lanzador. Drive, cruzado y drop. | Pega contra el lanzador y dice si el gesto es intuitivo y los errores se entienden. |
| **M3** | Rally contra IA | Movimiento automático, recuperación a la T, saque, reglas, marcador, cansancio y una IA básica (Fondo). | Juega partidos completos a 7. |
| **M4** | First Playable Rally | IA Shot player, 3 niveles de dificultad, pantalla de revancha, métricas y presets. Ajuste fino. | Criterios de aceptación de §17. **Fin del milestone que pidió el PO.** |
| M5 | Más golpes | Volea (primero, porque es muy importante en el squash), globo, boast, kill y nick. | El juego se siente más completo y táctico. |
| M6 | Dirección de arte | Estilo propio, HUD y menús diseñados, personajes cartoon. | El juego no parece genérico. |
| M7 | Personajes y estilos | Atributos distintos por personaje y nuevos estilos de IA (Físico, Alto en la T, Mágico). | Cada personaje se siente distinto. |
| M8 | Progresión y TestFlight | Experiencia, desbloqueos y pruebas con amigos vía TestFlight. | Otros jugadores vuelven a jugar. |
| M9+ | [FUTURO] | Let y stroke, torneos (con cansancio acumulado opcional), rankings, ligas, online, monetización ética, Android. | — |

---

## 16. Riesgos, desafíos y contradicciones

### 16.1 Duración vs. rallies largos — **resuelto**
El brief pedía partidas de 90–180 s, pero el momento clave para el PO son los rallies largos. A 7 puntos, eso solo daba rallies de 8–12 s. **Resolución [APROBADO]:** partidas de 3–5 minutos.

### 16.2 Sin tope de puntos vs. duración
Sin tope, un 6-6 puede alargar el partido más allá de 5 minutos. **Mitigación [PROPUESTA]:** se mide la duración con las métricas. Si más del 20 % de los partidos supera los 6 minutos, se revisa la velocidad o el ritmo antes de pensar en un tope.

### 16.3 El personaje corre solo: riesgo de pasividad
Si el movimiento es automático, el jugador puede sentir que solo apunta. **Mitigación [PROPUESTA]:** la decisión táctica está en *dónde* mandas al rival y en el ajuste de la posición de espera, y el cansancio da peso a cada decisión. **Alternativa si falla la prueba:** control parcial del movimiento (por ejemplo, arrastrar para elegir el lado del contacto). Se decide con el PO después de M3. **[PROPUESTA — decisión de Claude]** El control parcial *no* entra en M4: si hace falta, se agrega en un milestone posterior. Aun así, desde M3 la entrada se diseña de manera que un segundo modo de control se pueda sumar sin reescribir el reconocimiento de gestos.

### 16.4 Percepción de profundidad en vertical
En una pantalla vertical cuesta leer la altura y la profundidad de la pelota. **Mitigación:** sombra, estela, marcas de impacto y cámara ajustable (§8.5, §12.1). Es el **mayor riesgo técnico de legibilidad** y se valida en M1.

### 16.5 Sin voleas en el MVP
La volea es fundamental en el squash real, y el estilo "Alto en la T" depende de ella. Sin voleas, los rallies pueden sentirse más lentos y menos tácticos. **Mitigación:** la volea es lo primero del M5. La arquitectura de contacto (§9.1) ya contempla elegir un punto de contacto antes del bote.

### 16.6 Sin let ni stroke
Es la simplificación menos fiel al squash. Sin interferencia, un jugador de squash puede notar que "nadie estorba". **Mitigación:** la IA despeja siempre. Se documenta como deuda de realismo para M9.

### 16.7 Error aleatorio vs. "errores no aleatorios"
Hay una tensión: el brief pide errores no aleatorios, pero un modelo de error 100 % determinista sería predecible y aburrido. **Resolución [PROPUESTA]:** aleatoriedad acotada dentro de una zona visible antes del golpe, con un sesgo direccional que depende de la causa (§7.4). El PO valida si se siente justo.

### 16.8 Calibración de la física
Encontrar el equilibrio entre realismo y diversión puede llevar muchas iteraciones. **Mitigación:** todo en el panel, con presets exportables, y el PO como juez.

### 16.9 Curva de aprendizaje de Unity
El PO no ha usado Unity. **Mitigación:** el PO no necesita abrir el editor para probar. Solo instala builds en el iPhone y ajusta desde el panel. Codex y Claude trabajan sobre el código.

### 16.10 Crecimiento del alcance
Hay muchas ideas futuras atractivas. **Mitigación:** este documento separa con claridad lo que entra en el MVP. Cualquier agregado al MVP necesita la aprobación del PO.

### 16.11 Rendimiento en el iPhone
**[SUPUESTO]** Una escena simple con física propia cabe holgadamente en 60 FPS en un iPhone reciente. Se verifica en M0/M1 con el iPhone del PO.

---

## 17. Criterios de aceptación del First Playable Rally

**Criterios del PO [APROBADO]:** las mecánicas funcionan, las dos IAs se diferencian y dan ganas de volver.

**[APROBADO] Carácter de los criterios:** los números de abajo son una **guía flexible**, no una puerta obligatoria. Se ajustan durante el desarrollo según lo que vaya mostrando el juego, y el PO decide cuándo M4 está terminado. El plan de M4 debe tratar el ajuste fino como un ciclo iterativo con el PO, no como un alcance cerrado.

**Versión verificable [PROPUESTA]:**

### 17.1 Técnicos
1. La app se instala y corre en el iPhone del PO a ≥ 55 FPS promedio durante un partido completo (métrica del panel).
2. Hay tests automáticos que verifican, como mínimo: rebote en cada superficie, tin, out en el frontis, en las paredes laterales y en la trasera, segundo bote, saque válido e inválido, rotación del saque y puntuación con diferencia de 2.
3. La línea de trayectoria coincide con el recorrido real de un golpe sin error (diferencia < 5 cm en el impacto en el frontis).
4. El panel no existe en un build de release.

### 17.2 Mecánicas
5. Con timing "Perfecto" y buena posición, el golpe cae dentro de la zona de error mínima en ≥ 90 % de los casos (verificable con el modo práctica y la superposición de depuración).
6. Un drive paralelo con buen timing termina en el cuarto trasero del mismo lado en ≥ 70 % de los casos. Un cruzado, en el cuarto trasero contrario. Un drop, delante de la short line. (Valores de partida: el PO los ajusta.)
7. Cada error se puede explicar con la superposición de depuración: el panel muestra su causa principal.
8. Nunca hay un golpe al marco ni una pelota fallada con buena posición.
9. La duración media de partido, en 10 partidos del PO contra la dificultad media, queda entre 3 y 5 minutos.

### 17.3 IA
10. **Prueba a ciegas:** el PO juega 6 partidos contra un rival elegido al azar (Fondo o Shot) sin saber cuál es. Identifica el estilo correctamente en al menos 5 de 6, antes del 4-4.
11. En las métricas, el jugador de shot termina sus rallies (ganando o fallando) con menos golpes en promedio que el jugador de fondo.
12. En dificultad fácil, el PO gana con comodidad. En difícil, pierde algunos partidos jugando bien. La IA comete errores visibles (tin, pelotas sueltas) en todos los niveles.

### 17.4 Diversión (juicio del PO)
13. Tras una sesión de prueba, el PO juega otro partido por iniciativa propia.
14. Al menos un rally por sesión le da al PO la sensación que describió: mover al rival, que lo muevan y ganar con algo distinto.
15. El PO no se aburre antes del tercer partido. Si se aburre, se analiza la causa antes de pasar a M5.

---

## 18. Decisiones pendientes del Product Owner

Ordenadas por impacto. Entre paréntesis, el valor por defecto mientras no haya decisión.

1. **Sesgo direccional del error** (§7.4): ¿pegar tarde abre la pelota al medio y pegar temprano la cruza de más? ¿Es fiel al squash real? *(Por defecto: sí, como está descrito.)*
2. **Lateralidad del personaje** (§6.6): ¿se distinguen drive y revés en el MVP (lado del cuerpo, calidad distinta por lado)? *(Por defecto: el personaje es diestro y ambos lados tienen la misma calidad. Solo cambia la animación.)*
3. **Altura del tin** (§8.1). *(Por defecto: 0,48 m, estándar.)*
4. **Cámara fija o con seguimiento** (§12.1). *(Por defecto: seguimiento lateral suave. Se prueban ambas.)*
5. **Modelo de iPhone del PO** (para la meta de rendimiento y el iOS mínimo). *(Por defecto: el que tenga el PO.)*
6. **Control parcial del movimiento** si el automático resulta pasivo (§16.3). *(Se decide después de M3.)*
7. **Nombres provisionales** de los rivales de IA. *(Por defecto: "Fondo" y "Shot".)*

---

## Anexo A. Supuestos técnicos

- **[APROBADO]** Unity, C#, macOS, iPhone primero, vertical. Android en el futuro sin reconstruir las mecánicas.
- **[PROPUESTA]** Unity 6 LTS con URP (Universal Render Pipeline) y el Input System nuevo para el tacto.
- **[PROPUESTA]** El núcleo del juego (física de la pelota, reglas, *shot solver*, modelo de error, IA de decisión y puntuación) se escribe como C# puro, separado de los componentes de Unity. Motivos: tests automáticos rápidos (EditMode), Android sin cambios y una base que en el futuro puede servir para el modo online. No se implementa nada de red ahora.
- **[PROPUESTA]** Configuración de golpes, personajes, IA y física en archivos de datos editables (ScriptableObjects), cargados también por el panel.
- **[PROPUESTA]** Git con GitHub y PRs revisados por Claude, con la misma estructura de documentación que RallyIQ (`docs/superpowers/specs`, `docs/superpowers/plans`, `docs/reviews`). Git LFS cuando lleguen los assets de arte.
- **[SUPUESTO]** El PO tiene Xcode 27.1 (verificado), una cuenta de Apple Developer de pago (declarada por el PO) y un Mac con chip M5 (verificado). Unity Hub está instalado (verificado). El editor de Unity está pendiente de instalación por el PO.
- **[APROBADO]** Codex trabaja en el mismo Mac del PO, con el iPhone del PO conectado.
- **[PROPUESTA]** Builds al iPhone en M0–M4: Codex compila desde la línea de comandos (Unity en modo batch → proyecto Xcode → `xcodebuild`) y lo instala directamente en el iPhone conectado. El PO solo prueba. TestFlight a partir de M8.

## Anexo B. Glosario

- **T:** intersección de la short line con la línea media. Es la posición central de dominio.
- **Tin:** franja inferior del frontis. Pegarle es error.
- **PAR:** Point-A-Rally, se gana punto en cada rally, saque quien saque.
- **Drop:** golpe suave que muere cerca del frontis.
- **Boast:** golpe que toca primero la pared lateral.
- **Nick:** pelota que cae en la unión entre la pared lateral y el suelo y no rebota.
- **Let / Stroke:** reglas de interferencia entre jugadores.
- **Shot solver:** cálculo que convierte "quiero pegar en este punto del frontis con esta fuerza" en la velocidad inicial de la pelota.
