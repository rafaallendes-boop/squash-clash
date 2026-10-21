# Squash Clash: Product & Game Design Specification v0.2

- **Fecha:** 2026-10-19
- **Versión:** 0.2. Es la segunda iteración e incorpora la revisión de Codex y el encargo del PO.
- **Reemplaza a:** [v0.1](2026-10-19-squash-clash-design.md), que no se modificó y se conserva como histórico.
- **Documentos relacionados:**
  - [Revisión de Codex](../../reviews/2026-10-19-squash-clash-design-review-codex.md)
  - [Respuesta de Claude, con la matriz de los 26 hallazgos](../../reviews/2026-10-19-squash-clash-design-review-response-claude.md)
- **Autor:** Claude
- **Estado:** borrador para la validación del PO. **No autoriza implementación.**

---

## 0. Cómo leer este documento

### 0.1 Etiquetas

| Etiqueta | Significado |
|---|---|
| **[APROBADO·Dnn]** | Lo decidió el PO. `Dnn` remite al registro §0.3, que indica fuente y cita. Solo el PO puede cambiarlo. |
| **[PROPUESTA]** | Decisión de diseño de Claude. Se puede discutir. |
| **[EXPERIMENTO]** | Valor inicial o variante que se prueba en el iPhone. **No es un compromiso.** Se ajusta con el panel o se reemplaza según la evidencia. |
| **[SUPUESTO]** | Hecho técnico o de contexto que todavía no se ha verificado. |
| **[PENDIENTE PO]** | Lo decide el PO. Mientras tanto rige el valor por defecto indicado. |
| **[FUTURO]** | Queda fuera de los milestones de validación. Se documenta para no bloquearlo. |

Las referencias "(F07)" remiten a hallazgos de la revisión de Codex.

### 0.2 Roles

- **Product Owner (PO):** exjugador de squash de alto rendimiento. Decide producto, alcance, coherencia deportiva y cuándo se avanza.
- **Claude:** especificación, roadmap, plan de implementación y code review.
- **Codex:** revisión crítica e implementación en Unity/C#. Trabaja en el mismo Mac del PO, con el iPhone conectado.

### 0.3 Registro de decisiones del PO (F01)

Fuentes:
- **BS:** brainstorming del 2026-10-19, entre el PO y Claude.
- **R2:** encargo del PO para la v0.2.
- **Brief:** brief original del PO.

| ID | Decisión | Fuente | Evidencia |
|---|---|---|---|
| D01 | iPhone primero, vertical, Unity + C#, macOS. Android en el futuro sin reconstruir mecánicas. | Brief, R2 | Decisiones confirmadas en R2 §2. |
| D02 | **Un game a 7 puntos, diferencia de 2, sin tope de puntos y sin límite de tiempo.** | BS, R2 | BS: "Quizás no llegar a 11 pero si a 7"; "partidas a 7 puntos sin necesidad de un tiempo"; "sin tope, para que sea más realista". R2: "No vuelvas a clasificar el formato a 7 como una decisión pendiente". |
| D03 | Duración de 3–5 minutos como **referencia flexible**. No se modifica la física para forzarla. | BS, R2 | BS: "la idea es que las partidas te duren de 3 a 5 minutos (como el clash royale)". R2: "Si un partido dura más por tener rallies entretenidos, no es necesariamente un problema". |
| D04 | Apuntar a la **pared frontal** (opción A del boceto). | BS | BS: "Me parece la opción A". *El mecanismo de potencia no quedó aprobado: R2 pide compararlo (§6.2).* |
| D05 | Cansancio que se reinicia por partido. Acumulación en torneos como futuro. | BS | BS: "El cansancio se reinicia por partido. En el MVP…". |
| D06 | El cansancio actúa por **cadena causal**: llegar más lento → golpe atrasado → menos precisión. El brazo cansado puede producir un mal golpe. | BS | BS: "ya sin aire llego mas lento a las pelotas, y se retrasa el golpe y no llego bien… el brazo está cansado". |
| D07 | Validar el gameplay antes que el arte. Arte cartoon 3D posterior, con estilo propio. | BS, Brief, R2 | BS: "Validemos primero el gameplay, luego podemos ver los detalles". |
| D08 | Primeros rivales de IA: **jugador de fondo** y **jugador de shot**. | BS | BS: "tengamos a la IA para poder diferenciar como dijimos (el jugador de fondo vs el jugador que juega al shot)". |
| D09 | Sin let ni stroke en el prototipo. Pelota **más lenta que la real**, con 60–70 % como *valor de partida*. Un personaje All-Rounder. Tres niveles de dificultad. | BS | El PO respondió "Me parece bien" al alcance propuesto por Claude. *El 60–70 % y los 3 niveles eran propuestas de Claude aceptadas como punto de partida: se tratan como [EXPERIMENTO].* |
| D10 | Los criterios de aceptación son flexibles y el PO decide cuándo se cumple un milestone. | BS, R2 | BS: "los criterios de aceptación son variables, vamos a ir trabajando en el camino". |
| D11 | Sin pay-to-win y sin presión de tienda. | BS, Brief | BS: "Lo que me molesta… es el pay to win". |
| D12 | Cámara detrás del personaje, movimiento automático, recuperación asistida a la T, gestos contextuales, cuatro pilares, IA como rival inicial, panel de desarrollador, gameplay antes que monetización. | Brief, R2 | R2 §2. |
| D13 | Roadmap First Ball → First Shot → First Rally → First Match → First Fun, sin plazos. | R2 | R2 §4. |
| D14 | Drive, cruzado y drop son los golpes iniciales. Prueba de **volea** como condición de contacto en First Rally. | R2 | R2, Prioridad 4. |
| D15 | La física **no** se decide de antemano. Se elige con criterios después de una prueba técnica autorizada por el PO. | R2 | R2, Prioridad 5. |
| D16 | La IA comparte las reglas con el humano, no conoce decisiones privadas y no anticipa resultados futuros. | R2 | R2, Prioridad 6. |
| D17 | Panel desde el principio, con funciones progresivas y sin backend. | Brief, R2 | R2, Prioridad 7. |
| D18 | Codex trabaja en el mismo Mac del PO. | BS | BS: "Trabajará en el mismo Mac". |

**Nota de trazabilidad:** la v0.1 etiquetaba como [APROBADO] valores que el PO había aceptado como punto de partida (60–70 %) y detalles del gesto que solo se vieron en un boceto (la potencia con flick). En esta versión pasan a [EXPERIMENTO].

### 0.4 Cambios principales respecto de la v0.1

1. **Nuevo contrato de contacto y timing** (§7.1). Era el vacío más grave.
2. **Tres variantes de control** con protocolo de comparación (§6.2–6.3).
3. **Máquina de estados de entrada** (§6.5).
4. **Planificador de movimiento honesto** y profundidad táctica explícita (§9).
5. **Física sin decidir:** criterios, escenarios y regla de decisión (§8.2).
6. **Modelo de error:** aditivo con topes, sin sesgo direccional universal y con una tabla de casos relativos al cuerpo pendiente de validar por el PO (§7.6).
7. **Contrato de información de la IA** y decisión por utilidad (§11).
8. **Roadmap** de cinco milestones de validación (§15).
9. **Criterios** separados en invariantes, hipótesis, objetivos flexibles y pendientes (§17).
10. **Correcciones:** aritmética de la duración (F25), catálogo de golpes (F26) y afirmaciones de arquitectura (F20).

El anexo E detalla el registro completo de cambios.

---

## 1. Resumen ejecutivo

Squash Clash es un juego móvil de squash para iPhone en vertical. Los partidos son a 7 puntos con diferencia de 2 [APROBADO·D02] y duran como referencia entre 3 y 5 minutos [APROBADO·D03]. Combina controles de un solo gesto (apuntar a la pared frontal y soltar en el momento justo [APROBADO·D04]) con la táctica del squash real: mover al rival, ganar la T, cansarlo y terminar el punto con algo distinto.

La experiencia buscada la describió el PO. Un rally largo, en el que mueves al rival y él te mueve de vuelta, termina con un golpe distinto que define el punto. Después quedas apoyado en la pared, cansado y satisfecho, con ganas de jugar otra vez.

El desarrollo avanza por **cinco milestones de validación**, y cada uno responde una pregunta en el iPhone [APROBADO·D13]:

| Milestone | Pregunta |
|---|---|
| First Ball | ¿Se lee la pelota? |
| First Shot | ¿El golpe se siente bien y su resultado se entiende? |
| First Rally | ¿Hay intercambio y decisiones tácticas? |
| First Match | ¿El partido sostiene el ritmo? |
| First Fun | ¿Quiero jugar otra vez? |

Las decisiones técnicas grandes (física, mecanismo de potencia, modelo de contacto) se toman **con evidencia de esas pruebas**, no por adelantado.

---

## 2. Visión y objetivos del producto

### 2.1 Visión
Un juego de squash que se puede jugar en 4 minutos en el metro, que un jugador de squash reconoce como *su* deporte y que alguien que nunca jugó entiende en el primer rally.

### 2.2 Objetivos
1. Partidos a 7 con diferencia de 2, sin límite de tiempo [APROBADO·D02]. Duración de referencia de 3–5 minutos [APROBADO·D03].
2. Dar ganas de volver a jugar.
3. Reconocer los estilos reales del squash y que el estilo táctico importe.
4. Fácil de aprender y difícil de dominar.
5. Validar el gameplay antes que el arte, la progresión o la monetización [APROBADO·D07].

### 2.3 No-objetivos
- Un lanzamiento comercial a corto plazo.
- Fidelidad física del 100 %: el juego es semirrealista y la diversión manda.
- Arte final durante la validación.
- Ajustar la física para cumplir una duración [APROBADO·D03].

### 2.4 Anti-objetivos
- Pay-to-win o presión de tienda [APROBADO·D11].
- Errores que se sienten al azar o sin causa visible.
- Una IA perfecta o que hace trampa [APROBADO·D16].
- Un aspecto visual genérico, "hecho por IA", en la etapa de arte.
- Un jugador pasivo, porque el personaje corre solo y "el piloto automático decide el punto" (F07, §9.5).

---

## 3. Público objetivo

**[PROPUESTA]**, sin cambios respecto de la v0.1:
1. **Primario (fase de validación):** el PO y su círculo de jugadores de squash.
2. **Secundario:** jugadores aficionados de squash y pádel.
3. **Terciario:** jugadores casuales de juegos deportivos móviles.

Las ayudas visuales sirven al público terciario y se pueden reducir o quitar para el primario.

---

## 4. Principios de diseño

Los diez principios del brief están aprobados. Los siguientes son su concreción **[PROPUESTA]**:

1. **La causa del error se ve y se puede medir.** Cada error se descompone en sus fuentes (§7.6) y el panel muestra cuánto aportó cada una.
2. **El riesgo se ve antes de pegar**, como una *estimación* condicionada al timing y la potencia que todavía no se conocen (F15).
3. **Un solo vocabulario de gestos.** La volea y los golpes futuros usan el mismo gesto (§7.3).
4. **Las mismas reglas para la IA y el humano:** física, contacto, error, movimiento y cansancio. Además, la IA solo usa información observable (§11.2).
5. **El estilo se reconoce por cómo juega.**
6. **El cansancio es drama, no castigo.** Actúa solo a través de la cadena causal, sin penalizar dos veces lo mismo (§9.4).
7. **La asistencia nunca regala pelotas.** El movimiento automático llega solo a donde la cinemática del personaje lo permite (§9.1).
8. **Primero aislar y después combinar.** Cada sistema que cambia el rally se introduce y se evalúa por separado (F02).
9. **Una sola autoridad por dato:** una sola simulación de la pelota, un solo cierre de punto y un solo reloj (§8).

---

## 5. Core gameplay loop

### 5.1 Loop del rally
1. El rival le pega a la pelota. Empieza tu fase de **Recepción** (§6.5).
2. El planificador calcula dónde y cuándo puedes pegarle a la pelota, y tu personaje corre hacia ese punto (§9.1).
3. Apoyas el dedo, apuntas en el frontis y defines la potencia según la variante de control (§6.2).
4. Sueltas el dedo. Si lo haces dentro de la ventana, la orden queda **comprometida** y el contacto ocurre según el contrato de §7.1.
5. La calidad depende del timing, la posición, la dificultad y la técnica (§7.6).
6. Empieza tu fase de **Recuperación**: vuelves a la T o a la posición de espera que elijas (§9.3).

### 5.2 Loop del partido
1. Saque, rally, punto.
2. **Pausa entre puntos [PROPUESTA]:** dura 2–3 s, con el personaje apoyado en la pared. Se puede saltar. La recuperación de energía la fija una regla y **no depende de saltar la pausa** (F16).
3. Fin del partido: el primero que llega a 7 con 2 de diferencia [APROBADO·D02].

### 5.3 Loop de sesión
Pantalla final con el resultado, la duración, el rally más largo y un botón de **Revancha** que inicia otro partido contra el mismo rival en menos de 2 toques. **[FUTURO]** Progresión, recompensas y ranking.

### 5.4 Formato del partido [APROBADO·D02] y aritmética corregida (F25)
- Un game a 7, PAR, diferencia de 2, sin tope de puntos y sin límite de tiempo. **Es una adaptación:** el squash oficial juega a 11.
- **Número de rallies:**
  - Si el partido termina antes del 6-6, se juegan entre **7 y 12** rallies.
  - Si se llega a 6-6, hacen falta **14 o más**, sin máximo.

  (La v0.1 decía "7–13", lo cual era incorrecto.)
- **Duración:** la referencia de 3–5 minutos incluye saques y pausas. Sin restarlos, equivale a 15–43 s por rally. Los golpes por rally **se miden**, no se deducen.
- **Política [APROBADO·D03]:** si un partido dura más porque los rallies son buenos, no es un problema. Antes de tocar una física que funciona, se miden la mediana, el p90, el tiempo activo y el tiempo de espera (§17.C).

---

## 6. Sistema de controles

### 6.1 Principios [APROBADO·D04, D12]
- Se apunta a la **pared frontal**.
- Gestos contextuales, con **una mano**.
- Sin botones durante el rally. El HUD solo muestra el marcador, la energía de ambos jugadores y un botón de pausa.

### 6.2 Variantes candidatas para dirección, potencia y timing (F04)

Las tres variantes comparten:
- el **apuntado** en coordenadas del frontis (§6.4);
- el **timing al soltar** el dedo, con el contrato de §7.1.

Solo cambia **cómo se define la potencia**.

| | **A′: gesto original con mira congelada** | **B: apuntar, fijar y tensar** | **C: potencia contextual asistida** |
|---|---|---|---|
| Apuntado | Arrastre relativo | Arrastre relativo | Arrastre relativo |
| Potencia | Velocidad del dedo en los últimos ~60 ms antes de soltar [EXPERIMENTO], convertida con una curva configurable | Con el dedo quieto ≥ 120 ms [EXPERIMENTO], la mira se fija (destello visual). Luego se arrastra **hacia abajo**, como una honda: la distancia en mm define la potencia, con tope | La da el contexto: banda de altura (§7.2) + situación. Un flick rápido al soltar sube **un nivel** (suave → media → fuerte) |
| Protección contra el acoplamiento | Cuando la velocidad del dedo supera `v_flick`, la mira toma la posición que tenía ~50 ms antes y el flick ya no la mueve | Por diseño: la mira está fija mientras se tensa | Por diseño: casi no hay potencia manual |
| Ventaja | Un solo trazo, rápido, natural en vertical | Potencia deliberada y precisa | Lo más fácil de aprender. Aísla dirección y timing |
| Riesgo | Potencia involuntaria | Lento bajo presión: la pausa consume tiempo | Menos profundidad: drop y kill se confunden |

**[PROPUESTA] La v0.1 (flick sin congelar la mira) se descarta en favor de A′,** porque A′ conserva el mismo gesto y corrige el acoplamiento.

### 6.3 Experimento de controles en First Shot [EXPERIMENTO]

- **Dónde:** en el modo práctica, con un lanzador de pelotas. Energía fija y sin azar en el error.
- **Lanzamientos estandarizados (6):** media altura de drive, media altura de revés, esquina trasera del lado del drive, esquina trasera del lado del revés, adelante del lado del drive y adelante del lado del revés.
- **Tareas:** la pantalla indica una zona del frontis y un nivel de potencia. Ejemplo: "drive paralelo, potencia fuerte".
- **Protocolo:** cada variante en bloques de 20 golpes × 3 rondas. El orden se alterna entre sesiones (A′-B-C y luego C-B-A′). Se juega con una mano, diestra y zurda.
- **Métricas registradas:**
  - error entre la mira deseada (la de la tarea) y la mira final, en cm sobre el frontis;
  - potencia involuntaria: |potencia final − potencia pedida|;
  - duración del gesto;
  - distribución de Δ de timing;
  - tasa de descarte y de cancelación;
  - acierto en la zona de destino (primer bote);
  - valoración del PO de 1 a 5 en *control*, *comodidad de la mano* y *se siente squash*.
- **Decisión:** la toma el PO con los datos y su sensación. La variante ganadora es el control del MVP. Las demás pueden quedar como opción de accesibilidad **[FUTURO]**.

### 6.4 Coordenadas de la mira (F08) [PROPUESTA]
- La mira vive en **coordenadas del frontis en metros**: `x ∈ [0; 6,40]` y `y ∈ [0; ~5,0]`. No usa coordenadas de pantalla.
- El desplazamiento del dedo, medido en **mm físicos**, se convierte en metros sobre el frontis con una **ganancia fija** [EXPERIMENTO]. Que la cámara se mueva no mueve la mira.
- **Posición inicial** al apoyar el dedo [EXPERIMENTO]: un punto neutro a ~2,2 m de altura, alineado con un drive paralelo del lado en que está la pelota.
- Se puede apuntar al tin o por encima del out. La mira se ve en rojo. Si el jugador lo elige, el error es suyo.
- **[FUTURO]** Llevar la mira más allá del borde lateral del frontis indica un boast. En el MVP la mira se detiene en el borde.

### 6.5 Máquina de estados de entrada (F05) [PROPUESTA]

**Fases del jugador:**

| Fase | Empieza | Termina | Qué hace un toque |
|---|---|---|---|
| **Saque** | El punto empieza y le toca sacar | Contacto del saque | Gesto de saque (§6.6) |
| **Recepción** | El rival le pega a la pelota | Tu contacto, el fin del punto o el fin de la ventana legal | Gesto de golpe |
| **Recuperación** | Tu contacto | El rival le pega a la pelota | Toque = elegir la posición de espera (§9.3). Un arrastre se ignora. |
| **Fin de punto / Pausa** | Cierre del punto | Siguiente saque | Solo interfaz (saltar la pausa) |

**Reglas:**
1. **El propósito de cada toque se fija al apoyar el dedo** y no cambia aunque la fase cambie. Un toque que empezó en Recuperación nunca se convierte en golpe.
2. **Toque o arrastre:** un desplazamiento < 3 mm en menos de 250 ms es un toque. Los valores son [EXPERIMENTO] y se miden en **mm físicos**, no en píxeles.
3. **Cancelar un golpe:** llevar el dedo de vuelta a un círculo de ~6 mm alrededor del punto de origen y soltar ahí. No se ejecuta nada y se puede iniciar otro gesto si queda tiempo.
4. **Un solo dedo:** cuenta el primer toque. Los demás se ignoran, salvo el gesto oculto del panel en builds de desarrollo.
5. **Perder el foco** (notificación, bloqueo o app en segundo plano): el partido se pausa y el gesto activo se cancela.
6. **Bordes del sistema:** el área de juego excluye la zona del indicador de inicio y las áreas seguras. Se piden a iOS los gestos de borde diferidos.
7. **Lateralidad:**
   - La **mano con que se usa la interfaz** (espeja el HUD y la posición inicial de la mira) se configura aparte de la **mano dominante del personaje**.
   - Por defecto, el personaje es diestro y el drive y el revés tienen la misma calidad [PENDIENTE PO, §18].

### 6.6 Saque [PROPUESTA]
- Usa el mismo gesto. Al apoyar el dedo, el personaje lanza la pelota al aire. T\* es el instante en que la pelota llega a la altura ideal de impacto.
- Aplica el mismo contrato de contacto (§7.1). Excepción: si la orden se descarta o el jugador no suelta a tiempo, **se repite el lanzamiento sin falta**. Fallar el lanzamiento no debe costar un punto.
- Tope de 10 s para iniciar el saque [EXPERIMENTO].

---

## 7. Sistema de golpes

### 7.1 Contrato de contacto y timing (F03) — Prioridad 1

**Estado:** [PROPUESTA] **Modelo de contacto asistido con ventana limitada.** Es una hipótesis para First Shot y **no está aprobada** (R2). Los valores numéricos son [EXPERIMENTO].

#### 7.1.1 Definiciones

- **Ventana legal L = [t_in, t_out]:** instantes en que tu personaje *puede legalmente* pegarle a la pelota. La pelota tiene que haber tocado el frontis después del golpe del rival y no puede haber botado dos veces. Su altura tiene que estar entre `h_min` y `h_max` [EXPERIMENTO]. Sin volea, además, tiene que haber botado una vez. Con volea, se incluye el tramo antes del bote (§7.3).
- **Contacto alcanzable:** un instante de L en que la pelota está dentro del alcance del personaje, considerando su cinemática (§9.1).
- **Plan:** el punto P\* y el instante T\* de contacto elegidos por el planificador (§9.1). El **anillo de timing** se cierra exactamente en T\*.
- **T\*_visible:** el valor de T\* que mostraba el anillo cuando el jugador soltó. **El jugador se evalúa contra lo que vio.**
- **r:** instante en que se soltó el dedo, según la marca de tiempo del evento táctil convertida al reloj de la simulación (§8.4).
- **Δ = r − T\*_visible.**
- **Ventanas [EXPERIMENTO], en tiempo real percibido:**
  - anticipación máxima `E` = 250 ms;
  - atraso máximo `Lt` = 150 ms;
  - Perfecto |Δ| ≤ 50 ms; Bueno |Δ| ≤ 120 ms; el resto es Temprano o Tarde.

#### 7.1.2 Estados de la orden de golpe

`Inactivo → Apuntando → Comprometido → Ejecutado`

Salidas posibles sin golpe: `Cancelado`, `Descartado` y `Sin golpe`.

- **Apuntando:** dedo apoyado en la fase de Recepción. El golpe está **preparado** si además existe un plan alcanzable (el anillo es visible).
- **Comprometido:** soltó dentro de [T\* − E, T\* + Lt]. La mira hace un destello. **Desde aquí no se puede cambiar ni cancelar.**
- **Ejecutado:** ocurrió el contacto.

#### 7.1.3 Casos

| # | Situación | Resultado | Tipo de fallo |
|---|---|---|---|
| 1 | Suelta antes de T\* − E | **Descartado.** Aparece "¡Muy pronto!" y no hay golpe. Puede volver a apuntar si queda tiempo. | Ninguno. El costo es el tiempo perdido. |
| 2 | Suelta entre T\* − E y T\* | **Comprometido.** El contacto ocurre **en T\***, en P\*. La nota sale de Δ. | — |
| 3 | Suelta entre T\* y T\* + Lt, y la pelota sigue en L y al alcance | El contacto ocurre **en r**, en la posición real de la pelota en ese momento. El brazo se ajusta dentro del alcance y **el cuerpo no se teletransporta**. La nota sale de Δ. | — |
| 4 | Suelta después de T\* + Lt, o cuando la pelota ya salió de L o del alcance | **Sin golpe (tarde).** La pelota sigue su curso. | Omisión por tarde. No es un fallo de ejecución. |
| 5 | No suelta, o nunca apoya el dedo, durante toda la ventana | **Sin golpe (sin orden).** | Omisión. |
| 6 | No hay ningún contacto alcanzable | El anillo no aparece y se muestra un indicador de "no llega". El personaje hace su máximo esfuerzo visual. Si suelta, no hay golpe. | **Inalcanzable.** Cuenta como mérito del rival. |
| 7 | El plan cambia **antes** de comprometer | El anillo se actualiza a la vista y no hay penalización. | — |
| 8 | El plan cambia **después** de comprometer (solo si la predicción falló) | El contacto ocurre en el nuevo T\*′ si es alcanzable. La nota se calcula contra T\*_visible. Si deja de ser alcanzable, cuenta como **Inalcanzable** y se registra como *falla del sistema* en las métricas. | Del sistema. Debe ser ≈ 0 (§17.A). |
| 9 | Cancela (vuelve al origen y suelta) | **Cancelado.** Sin golpe. Puede volver a apuntar. | Ninguno. |
| 10 | Golpe al marco o pelota fallada | Solo si se cumplen **cansancio ≥ umbral y calidad de posición < umbral** (§9.4). **Nunca con buena posición.** Solo existe desde First Fun. | Fallo de ejecución. |
| 11 | La pelota ya botó dos veces o no es legal | Nunca hay contacto. | — |
| 12 | Volea habilitada y la pelota pasa por el alcance antes del bote | Ver §7.3. Aplica el mismo contrato con T\* en el tramo antes del bote. | — |

**Esto elimina la contradicción de la v0.1** (§17.2.8 frente a §7.5). La garantía "nunca fallar con buena posición" se refiere solo a los **fallos de ejecución**. La omisión y lo inalcanzable son categorías distintas y el jugador las ve como tales.

#### 7.1.4 Por qué este modelo y no otro
- **Contacto inmediato al soltar:** con el movimiento automático, el cuerpo podría no estar en su lugar. Obligaría a teletransportar el contacto o a fallar mucho. Se descarta.
- **Swing programado** (soltar inicia una preparación de duración fija): es más realista, pero agrega una latencia que hay que aprender y es frágil si la intercepción cambia. **Queda como variante alternativa** si el PO siente que el timing asistido "no se juega".
- **Contacto asistido:** encaja con el movimiento automático, la causa del error es medible (Δ) y los casos se pueden probar uno por uno.

#### 7.1.5 Retroalimentación de timing
- La nota (Perfecto, Bueno, Temprano o Tarde) se muestra brevemente. Se puede desactivar.
- El sonido y la vibración háptica del contacto se adelantan lo menos posible: se dispara en el frame del contacto.
- **[EXPERIMENTO]** Una compensación de latencia del toque, configurable en ms y calibrable en el panel.

### 7.2 Catálogo: intenciones, condiciones de contacto y resultados (F26)

| Clase | Elementos | Estado |
|---|---|---|
| **Intención** (lo que el jugador pide) | Drive paralelo, cruzado, drop | First Shot [APROBADO·D14] |
| | Globo (alto y suave), boast (mira en la pared lateral), **kill** (región bajo y fuerte de la intención de drive) | [FUTURO], milestone Golpes ampliados |
| **Condición de contacto** (cuándo y dónde se le pega) | Tras el bote | First Shot |
| | **Volea** (antes del bote) | Experimento en First Rally [APROBADO·D14] |
| | Tras la pared trasera | Emergente. Se permite si la pelota sigue legal |
| **Resultado emergente** (lo que pasa) | **Nick**, pelota suelta, tin, out, ganador | No se programan como golpes. Si un nick ocurre naturalmente por la física, se respeta. La fidelidad del nick se trabaja en el futuro. |

**Bandas de altura** [EXPERIMENTO]: baja (tin a ~0,9 m), media (~0,9–2,5 m) y alta (> 2,5 m). **Solo sirven para etiquetar** el golpe, elegir la animación y alimentar la IA. **No cambian la física:** la trayectoria es continua en el punto apuntado y la potencia (F12).

Dirección: el paralelo y el cruzado son extremos de un continuo. La pelota puede ir al medio si se apunta ahí o si el error la manda ahí.

### 7.3 Volea como condición de contacto (Prioridad 4) [EXPERIMENTO]
- **No agrega gestos.** Es el mismo gesto y la misma intención (drive, cruzado o drop) con contacto antes del bote.
- **Interruptor en el panel:** volea sí o no. Se comparan los rallies en First Rally.
- **Restricciones de la volea:**
  - solo si la pelota pasa por el alcance entre `h_volea_min` y `h_volea_max` [EXPERIMENTO; partida: 0,6–1,9 m];
  - las ventanas de timing son más estrechas (×0,8 [EXPERIMENTO]);
  - el error base del drop de volea es mayor.
- **Quién decide si se volea** (dos variantes):
  - **V1 (por defecto):** decide el planificador. Prefiere la volea cuando es alcanzable con holgura y el personaje está cerca de la T.
  - **V2:** decide el jugador con su timing. Si el plan admite volea y contacto tras el bote desde posiciones compatibles, el anillo muestra dos momentos. Soltar en el primero es volea y en el segundo es golpe tras el bote. V2 se prueba solo si V1 resulta poco táctico.
- **Qué se mide:**
  - porcentaje de voleas;
  - holgura del rival tras una volea frente a un golpe tras el bote;
  - ocupación de la T;
  - forma en que terminan los rallies.

  Si la volea genera un dominio del centro sin contrajuego, se evalúa adelantar una salida alta simple, tipo globo (F06).

### 7.4 Resolución física del golpe: shot solver (F12) [PROPUESTA]
1. **La potencia es una velocidad de salida concreta:** `v = v_min(intención) + p·(v_max(personaje, energía) − v_min)` en m/s, con `p ∈ [0; 1]`.
2. **Política "se respeta el objetivo":** dados el contacto C, el objetivo P en el frontis y la velocidad v, se busca la dirección de salida que lleva la pelota de C a P. Entre las dos soluciones balísticas se elige **el arco más bajo**.
3. **Objetivo infactible con esa v** (por ejemplo, demasiado alto para una pelota suave, o un arco que bota antes del frontis): se sube **la velocidad mínima necesaria** hasta un tope. Si ni así hay solución, se baja el objetivo hasta el punto factible más cercano.
4. **Se distinguen tres cosas y las tres se registran:**
   - la **intención** (P y p pedidos);
   - el **ajuste asistido** (la corrección del paso 3);
   - la **salida física** (la velocidad final).
5. **El ajuste se ve antes de comprometer:** la mira se desplaza de forma visible al punto factible, o el medidor de potencia indica la corrección.
6. **Orden:** primero se aplica el error (§7.6) sobre P y v, después se resuelve. **Nunca se corrige la salida después** para forzar un acierto.
7. La física es continua. Las bandas no introducen saltos (§7.2).

### 7.5 El drop y el "corte" (F13): diferido a First Shot
- First Shot empieza con **superficies y pelota con propiedades consistentes**. El drop se logra con poca velocidad y un objetivo bajo.
- Si el PO juzga que el drop no "muere" lo suficiente, se agrega un **estado explícito de la pelota** (*cortada*), con menor restitución en el siguiente contacto con el frontis y el suelo. Ese estado lo conocen el predictor, el solver y la IA, y se documenta como una simplificación arcade, no como física.

### 7.6 Calidad del golpe y modelo de error (F14, F15)

#### 7.6.1 Fuentes de error
Las dos primeras salen del brief [APROBADO]. La concreción es [PROPUESTA].

| Fuente | Variable `cᵢ ∈ [0; 1]` | Origen |
|---|---|---|
| Timing | Función de \|Δ\| (§7.1) | Input del jugador |
| Posición | Función de la **calidad de llegada**: holgura, estocada, distancia del cuerpo a la pelota (§9.1) | Planificador y cinemática |
| Dificultad | Dificultad del golpe pedido desde ese contacto: ángulo pedido, cercanía a la pared lateral, altura de la pelota | Geometría |
| Técnica | Atributos (§10) | Personaje |
| Brazo | Energía < umbral (solo desde First Fun, §9.4) | Cansancio |

**El cansancio no es una fuente directa.** Afecta a "posición" y "timing" a través de la cadena causal [APROBADO·D06]. Así no se cuenta dos veces (F16).

#### 7.6.2 Fórmula: aditiva con topes, en lugar de multiplicativa [PROPUESTA]
- `carga = min( Σ wᵢ·cᵢ ; C_max )`, con cada `wᵢ` y `C_max` configurables.
- **El error se separa en tres componentes:**
  - **Vertical (altura en el frontis).** Componente determinista según la causa (§7.6.3) más dispersión `σ_v = base_v(intención)·carga`.
  - **Lateral.** Sin sesgo en la línea base. Dispersión `σ_h = base_h(intención)·carga`. Es anisotrópica: el drop tiene tolerancias distintas en vertical y en lateral.
  - **Potencia.** Una mala calidad resta velocidad: `v_efectiva = v·(1 − k_v·carga)`.
- **Línea base determinista (First Shot y First Rally):** σ = 0. El error solo es el componente determinista (pelota más alta, más corta o más lenta según la causa). Es reproducible y fácil de diagnosticar.
- **Azar acotado (experimento en First Fun):** distribución normal truncada a ±2σ, con una **semilla registrada** por rally. Se compara con la línea base y el PO decide si se queda.
- **Registro:** en cada golpe se guardan `cᵢ`, `carga`, las componentes y el resultado. La superposición de depuración muestra la fuente dominante.
- **La mira previa** muestra la zona estimada *si el timing fuera Perfecto*, porque el timing real todavía no se conoce. Se declara como estimación.

#### 7.6.3 Tabla de casos de error relativos al cuerpo [PENDIENTE PO]

Retiro la regla universal de la v0.1 ("tarde abre al medio, temprano cruza de más"), porque no tenía en cuenta el lado ni la orientación del cuerpo (F14). La propuesta nueva:
- Los casos se expresan **relativos al cuerpo**: contacto adelantado o atrasado respecto del pie delantero, estirado o atorado, del lado del drive o del revés.
- Se proyectan después a la cancha. Por eso son **simétricos al espejar** el lado.
- Esta tabla es **candidata**: **el PO marca cada fila** como correcta, incorrecta o con matiz. Solo las filas validadas entran al modelo, y como pronto en First Fun.

| # | Causa (relativa al cuerpo) | Efecto candidato | Validación del PO |
|---|---|---|---|
| E1 | Contacto **adelantado** (pelota por delante del pie delantero) | La pelota cruza más de lo pedido, hacia el centro o el lado contrario | ☐ |
| E2 | Contacto **atrasado** (pelota a la altura del cuerpo o detrás) | La pelota se va hacia **la pared lateral propia**. Más paralela, con riesgo de tocar la lateral antes del frontis | ☐ |
| E3 | **Estirado** (contacto al límite del alcance, en estocada) | Menos potencia y pelota más alta: **corta y suelta** | ☐ |
| E4 | Pelota **baja** (bajo la rodilla) con contacto atrasado | Trayectoria más baja: **riesgo de tin** | ☐ |
| E5 | **Atorado** (pelota demasiado cerca del cuerpo) | Menos potencia y más dispersión lateral | ☐ |
| E6 | Pelota **pegada a la pared lateral** | Menos ángulo disponible: el cruzado sale corto o al medio | ☐ |
| E7 | Timing perfecto con buena posición | Sin componente determinista | ☐ |

**Línea base (First Shot):** solo se aplican E3, E5 y E7, que no tienen sesgo lateral, más el componente vertical o de potencia por timing. Pegarle tarde o temprano deja la pelota "más suelta" (más alta y lenta), sin sesgo lateral.

---

## 8. Física y reglas

### 8.1 Cancha (medidas WSF)
Igual que en la v0.1: 9,75 × 6,40 m, out del frontis a 4,57 m, línea de saque a 1,78 m, out trasero a 2,13 m, short line a 5,49 m, cuadros de 1,60 m. **Tin:** [PENDIENTE PO], 0,48 m por defecto.
**Pelota:** 40 mm de diámetro, por lo tanto **radio físico de 20 mm** (F10).

### 8.2 Elección de la simulación de la pelota (F09) — Prioridad 5

**Estado:** no decidida [APROBADO·D15]. Se decide después de una prueba técnica en First Ball, autorizada por el PO.

**Principio invariante:** **una sola autoridad sobre el estado de la pelota.** Nunca hay dos sistemas que corrijan alternadamente la misma pelota.

| Alternativa | Descripción |
|---|---|
| **P1: Rigidbody/PhysX** | La pelota es un Rigidbody con CCD. La predicción se hace en una **escena física separada** con simulación manual de paso fijo. |
| **P2: Simulación propia** | Integrador propio, con impacto esfera-plano analítico contra los planos de la cancha. Predicción y juego comparten el mismo código. Unity solo presenta. |
| **P3: Híbrida** | La autoridad sobre la pelota es propia (como P2), y se usan consultas geométricas de Unity (raycast o sweeps) para la detección. Unity presenta y mueve personajes. |

**Criterios de selección:**

| Criterio | Cómo se mide | Tipo |
|---|---|---|
| Fidelidad de la predicción | Desvío entre la trayectoria predicha y la simulada **en el segundo bote**, después de ≥ 3 contactos | Umbral [EXPERIMENTO]: ≤ 1 cm y ≤ 2 ms |
| Robustez de colisiones | Ninguna pelota atraviesa superficies en 10 000 lanzamientos aleatorios | **Invariante** |
| Estabilidad en esquinas | La rapidez después de un contacto doble nunca supera a la de antes. No hay vibración. | **Invariante** |
| Rendimiento | Costo de predecir una trayectoria completa hasta el segundo bote, en el iPhone del PO | Umbral [EXPERIMENTO]: ≤ 1 ms por predicción |
| Facilidad de prueba | ¿Los escenarios corren en EditMode sin escena? | Preferencia |
| Mantenibilidad | Líneas de código y número de conceptos propios a mantener | Juicio |
| Consistencia | Mismo resultado al repetir con el mismo estado inicial en el mismo dispositivo | **Invariante** (no se exige determinismo entre plataformas) |

**Escenarios de prueba (iguales para todas las alternativas):**
- S1: drive paralelo rápido (~28 m/s reales) con 3 rebotes.
- S2: drop bajo, 5 cm sobre el tin.
- S3: rebote rasante en la pared lateral (< 10°).
- S4: unión entre pared lateral y suelo (tipo nick).
- S5: frontis y lateral cerca de la esquina frontal.
- S6: dos contactos dentro de un mismo paso (esquina trasera).
- S7: pelota que sale de la pared trasera.
- S8: 10 000 lanzamientos aleatorios.

**Regla de decisión:** se elige **la alternativa más simple que cumpla todos los invariantes y los umbrales acordados**. No se construyen motores completos: la prueba usa solo una pelota y una caja con las medidas de la cancha.

**Inclinación de Claude (no aprobada):** P2. La cancha son planos alineados con los ejes, el impacto de una esfera es analítico y la predicción comparte código con el juego, así que su fidelidad está garantizada por construcción. P1 gana si cumple los umbrales con menos código propio.

### 8.3 Requisitos de colisión, válidos para cualquier alternativa (F10)
1. Se detecta el **primer impacto dentro del paso**, no un solapamiento al final del paso.
2. Se consume el **tiempo restante del paso** después del impacto, con un tope de N impactos por paso [EXPERIMENTO: 4].
3. **Contactos simultáneos** (dentro de un ε temporal) en esquinas: se resuelven juntos, reflejando cada componente normal.
4. Cada superficie tiene su restitución y su fricción tangencial. Sin spin en el MVP.
5. **Las líneas se evalúan por el contacto de la esfera, no por su centro:** la pelota toca la línea si la huella de contacto (radio configurable, ~r) se superpone con la línea. **Tocar la línea es out**, y tocar el tin es bajo.
6. Paso fijo [EXPERIMENTO: 120 Hz para la pelota], independiente del render.

### 8.4 Relojes, ritmo y velocidad (F11) [PROPUESTA]
- **Un solo reloj de simulación** gobierna la pelota, los personajes, la energía y las ventanas de contacto.
- **Tres controles separados**, nunca combinados sin querer:
  1. **Ritmo global τ:** escala todo el reloj de simulación. La forma de las trayectorias se mantiene igual a la real y el PO puede validarlas. Valor de partida τ ≈ 0,65 [EXPERIMENTO; equivale al "60–70 %" de D09].
  2. **Escala de lanzamiento λ:** multiplica las velocidades de salida. Con la gravedad igual, **cambia la forma** de las trayectorias. Partida: 1,0.
  3. **Cámara lenta de depuración:** solo existe en el panel y marca las métricas de la sesión como "no válidas".
- **[PROPUESTA] Empezar ajustando solo τ.** λ se toca únicamente si el PO pide trayectorias distintas a las reales.
- **Input:** `t_sim(evento) = t_sim(inicio del frame) + (t_evento − t_real(inicio del frame))·τ`. Se usan las marcas de tiempo del evento del Input System, no el frame en que se procesa.
- **Las ventanas de timing se definen en tiempo real percibido** (lo que siente la mano) y se convierten a tiempo de simulación con τ. La IA usa las mismas ventanas convertidas.

### 8.5 Reglas por fase (F24)
Basadas en las [reglas WSF](https://worldsquashofficiating.com/rules-of-squash/), con adaptaciones explícitas.

**Saque:**
- Se saca desde un cuadro de saque. **[Adaptación]** El personaje se coloca solo y no hay foot fault.
- La pelota tiene que ir **directo al frontis**, sin tocar antes otra pared, entre la línea de saque y la de out, sin tocar ninguna de las dos.
- El primer bote (si el receptor no volea) cae en el **cuarto trasero opuesto**, sin tocar sus líneas. El receptor puede volear el saque.
- **Un solo intento.** Un saque inválido es punto para el receptor.

**Retorno:**
- Tiene que llegar al frontis antes de tocar el suelo. **Puede tocar antes las paredes laterales o la trasera.**
- En el frontis, por encima del tin y por debajo del out. En ninguna pared puede tocar la línea de out ni pasarla.

**Cierre del punto:**
- Termina con el **primer evento terminal**: segundo bote, tin, out, pelota que no llega al frontis o saque inválido.
- **El punto se cierra una sola vez.** Los eventos posteriores se ignoran (invariante, §17.A).

**Conteo de botes:** vuelve a 0 con cada contacto de raqueta válido.

**Turno de golpe:** solo puede pegarle el jugador al que le corresponde. Sin interferencia [APROBADO·D09]: los personajes no chocan, la pelota no colisiona con ellos y la IA despeja. **No se presenta como una validación de interferencia real.**

**Rotación del saque y puntuación:**
- Saca el que gana el punto, cambiando de cuadro en cada punto que gana. Si gana el receptor, pasa a sacar desde el cuadro derecho, por defecto.
- PAR a 7, diferencia de 2 [APROBADO·D02].

### 8.6 Trayectorias en vertical
Igual que en la v0.1:
- sombra de la pelota siempre visible;
- estela corta;
- marca de impacto en las paredes;
- línea punteada del golpe propio.

Además (F08): las ayudas no pueden ser *imprescindibles* para entender un rebote, ni tapar al rival. Se validan en First Ball (§12.3).

---

## 9. Movimiento y posicionamiento (F07) — Prioridad 3

**Base aprobada:** movimiento automático hacia la pelota y recuperación asistida a la T [APROBADO·D12]. **El sistema no recupera todas las pelotas** (R2).

### 9.1 Planificador de contacto [PROPUESTA]
- **Cinemática del personaje:**
  - retraso de reacción `d_react` (atributo y, desde First Fun, energía);
  - aceleración `a`;
  - velocidad máxima `v_max`;
  - frenado `b`;
  - alcance `R` (brazo + raqueta, ~1,3 m desde el centro del cuerpo [EXPERIMENTO]);
  - estocada `+R_lunge` (~0,5 m), con mayor costo de recuperación.
- **Candidatos:** puntos de la trayectoria predicha dentro de la ventana legal L, muestreados cada ~20 ms de simulación.
- Para cada candidato se calcula el tiempo de llegada `t_arr` y la **holgura** `s = T − t_arr`.
  - **Legal:** `s ≥ 0` con el alcance normal, o `s ≥ 0` usando la estocada.
  - **Preferido:** el que maximiza una puntuación explícita:
    `U = w₁·min(s, s_cap) + w₂·altura_preferida(0,4–1,1 m) + w₃·separación_de_paredes(≥ 0,3 m) − w₄·estocada`
    [pesos: EXPERIMENTO].
- **Calidad de llegada** `q ∈ [0; 1]`: depende de la holgura y de si hizo falta la estocada. Es la fuente "Posición" del error (§7.6).
- **Estabilidad:**
  - el plan se calcula al terminar `d_react`;
  - se cambia solo si deja de ser legal, o si otro candidato lo supera por un margen de **histéresis** y el plan no está bloqueado;
  - **se bloquea** en `T* − 350 ms` [EXPERIMENTO] o al comprometer la orden.
- **Asistencia extra = 0:** no hay bonos de velocidad ni de alcance para el humano ni para la IA. Si se cambia en el panel para un experimento, queda registrado.
- **Sin candidatos legales:** caso 6 de §7.1 (inalcanzable).

### 9.2 Recuperación a la T
Después del contacto, el personaje vuelve a la posición de espera W (por defecto la T), con la misma cinemática. Mientras se recupera, es vulnerable: si el golpe propio dio poco tiempo, todavía no llegó a W cuando el rival le pega.

### 9.3 Posición de espera elegida por el jugador [PROPUESTA]
- En la fase de **Recuperación**, tocar el suelo define W dentro de un radio de la T [EXPERIMENTO: 2,5 m]. La v0.1 usaba 2 m sin evidencia (F07).
- W dura hasta el siguiente contacto propio y luego vuelve a la T por defecto.
- **Es una apuesta táctica,** el "adivinar" que describió el PO. Acercarse a una esquina reduce `t_arr` hacia ese lado y lo aumenta hacia los demás.
- La IA ve la **posición** del personaje cuando se mueve, nunca la **intención** del toque (§11.2).

### 9.4 Cansancio (F16) [APROBADO·D05, D06; concreción: PROPUESTA]
- **Energía fija hasta First Fun.** En First Rally y First Match la energía no cambia. Esto sirve de control experimental (F02).
- **First Fun, experimento 1 (piernas):**
  - la energía baja con la distancia recorrida, las estocadas y los golpes de potencia alta;
  - reduce `a`, `v_max` y aumenta `d_react`, con un **piso del 70 %** de los valores base [EXPERIMENTO] para que nadie quede inmóvil;
  - el efecto en el golpe aparece **solo** porque llega más tarde (cadena causal).
- **First Fun, experimento 2 (brazo, por separado):**
  - con energía < umbral, baja `v_max` del golpe;
  - se habilita el caso 10 de §7.1 (golpe al marco), solo con mala posición;
  - se mantiene solo si el PO siente una diferencia clara.
- **Recuperación entre puntos:** una cantidad fija por regla, sin importar si se salta la pausa.
- **Una sola barra visible,** con los componentes derivados documentados. Sin modelos fisiológicos múltiples.

### 9.5 Cómo se mantiene la profundidad táctica (Prioridad 3)

| Palanca | Decisión del usuario | Ventaja | Desventaja | Cómo se mide |
|---|---|---|---|---|
| **Elección de golpe** | Paralelo, cruzado o drop, y la altura | Los golpes largos y pegados obligan al rival a recorrer más: **baja su holgura** y su calidad | Los golpes rápidos y bajos te dejan menos tiempo para recuperar | Holgura del rival según el tipo y la precisión del golpe |
| **Recuperación** | Esperar en la T o en otro W | Llegas antes a lo que esperas | Llegas tarde a lo demás | Uso de W, efecto en `t_arr` y resultados |
| **Anticipación** | Mover W antes de que el rival pegue | El "adivinar" funciona si lees bien al rival | Un cambio de dirección te castiga | Aciertos y errores de la apuesta |
| **Dominio del centro** | Golpes que te dejan tiempo para volver | Ocupas la T y el rival corre | Un golpe suelto te deja fuera | % de tiempo en la T por jugador |
| **Riesgo y recompensa** | Apuntar cerca del tin o de la línea | Golpe ganador | Error directo | Margen al tin frente al resultado |
| **Volea** (experimento) | Cortar la pelota antes del bote | Le quita tiempo al rival | Más error y ventana más estrecha | §7.3 |

**Prueba de profundidad (§17.B):** con atributos iguales, los golpes largos y anchos tienen que bajar de forma medible la holgura del rival frente a los golpes al medio. Si no la bajan, la asistencia está regalando pelotas y se revisa antes de seguir.

**Alternativa si sigue sintiéndose pasivo:** control parcial del movimiento, por ejemplo arrastrar para elegir el lado del contacto. Si aparece la necesidad, **se puede volver a First Rally** antes de cerrar First Fun (F07, F25). Por eso la entrada se diseña para admitir un segundo modo de control.

---

## 10. Sistema de jugadores

### 10.1 Pilares [APROBADO·D12]
Mental, Físico, Técnico y Táctico, con los subatributos del brief y una escala de 0 a 100 [PROPUESTA]. **El esquema de datos conserva todos los subatributos.** El código conecta solo los activos (F19, aceptado parcialmente).

### 10.2 Atributos activos durante la validación [PROPUESTA]

| Subatributo | Capacidad afectada | Límite [EXPERIMENTO] | Decisión que sigue siendo del usuario |
|---|---|---|---|
| Velocidad, explosividad | `v_max`, `a` (§9.1) | Entre 50 y 100 cambia como máximo un ±12 % | Dónde esperar y qué golpe elegir |
| Resistencia | Consumo y recuperación de energía | ±20 % | Cuánto arriesgar en un rally largo |
| Alcance | `R`, `R_lunge` | ±10 % | — |
| Técnica (= 0,5·precisión + 0,5·subatributo del golpe) | Fuente "Técnica" del error | ±15 % de la carga | Qué golpe intentar |
| Potencia | `v_max` del golpe | ±10 % | Cuándo pegar fuerte |
| Anticipación | `d_react` | ±40 ms | **No** mueve al personaje por su cuenta: la apuesta táctica (§9.3) sigue siendo del usuario |

**Unificación (F19):** la precisión general y la técnica de cada golpe se combinan en **una sola** fuente, para evitar el doble efecto.

### 10.3 Estilos reales y arquetipos (F26)
Los estilos que describió el PO (Físico, Fondo/contraataque, Shot player, Alto en la T, Mágico, Equilibrado) son la referencia [APROBADO como referencia]. **Se elimina la correspondencia rígida con los arquetipos del brief.** Por ejemplo, Shot ≠ Powerhouse: el Shot se define por cómo elige y cuánto arriesga, no por su potencia. Los nombres de los arquetipos son provisionales.

### 10.4 Personajes durante la validación
- **Humano:** un All-Rounder [APROBADO·D09].
- **IA:** Fondo y Shot [APROBADO·D08], **con los mismos atributos que el humano**. La diferencia es solo táctica.

### 10.5 Validación de los pilares
- **Los milestones First Ball–First Fun no validan el sistema de personajes** (F19). Esto se declara de forma explícita.
- **Milestone Personajes [FUTURO]:** pares de personajes con ventajas y debilidades compensables, y un efecto mental pequeño, observable y recuperable. Por ejemplo, la concentración baja tras varios errores seguidos y se recupera en la pausa. Nunca se traduce en fallos aleatorios inevitables.
- **[FUTURO]** Progresión. Los atributos se ganan jugando, nunca pagando [APROBADO·D11].

---

## 11. Inteligencia artificial — Prioridad 6

### 11.1 Principios [APROBADO·D16]
- La IA comparte las reglas con el humano.
- No conoce las decisiones privadas del jugador.
- No anticipa resultados que todavía no ocurrieron.
- No es perfecta.

### 11.2 Contrato de información (F17) [PROPUESTA]

| | Contenido |
|---|---|
| **Observable** (con latencia de percepción `d_perc` según dificultad [EXPERIMENTO: 0–80 ms]) | Posición y velocidad pasadas de la pelota. Posición y velocidad de ambos personajes. Marcador y quién saca. Barras de energía (también visibles para el humano en el HUD). Estado de las reglas. |
| **Prohibido** | El toque, el gesto, la mira o la potencia del humano. Si el humano tiene una orden comprometida y su Δ. La posición de espera elegida *antes* de que el personaje se mueva. Los objetos internos del planificador del rival. **Los valores aleatorios futuros.** El resultado de un golpe antes de que ocurra. |
| **Permitido con simetría** | Predecir la trayectoria de la pelota *a partir del estado observado*, con el predictor compartido. El planificador del humano hace lo mismo para moverlo, así que es simétrico. **La IA nunca predice mejor que el planificador del humano**: la dificultad solo puede añadirle ruido (`σ_pred`). |

**Latencias separadas, nunca sumadas sin control:**
1. `d_react` del **personaje**: atributo, igual fórmula para ambos.
2. `d_perc` de **percepción** de la IA: dificultad.
3. `d_dec` de **decisión** de la IA: el plazo para elegir el golpe, según dificultad.

La reacción real del humano es su propia mano y no se simula.

### 11.3 Estructura percepción → decisión → ejecución → recuperación (F18) [PROPUESTA]

**1. Percepción.** Estado observado con `d_perc` y ruido `σ_pred`.

**2. Decisión por utilidad sobre un conjunto pequeño de opciones factibles:**
- **Opciones:** {paralelo, cruzado, drop} × {altura baja, media, alta} × {potencia media, fuerte}. Más las mismas con volea, si está habilitada y es factible.
- **Rasgos de contexto:**
  - holgura propia (presión);
  - distancia propia a la T;
  - posición del humano (adelante o atrás, de qué lado, qué tan lejos de la T);
  - margen estimado al tin;
  - tiempo de vuelo, que es tiempo para recuperar;
  - energía de ambos (desde First Fun).
- **Utilidad:** `U(opción) = Σ peso_estilo·rasgo − aversión_al_riesgo·riesgo_estimado`.
- **Elección:** softmax con temperatura según dificultad. Hay variación entre opciones razonables, sin alternancias fijas.
- **Defensa obligatoria:** si la holgura es menor que `s_def`, solo se consideran opciones de supervivencia (alta y larga).

**3. Ejecución, por el mismo contrato que el humano (§7.1):**
- La IA genera la intención (P, p) con **ruido de intención** `σ_int` según dificultad.
- Muestrea su instante de soltar `r = T* + Δ`, con `Δ ~ N(0, σ_timing(dificultad))`.
- Por lo tanto **puede quedar Descartada o sin golpe** si Δ cae fuera de la ventana, igual que el humano.
- El sistema compartido decide el contacto y su calidad.

**4. Recuperación.** Posición de espera según el estilo. Puede "apostar" (§9.3) usando solo información observable.

**Fuentes de error de la IA, cada una con tope y registradas por separado:**
- `σ_pred` (percepción);
- `σ_int` (intención);
- `σ_timing` (ejecución);
- el modelo de error compartido (§7.6).

### 11.4 Estilos [APROBADO·D08; pesos: EXPERIMENTO]

| Rasgo | Jugador de fondo | Jugador de shot |
|---|---|---|
| Peso de paralelo y cruzado largos | Alto | Medio |
| Peso del drop | Bajo; solo con el humano atrás y holgura propia alta | Alto desde media cancha |
| Aversión al riesgo (margen al tin) | Alta | Baja |
| Posición de espera | ~0,5 m detrás de la T | En la T o ~0,3 m adelante |
| Con energía baja (First Fun) | La aversión al riesgo baja un poco | La aversión baja mucho y apuesta más a anticipar |

- **Contrajuego esperado:**
  - al jugador de fondo se le gana llevándolo adelante, porque le cuesta tomar la iniciativa;
  - al jugador de shot se le gana alargando el rally, porque arriesga y falla.
- **Estilo ≠ habilidad.** El estilo cambia los pesos y la dificultad cambia `d_perc`, `σ_pred`, `σ_int`, `σ_timing` y la temperatura.

### 11.5 Dificultad [APROBADO·D09 tres niveles; valores: EXPERIMENTO]
- Hay tres niveles: Fácil, Medio y Difícil, definidos por los parámetros de §11.3.
- La dificultad **nunca** cambia la física, las reglas, los atributos ni la información disponible.
- No se exige "un tin por partido" para demostrar que la IA es humana (F18). Sus errores salen de sus fuentes de error.

### 11.6 [FUTURO]
Estilos Físico, Alto en la T y Mágico. Aprendizaje de patrones durante el partido. Personalidad.

---

## 12. Cámara y experiencia visual

### 12.1 Cámara (F08) [APROBADO·D12: detrás, elevada y vertical]
- **Referencia experimental: cámara fija** detrás y por encima de la pared trasera, mostrando el frontis, las laterales y ambos jugadores.
- **Variante:** seguimiento lateral amortiguado. **Mientras el jugador apunta, el seguimiento se congela** o se reduce al mínimo.
- La mira no depende de la cámara (§6.4).

### 12.2 Estética [APROBADO·D07]
- Durante la validación se usa geometría simple. Aun así, la **legibilidad tiene que estar resuelta**: contraste entre la pelota y el fondo, y líneas claras.
- Milestone **Dirección de arte [FUTURO]:** estilo cartoon 3D propio. Claude Design o frontend-design sirven para la paleta, el HUD y los menús. Los modelos 3D necesitan otras herramientas, que se deciden en ese momento.

### 12.3 Escenarios de legibilidad en First Ball [EXPERIMENTO]
Se prueban con la cámara fija y con seguimiento, con ambas manos y en el iPhone real:
1. Drive largo que rebota en la pared trasera.
2. Pelota baja pegada a la pared lateral.
3. Drop corto adelante.
4. Pelota alta (tipo globo, lanzada por el lanzador).
5. Pelota que pasa detrás del personaje humano (oclusión).
6. Pelota en la esquina trasera con el rival cerca.

El PO debe poder distinguir altura y profundidad **sin abrir el panel**.

### 12.4 Retroalimentación sensorial (F25) [PROPUESTA]
Desde First Shot:
- **Sonido distinto** para la raqueta, el frontis, las laterales, el tin y el fin del punto.
- **Vibración háptica** breve en el contacto.
- **Alternativa visual** para cada evento (destello en la pared, marca roja en el tin), por si se juega sin audio.

---

## 13. Panel de desarrollador — Prioridad 7

### 13.1 Acceso y restricción [APROBADO·D17; PROPUESTA]
- Se abre con tres dedos mantenidos 1 s o desde el menú de pausa.
- **El código del panel queda fuera de los builds de release** gracias a un flag de compilación. Se verifica con una prueba (§17.A).

### 13.2 Funciones por milestone (crecimiento progresivo)

| Milestone | Funciones nuevas |
|---|---|
| First Ball | Lanzador (posición, velocidad, dirección, presets de lanzamiento). Reinicio. Física (gravedad, restitución y fricción por superficie, τ, λ). Cámara (fija o con seguimiento, posición, FOV). Trayectorias de depuración. Cámara lenta. Avance **por pasos de simulación**. FPS y tiempos de frame. |
| First Shot | Variante de control (A′, B o C) y sus parámetros. Ventanas de timing. Bandas. Pesos del error. Superposición de depuración (contacto, P\*, T\*, Δ, fuentes de error). Instrumentación del input. **Guardar y exportar presets**, que tienen que estar listos para las comparaciones. |
| First Rally | Planificador (pesos, histéresis, bloqueo). Alcance. Radio de W. Volea sí o no y su variante. Parámetros de la IA. Visualización de la holgura y del plan. |
| First Match | Reglas (tin, puntos para ganar). Métricas de ritmo y duración. |
| First Fun | Energía (experimentos 1 y 2). Azar acotado y semilla. Estilos y dificultad. Prueba a ciegas (rival al azar oculto). |

### 13.3 Herramientas de depuración
Reiniciar el rally, cámara lenta, trayectorias, posiciones y métricas (del brief), además de:
- modo práctica con lanzador;
- superposición de contacto y error;
- pausa con avance paso a paso.

### 13.4 Reglas de cambio y persistencia (F21) [PROPUESTA]

**Clases de parámetros:**

| Clase | Ejemplos | Cuándo se aplica |
|---|---|---|
| **Inmediata** | Cámara, ayudas visuales, mostrar la nota de timing | En el acto |
| **Próximo rally** | Física, golpes, ventanas, error, planificador, IA | Al empezar el siguiente rally. El panel muestra "cambios pendientes" y ofrece **"Aplicar y reiniciar rally"**. **Nunca se aplican a mitad de un gesto.** |
| **Reinicio de partido** | Reglas, puntos para ganar, estilo de la IA | En el siguiente partido |

**Persistencia:**
- Los presets se guardan como **JSON local versionado** en el almacenamiento de la app, con nombre, versión de esquema, unidades y rangos.
- Al arrancar se carga el último preset activo. Hay que verificar que se recupera tras reiniciar la app.
- Los ScriptableObjects contienen los **valores por defecto de autoría**. En tiempo de ejecución se trabaja con una copia. El estado de la partida (energía, marcador) **nunca** se guarda en la configuración.

**Trazabilidad:** cada registro de métricas guarda el preset (nombre y hash), la versión del build y la semilla.

**Exportación:** el preset (y opcionalmente las métricas) se exporta como texto mediante el menú de compartir de iOS o el portapapeles. Así el PO se lo pasa a Codex y los valores se vuelven los nuevos valores por defecto.

**Sin backend** [APROBADO·D17].

### 13.5 Métricas
El listado completo está en §17.E.

---

## 14. Definición y alcance del MVP por milestone

**"MVP"** son los cinco milestones de validación. El alcance de la v0.1 [APROBADO·D09] se mantiene completo y solo se **reparte** entre milestones (F02). No se agrega ni se quita funcionalidad, con una excepción: la **prueba de volea** [APROBADO·D14].

| Sistema | Ball | Shot | Rally | Match | Fun |
|---|---|---|---|---|---|
| Cancha, cámara, pelota, rebotes | ✔ | ✔ | ✔ | ✔ | ✔ |
| Prueba técnica de física y su decisión | ✔ | | | | |
| Gesto (A′, B y C) y contrato de contacto | | ✔ | ✔ | ✔ | ✔ |
| Drive, cruzado y drop | | ✔ | ✔ | ✔ | ✔ |
| Modo práctica con lanzador | (lanzador) | ✔ | ✔ | ✔ | ✔ |
| Error en su línea base determinista | | ✔ | ✔ | ✔ | ✔ |
| Retroalimentación de sonido, háptica y visual | | ✔ | ✔ | ✔ | ✔ |
| Movimiento automático, planificador, T y W | | | ✔ | ✔ | ✔ |
| IA básica (una política, Fondo) | | | ✔ | ✔ | |
| Reglas del retorno y fin del rally | | | ✔ | ✔ | ✔ |
| **Experimento de volea** | | | ✔ | | |
| Saque, rotación, puntuación a 7 y revancha | | | | ✔ | ✔ |
| Pausa entre puntos | | | | ✔ | ✔ |
| Dos estilos y tres dificultades | | | | | ✔ |
| Cansancio (experimentos 1 y 2) | | | | | ✔ |
| Azar acotado y sesgos validados por el PO | | | | | ✔ |
| Panel | ✔ (progresivo, §13.2) | ✔ | ✔ | ✔ | ✔ |

**Fuera del MVP:**
- globo, boast y kill como intenciones;
- más personajes y validación de los pilares;
- progresión, ranking, ligas, torneos y online;
- tienda y anuncios;
- dirección de arte;
- let y stroke;
- Android.

---

## 15. Roadmap incremental [APROBADO·D13]

Son **puntos de validación**, sin plazos. Cada uno termina con una decisión del PO: **continuar, ajustar o volver a una pregunta anterior**. La diversión se observa desde First Shot. First Fun la *consolida*, no la pospone.

| Milestone | Pregunta | Qué se prueba en el iPhone | Salida (guía, la decide el PO) |
|---|---|---|---|
| **Preparación** (dentro de First Ball) | ¿La cadena de herramientas funciona? | App vacía instalada por Codex desde la línea de comandos | Unity 6 LTS con el módulo iOS instalado. **Compatibilidad Unity–Xcode 27.1 verificada.** Repositorio git creado. |
| **First Ball** | ¿Se lee la pelota en vertical? ¿Qué física usamos? | Lanzador, rebotes y cámaras. Escenarios de legibilidad (§12.3). | El PO distingue altura y profundidad. Invariantes de física cumplidos. **Decisión de física** según §8.2. Rendimiento base medido. |
| **First Shot** | ¿Puedo pegar con intención y entender el resultado? | Práctica con lanzador. Variantes A′, B y C. Drive, cruzado y drop. | Variante de control elegida. Timing comprensible. Los fallos tienen una causa visible. El drop se siente drop (§7.5). |
| **First Rally** | ¿Hay intercambio y decisiones que cambian el punto? | Rally contra una IA básica (Fondo) sin saque: empieza con un lanzamiento neutro. Energía fija. Volea sí y no. | La asistencia no devuelve todo. Los golpes profundos bajan la holgura del rival. W tiene efecto. Resultado de la volea. |
| **First Match** | ¿El partido sostiene el ritmo? | Saque, reglas completas, marcador a 7, pausa, revancha. | Reglas estables. Revancha inmediata. Duración **observada**. |
| **First Fun** | ¿Quiero repetir porque aprendo y puedo jugar distinto? | Dos estilos, tres dificultades. Cansancio, azar y sesgos introducidos **por separado**. | El PO reconoce los estilos y su contrajuego, entiende los errores sin el panel y pide revancha en varias sesiones. |
| Golpes ampliados [FUTURO] | ¿Qué golpe falta para la táctica? | Globo, boast, kill y fidelidad del nick, según lo que falte | Cada golpe agrega una decisión nueva |
| Dirección de arte [FUTURO] | ¿Tiene identidad propia? | Estilo, HUD, menús y personajes cartoon | No parece genérico |
| Personajes [FUTURO] | ¿Los pilares diferencian sin reemplazar decisiones? | Pares de personajes compensables y efecto mental | Ventajas legibles |
| Progresión y TestFlight [FUTURO] | ¿Otros vuelven a jugar? | Progresión y amigos vía TestFlight | — |
| M9+ [FUTURO] | — | Let y stroke, torneos con cansancio acumulado, ranking, ligas, online, monetización ética, Android | — |

---

## 16. Riesgos, desafíos y contradicciones

| # | Riesgo | Estado | Mitigación |
|---|---|---|---|
| 16.1 | Duración frente a rallies largos | Resuelto (D02, D03) | La duración es una referencia flexible. Se mide la mediana y el p90 (§5.4). |
| 16.2 | Sin tope de puntos en 6-6 | Aceptado por el PO | Se mide. No es motivo para tocar la física. |
| 16.3 | Pasividad por el movimiento automático | **Abierto** | Planificador honesto y palancas tácticas (§9.5). Se puede volver a First Rally (§9.5). |
| 16.4 | Profundidad difícil de percibir en vertical | **Abierto, el mayor riesgo de legibilidad** | §8.6, §12.3. Se valida en First Ball. |
| 16.5 | Sin volea | Mitigado | Experimento en First Rally (§7.3). |
| 16.6 | Sin let ni stroke | Aceptado (D09) | No se presenta como validación. Queda para M9+. |
| 16.7 | Errores aleatorios frente a errores comprensibles | Replanteado | Línea base determinista, azar acotado con semilla como experimento (§7.6). La v0.1 suponía sin pruebas que lo determinista sería aburrido: se retira. |
| 16.8 | Calibración de la física | Abierto | Panel, presets exportables, escenarios de referencia (anexo C). |
| 16.9 | Curva de aprendizaje de Unity para el PO | Mitigado | El PO no necesita el editor. Codex compila e instala. |
| 16.10 | Crecimiento del alcance | Mitigado | La tabla §14 es el alcance. Cualquier cambio requiere al PO. |
| 16.11 | Rendimiento | Abierto | §17.A y §17.B, medidos en el iPhone del PO. |
| 16.12 | **Compatibilidad entre Unity 6 LTS y Xcode 27.1 / iOS** | **Nuevo, sin verificar** | Es lo primero que se verifica en la Preparación. Si no hay soporte, se decide entre otra versión de Unity o de Xcode. |
| 16.13 | **Acoplamiento entre potencia y puntería** | Nuevo (F04) | Experimento de §6.3. |
| 16.14 | **Penalizaciones dobles** (cansancio, posición, timing) | Nuevo (F16) | Sin término directo de cansancio. Fórmula con topes (§7.6.2). |
| 16.15 | **La predicción no coincide con la simulación** | Nuevo (F09) | Criterio de §8.2. Caso 8 de §7.1 medido como falla del sistema. |

---

## 17. Criterios de aceptación y métricas

Los criterios son una **guía flexible** y el PO decide cuándo se cumple un milestone [APROBADO·D10]. Se separan en cuatro clases (R2 §6). **Solo la clase A es obligatoria.** Las demás se ajustan con la evidencia, y ningún valor experimental se convierte en una restricción permanente.

### 17.A Invariantes técnicas (obligatorias)
1. **Una sola autoridad sobre la pelota.** Ninguna pelota atraviesa superficies (0 de 10 000 en S8). En esquinas, la rapidez nunca aumenta tras un contacto.
2. **Consistencia:** el mismo estado inicial produce el mismo resultado en el mismo dispositivo.
3. **El punto se cierra una sola vez.** Ningún evento posterior cambia el marcador.
4. **Contrato de contacto:** los 12 casos de §7.1.3 tienen pruebas automáticas y las pasan. No hay contacto después del segundo bote ni fuera del alcance.
5. **Ningún fallo de ejecución con buena posición** (caso 10).
6. **Reglas:**
   - pruebas de saque directo al frontis frente a uno que toca antes la lateral;
   - líneas por contacto de la esfera;
   - tin;
   - out en cada pared;
   - segundo bote;
   - rotación del saque;
   - diferencia de 2;
   - turno de golpe.
7. **Simetría entre humano e IA:** las restricciones de alcance, ventanas y error son idénticas. Hay una prueba de que la IA no accede a los datos prohibidos (§11.2).
8. **Panel excluido del release.** Un build de release no contiene sus tipos.
9. **Fallas del sistema** (caso 8 que termina en inalcanzable): se registran y su objetivo es ≈ 0.
10. **Presets:** se recupera el preset tras reiniciar la app. El registro de métricas incluye el preset, el build y la semilla.

### 17.B Hipótesis de jugabilidad (se prueban y pueden fallar)
| # | Hipótesis | Evidencia | Milestone |
|---|---|---|---|
| H1 | En vertical, la pelota se lee sin el panel | Escenarios de §12.3, juicio del PO | Ball |
| H2 | Una de las variantes A′, B o C permite apuntar y dar potencia con intención, con una mano | Métricas de §6.3 y juicio del PO | Shot |
| H3 | El contacto asistido con ventana hace que el timing "se juegue" | Distribución de Δ que mejora con la práctica, juicio del PO | Shot |
| H4 | Los errores se entienden sin el panel | El PO explica la causa de un fallo en la mayoría de los casos | Shot y Fun |
| H5 | El movimiento automático no vuelve pasivo al jugador | Los golpes profundos bajan la holgura del rival, W tiene efecto medible, juicio del PO | Rally |
| H6 | La volea agrega profundidad táctica | Comparación con y sin volea (§7.3) | Rally |
| H7 | Los estilos se reconocen y tienen contrajuego | **Prueba a ciegas exploratoria:** rival oculto al azar, el PO lo identifica. *No es evidencia estadística* (F23). | Fun |
| H8 | El cansancio agrega drama sin espiral de derrota | Experimentos 1 y 2 por separado, juicio del PO | Fun |
| H9 | El azar acotado mejora el juego frente a la línea base determinista | Comparación con semilla | Fun |
| H10 | El juego da ganas de revancha en **varias** sesiones | El PO juega otra partida por iniciativa propia y recuerda una decisión táctica útil | Fun |

### 17.C Objetivos flexibles (valores de partida, ajustables)
- **Rendimiento:**
  - 60 FPS como meta, con presupuesto de 16,7 ms por frame;
  - p95 del tiempo de frame ≤ 16,7 ms y p99 ≤ 25 ms [EXPERIMENTO hasta conocer el iPhone];
  - frames > 33 ms: ≤ 1 por minuto;
  - sesión de 15 minutos sin estado térmico "serious";
  - predicción ≤ 1 ms.
- **Latencia del input:** del toque a la retroalimentación del contacto ≤ 50 ms adicionales al frame [EXPERIMENTO].
- **Duración:**
  - mediana de 3–5 minutos como referencia;
  - p90 registrado;
  - tiempo activo frente a tiempo de espera.

  *No se cambia la física por esto* [APROBADO·D03].
- **Golpes por rally:** se miden y no tienen objetivo fijo.
- **Precisión (escenarios del anexo C):** se registran los eventos completos de cada golpe: primer y segundo bote (lugar y tiempo), margen al tin y desvío respecto del objetivo. Los porcentajes objetivo los fija el PO al ver los datos.

### 17.D Decisiones pendientes que afectan a los criterios
Ver §18. En particular: el modelo de iPhone (umbrales de rendimiento), la tabla de errores (§7.6.3) y los escenarios de referencia (anexo C).

### 17.E Métricas registradas
| Grupo | Métricas |
|---|---|
| Tiempo de contacto | Distribución de Δ, nota, estado de la orden (comprometida, descartada, cancelada, sin golpe), T\* frente al contacto real |
| Latencia del input | Marca del evento → procesamiento → contacto → retroalimentación |
| Precisión | Mira deseada frente a final (experimento de controles), objetivo frente a impacto real en el frontis, primer y segundo bote, margen al tin |
| Rebotes y trayectorias | Desvío entre predicción y simulación tras varios contactos, contactos por paso, eventos de esquina |
| FPS y estabilidad | Tiempos de frame (p50/p95/p99), frames largos, uso de CPU y GPU, estado térmico, duración de la sesión |
| Rallies | Duración activa, golpes, forma en que termina (tin, out, segundo bote, ganador, inalcanzable, sin orden) |
| Errores | `cᵢ` por fuente, carga, componentes y fuente dominante |
| Decisiones tácticas | Distribución de golpes por contexto, uso de W, acierto de la apuesta, % de tiempo en la T, holgura del rival según el golpe, uso de la volea |
| Partido | Duración (mediana y p90), rallies, revanchas por sesión |

---

## 18. Decisiones pendientes del Product Owner

Ordenadas por impacto. Entre paréntesis, el valor por defecto.

| # | Decisión | Cuándo | Valor por defecto |
|---|---|---|---|
| P1 | **Aprobar el contrato de contacto asistido** (§7.1) como hipótesis para First Shot. | Antes del plan | Contacto asistido. El swing programado queda como alternativa. |
| P2 | **Autorizar el experimento de controles A′, B y C** (§6.3) y su protocolo. | Antes del plan | Las tres variantes. |
| P3 | **Validar la tabla de casos de error relativos al cuerpo** (§7.6.3). Marcar cada fila. | Antes de First Fun (cuanto antes, mejor) | Solo E3, E5 y E7. |
| P4 | **Modelo de iPhone y versión de iOS.** | Antes de la Preparación | — |
| P5 | **Autorizar la prueba técnica de física** (§8.2) y aceptar sus escenarios y umbrales. | Antes de First Ball | La regla de §8.2. |
| P6 | Aceptar la **posición de espera** (§9.3) como mecanismo de anticipación, solo en la fase de Recuperación. | Antes de First Rally | Sí, con un radio de 2,5 m. |
| P7 | **Lateralidad:** ¿el drive y el revés tienen calidades distintas durante la validación? | Antes de First Shot | Mismo nivel. Solo cambia la animación. |
| P8 | **Altura del tin.** | Antes de First Ball | 0,48 m. |
| P9 | Revisar y corregir los **escenarios deportivos de referencia** (anexo C). | Antes de First Shot | Los del anexo C. |
| P10 | Revisar el **registro de decisiones** (§0.3). | Al aprobar la v0.2 | — |
| P11 | ¿Quién decide la volea, el planificador (V1) o el jugador con su timing (V2)? | Durante First Rally | V1. |
| P12 | Mecanismo de ralentización: ritmo global τ o escala de lanzamiento λ. | Después de First Ball | τ. |
| P13 | Nombres provisionales de los rivales. | Cuando quiera | "Fondo" y "Shot". |

---

## Anexo A. Supuestos y lineamientos técnicos (F20)

- [APROBADO·D01] Unity, C#, macOS, iPhone y vertical.
- [SUPUESTO] **Unity 6 LTS** con el módulo iOS. **Hay que verificar su compatibilidad con Xcode 27.1** (riesgo 16.12). Unity Hub está instalado.
- [PROPUESTA] URP y el Input System nuevo, usando las marcas de tiempo de los eventos (§8.4).
- [PROPUESTA] **Arquitectura proporcional:**
  - **Dos ensamblados.**
    - *Lógica:* reglas, contrato de contacto, solver, modelo de error, decisión de la IA y puntuación. Sin dependencias de escena, para que las pruebas EditMode sean rápidas.
    - *Integración:* presentación, input, cámara, panel y el adaptador de física.
  - El adaptador de física **puede usar Unity** si gana P1 o P3.
  - Corrección de la v0.1: **el C# puro no es un requisito para Android** (Unity ya es multiplataforma) y **el modo online no justifica ninguna abstracción hoy**. La frontera existe solo porque abarata las pruebas.
  - **No se usan:** ECS/DOTS, contenedores de inyección, buses globales de eventos, frameworks genéricos de habilidades, motores de red, aritmética determinista ni motores físicos de propósito general.
- [PROPUESTA] ScriptableObjects para la autoría de valores por defecto, con copia en tiempo de ejecución, y presets JSON (§13.4).
- [PROPUESTA] Git y GitHub con PRs revisados por Claude. Estructura `docs/superpowers/specs`, `docs/superpowers/plans` y `docs/reviews`. Agregar `.superpowers/` al `.gitignore`.
- [APROBADO·D18] Codex trabaja en el Mac del PO. [PROPUESTA] Compila con Unity en modo batch, genera el proyecto Xcode y lo instala en el iPhone conectado con `xcodebuild` / `devicectl`. TestFlight llega con la progresión.
- [SUPUESTO] El PO tiene Xcode 27.1 con la licencia aceptada (verificado), cuenta de Apple Developer de pago (declarada) y un Mac con chip M5 (verificado).

## Anexo B. Glosario

- **T:** intersección de la short line con la línea media.
- **Tin:** franja inferior del frontis.
- **PAR:** se gana un punto en cada rally.
- **Holgura:** tiempo de margen con que el personaje llega al punto de contacto. Si es negativa, no llega.
- **T\*, P\*:** instante y punto de contacto planificados.
- **Δ:** diferencia entre el momento de soltar y T\*.
- **W:** posición de espera durante la Recuperación.
- **τ:** ritmo global de la simulación.
- **Shot solver:** cálculo de la velocidad de salida a partir del objetivo y la potencia.
- **Orden comprometida:** golpe que ya no se puede cancelar.
- **Nick, drop, boast, volea, let/stroke:** igual que en la v0.1.

## Anexo C. Escenarios deportivos de referencia [PENDIENTE PO: P9]

Sirven para calibrar y para las métricas de precisión. El PO corrige los valores con su experiencia. Todos se lanzan con timing Perfecto, buena posición y la línea base determinista.

| # | Escenario | Resultado esperado (candidato) |
|---|---|---|
| C1 | Drive paralelo desde el cuarto trasero, a media altura | 1.er bote detrás de la short line. 2.º bote cerca de la pared trasera o contra ella. Trayectoria a < 0,5 m de la lateral ("pegado"). |
| C2 | Cruzado desde el cuarto trasero | Toca la lateral contraria **después** del 1.er bote, a la altura del jugador que recibe ("ancho"). 2.º bote en el cuarto trasero contrario. |
| C3 | Drop desde adelante (pelota cómoda) | Margen sobre el tin < 15 cm. 2.º bote a < 2 m del frontis. |
| C4 | Drop desde atrás (defensivo) | Más alto y lento que C3. 2.º bote a < 3 m del frontis. Es más fácil de alcanzar para el rival. |
| C5 | Drive con mala posición (estirado) | Pelota más alta y corta: 1.er bote por delante de la short line ("suelta"). |
| C6 | Volea de drive desde la T (experimento) | El rival recibe con menos holgura que con el mismo golpe tras el bote. |
| C7 | Saque al cuadro contrario | Golpea el frontis entre la línea de saque y el out. 1.er bote en el cuarto trasero contrario. |

## Anexo D. Trazabilidad de los hallazgos de Codex

| Hallazgo | Sección de la v0.2 |
|---|---|
| F01 | §0.1, §0.3 |
| F02 | §14, §15 |
| F03 | §7.1 |
| F04 | §6.2, §6.3 |
| F05 | §6.5 |
| F06 | §7.3 |
| F07 | §9.1, §9.3, §9.5 |
| F08 | §6.4, §12.1, §12.3 |
| F09 | §8.2 |
| F10 | §8.1, §8.3 |
| F11 | §8.4 |
| F12 | §7.4 |
| F13 | §7.5 |
| F14 | §7.6.3 |
| F15 | §7.6.2 |
| F16 | §9.4 |
| F17 | §11.2 |
| F18 | §11.3, §11.4 |
| F19 | §10.1, §10.2, §10.5 |
| F20 | Anexo A |
| F21 | §13.2, §13.4 |
| F22 | §16.12, §17.A, §17.C, §17.E |
| F23 | §17, anexo C |
| F24 | §8.5 |
| F25 | §5.4, §12.4, §17.C |
| F26 | §7.2, §10.3 |

## Anexo E. Registro de cambios respecto de la v0.1

| Sección | Cambio |
|---|---|
| Etiquetas | Se agregan [EXPERIMENTO] y el registro de procedencia de las decisiones. |
| §2.2, §5.4 | Formato a 7 ratificado (D02). La duración pasa a ser una referencia flexible (D03). Aritmética corregida. |
| §6 | El flick simple pasa a A′ con mira congelada. Se agregan B y C, el experimento de comparación, las coordenadas del frontis y la máquina de estados de entrada. Se precisa la cancelación. |
| §7 | Nuevo contrato de contacto. Catálogo en tres clases. Volea como condición de contacto. Contrato del solver. Drop diferido. Modelo de error aditivo con topes. Se retira el sesgo universal y se agrega la tabla relativa al cuerpo. |
| §8 | Física no decidida: tres alternativas, criterios, escenarios y regla de decisión. Requisitos de colisión. Tres controles de ritmo y velocidad. Reglas por fase. |
| §9 | Planificador con cinemática, holgura, histéresis y bloqueo. Asistencia extra = 0. W con radio experimental de 2,5 m. Energía fija hasta First Fun. Fatiga sin término directo. Tabla de profundidad táctica. |
| §10 | Validación parcial explícita. Tabla de atributos activos con límites. Técnica unificada. Sin correspondencia rígida con los arquetipos. |
| §11 | Contrato de información. Latencias separadas. Decisión por utilidad. La IA ejecuta por el mismo contrato de contacto. |
| §12 | Cámara fija como referencia. Escenarios de legibilidad. Retroalimentación sensorial desde First Shot. |
| §13 | Funciones por milestone. Clases de cambio. Persistencia en JSON versionado. Trazabilidad de preset, build y semilla. |
| §14–15 | El alcance se reparte en cinco milestones. Se agrega la Preparación con la verificación Unity–Xcode. |
| §16 | Riesgos nuevos: 16.12 a 16.15. |
| §17 | Clases A, B, C, D y E. Invariantes obligatorias. Hipótesis H1–H10. Objetivos flexibles. Métricas completas. |
| §18 | Trece decisiones pendientes con su valor por defecto. Se eliminan las ya resueltas (formato, tope, cansancio por partido). |
| Anexos | A reescrito. Nuevos C (escenarios), D (trazabilidad) y E (este registro). |
