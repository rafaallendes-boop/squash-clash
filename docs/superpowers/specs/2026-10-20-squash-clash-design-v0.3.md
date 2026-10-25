# Squash Clash: Product & Game Design Specification v0.3

- **Fecha:** 2026-10-20
- **Versión:** 0.3. Consolida las directrices del PO sobre la revisión focalizada de la v0.2 y cierra la fase de diseño funcional.
- **Reemplaza a:** [v0.2](2026-10-19-squash-clash-design-v0.2.md). La v0.2 y la [v0.1](2026-10-19-squash-clash-design.md) no se modificaron y se conservan como histórico.
- **Documentos relacionados:**
  - [Revisión de Codex](../../reviews/2026-10-19-squash-clash-design-review-codex.md)
  - [Respuesta de Claude, con la matriz de los 26 hallazgos](../../reviews/2026-10-19-squash-clash-design-review-response-claude.md)
  - [Revisión focalizada de la v0.2](../../reviews/2026-10-19-squash-clash-v0.2-focused-review-claude.md)
- **Autor:** Claude
- **Estado:** borrador para la revisión del PO. **No autoriza implementación ni plan técnico.** Si el PO la aprueba, el siguiente paso es el plan técnico incremental de First Ball.

**Alcance del cambio:** solo se modificaron las secciones afectadas por las directrices R3 y sus referencias cruzadas. El resto del texto es el de la v0.2. El §0.4 resume los cambios y el anexo E los detalla sección por sección.

---

## 0. Cómo leer este documento

### 0.1 Etiquetas

| Etiqueta | Significado |
|---|---|
| **[APROBADO·Dnn]** | Lo decidió el PO. `Dnn` remite al registro §0.3, que indica fuente y cita. Solo el PO puede cambiarlo. |
| **[HIPÓTESIS·Dnn]** | El PO lo aprobó como **hipótesis principal** que se prueba en el iPhone. Es la opción por defecto, pero la evidencia puede reemplazarla. El reemplazo lo decide el PO. |
| **[PROPUESTA]** | Decisión de diseño de Claude. Se puede discutir. |
| **[EXPERIMENTO]** | Valor inicial o variante que se prueba en el iPhone. **No es un compromiso.** Se ajusta con el panel o se reemplaza según la evidencia. |
| **[SUPUESTO]** | Hecho técnico o de contexto que todavía no se ha verificado. |
| **[PENDIENTE PO]** | Lo decide el PO. Mientras tanto rige el valor por defecto indicado. |
| **[FUTURO]** | Queda fuera de los milestones de validación. Se documenta para no bloquearlo. |

Las referencias "(F07)" remiten a hallazgos de la revisión de Codex. Las referencias "(FR1)" a "(FR6)" remiten a los seis puntos de la revisión focalizada de la v0.2, y "PO-n" a sus decisiones.

### 0.2 Roles

- **Product Owner (PO):** exjugador de squash de alto rendimiento. Decide producto, alcance, coherencia deportiva y cuándo se avanza.
- **Claude:** especificación, roadmap, plan de implementación y code review.
- **Codex:** revisión crítica e implementación en Unity/C#. Trabaja en el mismo Mac del PO, con el iPhone conectado.

### 0.3 Registro de decisiones del PO (F01)

Fuentes:
- **BS:** brainstorming del 2026-10-19, entre el PO y Claude.
- **R2:** encargo del PO para la v0.2.
- **Brief:** brief original del PO.
- **R3:** directrices del PO para la v0.3 (2026-10-20), en respuesta a la revisión focalizada. Los números de sección (R3 §n) son los del mensaje del PO.

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
| D19 | **Timing T3**, asistencia mínima con consecuencia física, como hipótesis principal de First Shot. Tolerancia inicial de ±40 ms, configurable en el panel. Fuera de ella, el instante real de contacto tiene consecuencias físicas observables. Se conserva una **variante de asistencia suave** para comparar si T3 frustra. **Un solo intento de golpe por recepción, con cancelación explícita.** | R3 | R3 §1 (PO-1): "Aprobar T3 (asistencia mínima con consecuencia física) como hipótesis principal para First Shot"; "Permitir un solo intento de golpe por recepción, con cancelación explícita". |
| D20 | Se permite el **golpe al aire** cuando el jugador se anticipa demasiado. Es una categoría distinta del error de ejecución y de la pelota inalcanzable. | R3 | R3 §1 (PO-2): "Permitir golpes al aire cuando el jugador se anticipa demasiado, diferenciándolos de errores de ejecución o pelotas inalcanzables". |
| D21 | Tres niveles de ayuda visual de timing: **completa, parcial y sin anillo**. | R3 | R3 §1 (PO-3). |
| D22 | Hay que validar que **el timing mejora con la práctica** y que **anticiparse sistemáticamente no es una estrategia dominante**. | R3 | R3 §1. |
| D23 | **Volea V2 + V3** como hipótesis principal y V1 como control experimental. La posición de espera influye en las oportunidades de volea. El jugador decide cuándo interceptar con su timing, y el planificador **nunca volea automáticamente**. Solo se muestran las oportunidades físicamente alcanzables y relevantes, no necesariamente dos anillos. Se puede dejar pasar la pelota cuando sea viable, **sin garantía** de alcanzar el golpe posterior. Hay voleas técnicamente posibles pero tácticamente inconvenientes: pelota pegada a la pared, altura incómoda o mala posición corporal. | R3 | R3 §2 (PO-4, PO-5). |
| D24 | **Principio:** el movimiento automático asiste el desplazamiento, pero no reemplaza las decisiones tácticas del jugador. | R3 | R3 §2: "el movimiento automático asiste al desplazamiento, pero no reemplaza las decisiones tácticas del jugador". |
| D25 | **Anexo C en tres capas** (capacidad expresiva, relaciones comparativas y bandas de calibración flexibles), con la validación deportiva final del PO. Cada relación comparativa declara sus **condiciones de validez** y ninguna se convierte en invariante universal. | R3 | R3 §3 (PO-6): "No convertir en invariantes universales afirmaciones que dependen del ángulo, altura, velocidad, posición de contacto o rebotes". |
| D26 | **Error E-C**, que separa la consecuencia causal de la variabilidad aleatoria. El timing actúa principalmente a través de la geometría del contacto, **sin penalizaciones duplicadas**. La posición corporal y la dificultad situacional producen consecuencias comprensibles, y la técnica puede reducir la magnitud de ciertos errores causales. La variabilidad aleatoria está **apagada en las primeras validaciones**. En First Fun se prueba una variabilidad pequeña, acotada y reproducible, en la que pueden influir la dificultad del golpe y la Consistencia (pilar Mental). **No se establece que el estado mental sea la causa dominante del error.** | R3 | R3 §4 (PO-7, PO-8): "No establecer todavía que el estado mental sea la causa dominante de los errores. Debemos comprobar si su incorporación realmente mejora la experiencia". |
| D27 | Física: se incorpora la corrección técnica (FR5) y se elimina toda afirmación de fidelidad garantizada que no esté verificada. La elección entre PhysX, simulación propia e híbrida sigue abierta. Se elige **la solución más sencilla que cumpla los requisitos medidos** y **no se desarrollan tres motores completos** para compararlos. | R3 | R3 §5. Complementa D15. |
| D28 | Líneas: una **huella de contacto configurable** como simplificación experimental y una **marca visual del contacto** que explica las decisiones ajustadas de tin, out y saque. Las dimensiones y reglas oficiales de la WSF se **verifican antes de implementar**. Se distinguen las líneas de out, tin y saque, incluidas las del suelo, y **no se asume que tocar una línea tenga la misma consecuencia en todas las fases**. Toda adaptación arcade se documenta. | R3 | R3 §6 (PO-9, PO-10). |
| D29 | **Panel parte del MVP desde First Ball.** Crece con cada milestone e incluye controles de física, timing, movimiento, IA, cansancio y errores, además de herramientas de diagnóstico. Los parámetros experimentales se ajustan y se guardan en **presets locales**, sin modificar código para cada calibración. Sin backend y excluido de las builds de producción. | R3 | R3 §7. Concreta D17. |

**Ratificación (R3 §8):** el PO mantiene sin cambios D01, D02, D03, D07, D08, D09 (sin let ni stroke), D11, D12 y D13. También mantiene los personajes provisionales antes del arte cartoon 3D (D07, §10.4) y su atribución de decidir cuándo un milestone está validado (D10).

**Nota de trazabilidad:** la v0.1 etiquetaba como [APROBADO] valores que el PO había aceptado como punto de partida (60–70 %) y detalles del gesto que solo se vieron en un boceto (la potencia con flick). Desde la v0.2 pasan a [EXPERIMENTO]. En la v0.3, los **valores numéricos** que acompañan a D19–D29 (±40 ms, ~10 mm, etc.) siguen siendo [EXPERIMENTO]: el PO aprobó el mecanismo y el valor de partida, no un valor definitivo.

#### Registro First Ball autorizado en T7.3 — 2026-10-24

Registro de decisiones posteriores a R3, con [autorización documental del PO para T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24), ejecutada tras el [cierre de R1](../../reviews/2026-10-21-first-ball-po-approvals.md#cierre-r1--2026-10-24). D15/D27 conservan el proceso de selección original; D30 registra su resultado para First Ball.

| ID | Decisión | Fuente | Evidencia |
|---|---|---|---|
| D30 | **P2-Analytic es el backend de First Ball**, por la regla de §8.2.3: alternativa más simple que cumple todos los invariantes y umbrales medidos. **P1 falla C3/C10/C11/C17** y queda como respaldo configurable, sin usarse por defecto. | PO, 2026-10-23; registro T7.3 autorizado el 2026-10-24. | [Decisión del gate y comparación de los 18 criterios](../../reviews/2026-10-23-first-ball-physics-backend-decision.md). |
| D31 | Aclaración **D-4**: §8.3.1–8.3.3 y §8.3.7 se miden en **C12–C15**, sin umbral numérico nuevo. `physics.maxImpactsPerStep` no es operativo con P1; **sí es operativo con P2**. | [D-4 de la respuesta a Claude](../../reviews/2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación); [autorización PO T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24). | [P2: tope configurable, pruebas con topes 1/2 y C14 con tope 4, cero alcanzados](../../reviews/2026-10-23-first-ball-physics-spike-P2.md#autorización-y-contrato). |
| D32 | Aclaración **D-5**: **DefaultPreset.json es la única fuente de defaults**; validación de esquema, tipos y rangos. Sustituye la propuesta de ScriptableObjects de §13.4 y anexo A. | [D-5](../../reviews/2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación); [autorización PO T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24). | Directriz 11 del plan r2, aplicada en la configuración y presets existentes. |
| D33 | Aclaración **D-6**: la diagonal lateral mide **50 mm perpendiculares a la diagonal**, con el **borde inferior** como referencia (§8.7.2). | [D-6](../../reviews/2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación); [autorización PO T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24). | Precisión de la interpretación geométrica; no cambia la geometría implementada ni los criterios. |
| D34 | **First Ball aceptado por el PO con desviación H17 p95**: H17-1 **17,2 ms**, H17-2 **16,8 ms**, frente a **≤16,7 ms**. Ambos resultados se conservan como **Fail**, sin cambiar umbral, restar sesgo de cubeta ni repetir. El umbral sigue **[EXPERIMENTO hasta conocer el iPhone]**. | [Decisión PO tras revisión I7a](../../reviews/2026-10-21-first-ball-po-approvals.md#revisión-final-claude-y-aceptación-po-de-first-ball--2026-10-24); [registro solo en §0.3 autorizado para T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24). | P99, frames >33 ms/min, térmico y DroppedSteps cumplen; distribución estrecha en torno a 16,7 ms (oscilación de cadencia, sin frames perdidos), fundamento de aceptación expresado por PO. **§17.C/§17.D permanecen intactos.** |
| D35 | **No promover P0-calib-1 a DefaultPreset.json**. Solo difiere en `launcher.repeatSeconds=3`, que no es física. Defaults efectivos **0d39e6016bd8** intactos, con P2 por defecto. | [Decisión PO, paso 2 de T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24). | [Punto de control de Etapa 5 y diferencia de preset](../../reviews/2026-10-21-first-ball-po-approvals.md#cierre-i6a-punto-de-control-etapa-5-y-autorización-i6b--2026-10-24). |

### 0.4 Cambios principales respecto de la v0.2 (resumen ejecutivo)

1. **Timing con habilidad real (§7.1).** El "imán" de 250 ms de la v0.2 se reemplaza por T3: un perdón de ±40 ms y, fuera de él, un contacto en el instante en que se suelta, con consecuencia física. El golpe al aire existe como **fallo de timing**. Hay un solo intento por recepción, tres niveles de ayuda visual y una variante suave como control. Se agrega una prueba de que anticiparse no es una estrategia dominante [D19–D22].
2. **Volea decidida por el jugador (§7.3, §9).** V2 + V3 como hipótesis y V1 como control. El planificador nunca volea solo. Se muestran solo las oportunidades alcanzables y relevantes. W cambia las oportunidades, y dejar pasar la pelota no garantiza el golpe siguiente. Se agrega el principio D24 [D23, D24].
3. **Anexo C en tres capas**, con condiciones de validez para cada relación. Se agrega la clase de criterios §17.F, que se revisa pero no es universal [D25].
4. **Modelo de error E-C (§7.6).** Una capa causal con vectores de efecto por causa y una capa aleatoria apagada hasta First Fun. El timing actúa solo a través de la geometría del contacto. Se corrige la contradicción de la v0.2 sobre qué filas de la tabla de error se aplican antes de su validación [D26].
5. **Física (§8.2).** Se elimina "fidelidad garantizada por construcción". Se declaran las condiciones en que la predicción y la simulación coinciden y se agrega una invariante bit a bit. La evaluación pasa a ser **secuencial**, de lo más simple a lo más complejo, sin construir tres motores [D27].
6. **Líneas y decisiones arbitrales (nueva §8.7).** Huella configurable, geometría explícita de las líneas, consecuencias **por línea y por fase** (saque o rally), marca de cantado, tabla de adaptaciones arcade y verificación WSF previa a la implementación [D28].
7. **Panel (§13).** Presets locales desde First Ball y controles nuevos para timing, volea, error y líneas [D29].
8. **Criterios (§17).** Invariantes nuevas: el planificador nunca volea solo, el timing no se penaliza dos veces, la predicción coincide bit a bit y los bordes de las líneas se prueban. Hipótesis H3 y H6 reformuladas, y H11 y H12 nuevas.
9. **Pendientes (§18).** Se marcan como resueltos P1, P6, P9, P10 y P11 y se agregan aclaraciones menores con valor por defecto. Se separan los **bloqueantes reales**.

El anexo E detalla los cambios sección por sección. Los cambios de la v0.1 a la v0.2 se conservan en el mismo anexo.

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

1. **La causa del error se ve y se puede medir.** Cada error se descompone en su **capa causal**, fuente por fuente, y su **capa aleatoria** (§7.6). El panel muestra cuánto aportó cada una [D26].
2. **El riesgo se ve antes de pegar**, como una *estimación* condicionada al timing y la potencia que todavía no se conocen (F15).
3. **Un solo vocabulario de gestos.** La volea y los golpes futuros usan el mismo gesto (§7.3).
4. **Las mismas reglas para la IA y el humano:** física, contacto, error, movimiento y cansancio. Además, la IA solo usa información observable (§11.2).
5. **El estilo se reconoce por cómo juega.**
6. **El cansancio es drama, no castigo.** Actúa solo a través de la cadena causal, sin penalizar dos veces lo mismo (§9.4).
7. **La asistencia nunca regala pelotas.** El movimiento automático llega solo a donde la cinemática del personaje lo permite (§9.1).
8. **Primero aislar y después combinar.** Cada sistema que cambia el rally se introduce y se evalúa por separado (F02).
9. **Una sola autoridad por dato:** una sola simulación de la pelota, un solo cierre de punto y un solo reloj (§8).
10. **El movimiento automático asiste el desplazamiento, pero no reemplaza las decisiones tácticas** [APROBADO·D24]. El sistema lleva el cuerpo. El jugador decide **dónde espera**, **qué golpe pide**, **cuándo suelta** y, por lo tanto, **si volea o deja pasar** la pelota.
11. **Cada fallo tiene su categoría:** fallo de timing, error de ejecución, inalcanzable u omisión. El jugador ve de cuál se trata (§7.1.3) [D20].

---

## 5. Core gameplay loop

### 5.1 Loop del rally
1. El rival le pega a la pelota. Empieza tu fase de **Recepción** (§6.5).
2. El planificador calcula dónde y cuándo puedes pegarle a la pelota, y tu personaje corre hacia ese punto (§9.1). Si hay una oportunidad de volea alcanzable y relevante, se te muestra, pero **no se ejecuta sola** (§7.3).
3. Apoyas el dedo, apuntas en el frontis y defines la potencia según la variante de control (§6.2).
4. Sueltas el dedo: es tu **único intento** en esta recepción. Si sueltas cerca del momento ideal (±A), el contacto se asiste. Si te adelantas o te atrasas, la pelota se golpea donde esté en ese instante, con su consecuencia. Si te adelantas demasiado, le pegas al aire (§7.1).
5. El resultado depende de la geometría del contacto (timing), la posición, la dificultad situacional y la técnica, que forman la capa causal. Desde First Fun, y solo como experimento, se suma una variabilidad acotada (§7.6).
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

- **Dónde:** en el modo práctica, con un lanzador de pelotas. Energía fija, capa aleatoria apagada y **ayuda de timing completa**, para que la comparación aísle el control. Los niveles de ayuda se comparan después, con la variante ganadora (§7.1.5).
- **Lanzamientos estandarizados (6):** media altura de drive, media altura de revés, esquina trasera del lado del drive, esquina trasera del lado del revés, adelante del lado del drive y adelante del lado del revés.
- **Tareas:** la pantalla indica una zona del frontis y un nivel de potencia. Ejemplo: "drive paralelo, potencia fuerte".
- **Protocolo:** cada variante en bloques de 20 golpes × 3 rondas. El orden se alterna entre sesiones (A′-B-C y luego C-B-A′). Se juega con una mano, diestra y zurda.
- **Métricas registradas:**
  - error entre la mira deseada (la de la tarea) y la mira final, en cm sobre el frontis;
  - potencia involuntaria: |potencia final − potencia pedida|;
  - duración del gesto;
  - distribución de Δ de timing y categoría del contacto (asistido, adelantado, atrasado, al aire o tarde, §7.1.3);
  - tasa de cancelación;
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
| **Recepción** | El rival le pega a la pelota | Tu contacto, tu intento consumido sin contacto, el fin del punto o el fin de la ventana legal | Gesto de golpe. Con el intento consumido, los toques se ignoran hasta la siguiente fase. |
| **Recuperación** | Tu contacto | El rival le pega a la pelota | Toque = elegir la posición de espera (§9.3). Un arrastre se ignora. |
| **Fin de punto / Pausa** | Cierre del punto | Siguiente saque | Solo interfaz (saltar la pausa) |

**Reglas:**
1. **El propósito de cada toque se fija al apoyar el dedo** y no cambia aunque la fase cambie. Un toque que empezó en Recuperación nunca se convierte en golpe.
2. **Toque o arrastre:** un desplazamiento < 3 mm en menos de 250 ms es un toque. Los valores son [EXPERIMENTO] y se miden en **mm físicos**, no en píxeles.
3. **Un solo intento por recepción** [APROBADO·D19]. Soltar el dedo fuera del círculo de cancelación **siempre** ejecuta el swing y consume el intento, haya contacto o no (§7.1.3).
4. **Cancelar un golpe** [APROBADO·D19, concreción: PROPUESTA]: llevar el dedo de vuelta a un círculo de ~6 mm alrededor del punto de origen y soltar ahí. No se ejecuta nada y **no se consume el intento**: se puede iniciar otro gesto si queda tiempo. El costo es el tiempo perdido. *[PENDIENTE PO, N1: confirmar que la cancelación no consume el intento.]*
5. **Toque accidental en Recepción** [PROPUESTA]: un toque (regla 2) en esta fase no ejecuta swing ni consume el intento. Solo un gesto de golpe cuenta como intento. *[PENDIENTE PO, N2.]*
6. **Un solo dedo:** cuenta el primer toque. Los demás se ignoran, salvo el gesto oculto del panel en builds de desarrollo.
7. **Perder el foco** (notificación, bloqueo o app en segundo plano): el partido se pausa y el gesto activo se cancela sin consumir el intento.
8. **Bordes del sistema:** el área de juego excluye la zona del indicador de inicio y las áreas seguras. Se piden a iOS los gestos de borde diferidos.
9. **Lateralidad:**
   - La **mano con que se usa la interfaz** (espeja el HUD y la posición inicial de la mira) se configura aparte de la **mano dominante del personaje**.
   - Por defecto, el personaje es diestro y el drive y el revés tienen la misma calidad [PENDIENTE PO, §18].

### 6.6 Saque [PROPUESTA]
- Usa el mismo gesto. Al apoyar el dedo, el personaje lanza la pelota al aire. T\* es el instante en que la pelota llega a la altura ideal de impacto.
- Aplica el mismo contrato de contacto (§7.1, T3). Excepción: si el sacador **le pega al aire** o no suelta a tiempo, **se repite el lanzamiento sin falta**. Fallar el lanzamiento no debe costar un punto. **Es una adaptación arcade** (§8.7.5): en el reglamento, si la pelota toca el suelo sin haber sido golpeada es falta (WSF 2025, regla 5.7.1; verificado).
- Tope de 10 s para iniciar el saque [EXPERIMENTO].

---

## 7. Sistema de golpes

### 7.1 Contrato de contacto y timing (F03, FR1) — Prioridad 1

**Estado:** [HIPÓTESIS·D19] **T3, asistencia mínima con consecuencia física**, para First Shot. Reemplaza al contacto asistido con ventana de 250 ms de la v0.2. Los valores numéricos son [EXPERIMENTO] y se ajustan en el panel.

**Qué busca:** que soltar en el momento justo sea una **habilidad que se aprende**, que adelantarse o atrasarse tenga una consecuencia que se ve, y que anticiparse siempre no sea la mejor estrategia [D22].

#### 7.1.1 Definiciones

- **Ventana legal L = [t_in, t_out]:** instantes en que tu personaje *puede legalmente* pegarle a la pelota. La pelota tiene que haber tocado el frontis después del golpe del rival y no puede haber botado dos veces. Su altura tiene que estar entre `h_min` y `h_max` [EXPERIMENTO]. Sin volea, además, tiene que haber botado una vez. Con volea, se incluye el tramo antes del bote (§7.3).
- **Oportunidad de contacto:** un tramo continuo de L, antes o después del bote, en el que la pelota pasa al alcance del personaje. Puede haber una, dos (volea y bote) o ninguna (§7.3).
- **Plan:** para cada oportunidad, el punto P\* y el instante T\* de contacto ideal que elige el planificador (§9.1). El **anillo de timing** de esa oportunidad se cierra en T\*.
- **Tramo alcanzable K = [k_in, k_out]:** instantes en que la pelota está **legal y dentro del alcance real** del personaje (§9.1), según dónde está su cuerpo en cada instante. Se calcula con la trayectoria real, no con la predicha.
- **T\*_visible:** el valor de T\* que mostraba el anillo cuando el jugador soltó. **La asistencia se evalúa contra lo que vio el jugador.**
- **r:** instante en que se soltó el dedo, según la marca de tiempo del evento táctil convertida al reloj de la simulación (§8.4).
- **Δ = r − T\*_visible.**
- **Tolerancia de asistencia A** [APROBADO·D19 como valor de partida; EXPERIMENTO]: **±40 ms**, en tiempo real percibido. Es el perdón por la latencia y la imprecisión del dedo. Para la volea se usa `A_volea = 0,8·A` [EXPERIMENTO].
- **Geometría del contacto:** posición de la pelota respecto del cuerpo en el contacto. Se mide por la distancia, a lo largo de la dirección del golpe, entre la pelota y el punto ideal de contacto del cuerpo: **adelantado** si la pelota está por delante y **atrasado** si está a la altura del cuerpo o detrás. **Es la única vía por la que el timing afecta al golpe** (§7.6) [D26].

#### 7.1.2 Estados de la orden de golpe

`Inactivo → Apuntando → Ejecutado (swing)`

El swing termina **con contacto** o **sin contacto** (al aire o tarde). Además, hay dos salidas sin swing: `Cancelado` y `Sin orden`.

- **Apuntando:** dedo apoyado en la fase de Recepción. El golpe está **preparado** si hay al menos una oportunidad alcanzable, es decir, si el anillo es visible.
- **Ejecutado:** se soltó fuera del círculo de cancelación. **Consume el intento de la recepción** [D19]. Desde aquí ya no se puede cambiar ni cancelar.
- La palabra "Comprometido" de la v0.2 desaparece: con T3, soltar ya *es* el swing.

#### 7.1.3 Casos

| # | Situación | Resultado | Nota visible | Categoría |
|---|---|---|---|---|
| 1 | \|Δ\| ≤ A, con la pelota en K | **Contacto asistido.** Si r ≤ T\*, el contacto ocurre en T\* y en P\*. Si r > T\*, ocurre en r, con la pelota donde esté, y el brazo se ajusta. El vector causal de timing es **cero**. | Perfecto | — |
| 2 | Δ < −A, con la pelota en K en el instante r | Contacto **en r**, con la pelota donde esté: **adelantado**. El vector causal de timing crece con la distancia geométrica (§7.6.2). | Adelantado | — |
| 3 | Δ > A, con la pelota en K en el instante r | Contacto **en r**: **atrasado**, con su vector causal. | Atrasado | — |
| 4 | Suelta antes de que la pelota entre a K (r < k_in) | **Golpe al aire.** Hay swing pero no contacto, y la pelota sigue su curso. **Intento consumido.** | ¡Al aire! | **Fallo de timing** (temprano) [D20] |
| 5 | Suelta después de que la pelota salió de K (r > k_out), con la pelota todavía en juego | Swing sin contacto. Intento consumido. | ¡Tarde! | **Fallo de timing** (tarde) |
| 6 | No suelta, o nunca apoya el dedo, durante toda L | **Sin orden.** | — | Omisión |
| 7 | No hay ninguna oportunidad alcanzable | El anillo no aparece y se muestra un indicador de "no llega". El personaje hace su máximo esfuerzo visual. Si suelta, el swing es sin contacto. | No llega | **Inalcanzable.** Cuenta como mérito del rival. |
| 8 | El plan cambia **antes** de soltar | El anillo se actualiza a la vista y no hay penalización. | — | — |
| 9 | Después de soltar, la pelota real se aparta de la predicción | El contacto se evalúa con la pelota **real** en r (o en T\* si \|Δ\| ≤ A). Si deja de estar en K, cuenta como **Inalcanzable** y se registra como *falla del sistema*. | No llega | Del sistema. Debe ser ≈ 0 (§17.A). |
| 10 | Cancela (vuelve al origen y suelta) | **Cancelado.** Sin swing. **No consume el intento** [PENDIENTE PO, N1]. | — | Ninguna |
| 11 | Ya consumió el intento en esta recepción | Los toques siguientes se ignoran hasta la siguiente fase. **No hay segundo swing.** | — | — |
| 12 | Golpe al marco o pelota fallada **con la pelota en K** | Solo si se cumplen **cansancio ≥ umbral y calidad de posición < umbral** (§9.4). **Nunca con buena posición.** Solo existe desde First Fun. | Marco | **Error de ejecución** |
| 13 | La pelota ya botó dos veces o no es legal | Nunca hay contacto. | — | — |
| 14 | Hay dos oportunidades (volea y bote) | Ver §7.3. El contrato se aplica a la oportunidad **en cuyo tramo cae r**. | — | — |

**Las cuatro categorías de fallo son distintas, y el jugador ve cuál fue:**
- **Fallo de timing:** casos 4 y 5. Es una decisión de la mano del jugador.
- **Error de ejecución:** caso 12. Solo existe desde First Fun, con cansancio y mala posición.
- **Inalcanzable:** casos 7 y 9. Es mérito del rival o, en el caso 9, una falla del sistema.
- **Omisión:** caso 6.

La garantía "nunca fallar con buena posición" (§17.A.6) se refiere **solo al error de ejecución**. Con T3 se puede fallar la pelota estando bien ubicado, pero solo por **fallo de timing** [APROBADO·D20].

**La ventana útil escala sola con la presión.** Con una pelota rápida, K dura menos milisegundos y hay menos margen. Con una lenta, el margen es mayor. No se escalan ventanas a mano.

#### 7.1.4 Variantes y por qué T3

| Variante | Descripción | Estado |
|---|---|---|
| **T3** | La de este contrato | [HIPÓTESIS·D19] |
| **T3-suave** | Igual que T3, salvo el caso 4: soltar antes de k_in produce un contacto en **k_in**, el primer punto alcanzable, con el vector "adelantado" máximo, en lugar de un golpe al aire. Nunca se falla la pelota por timing, pero el golpe sale muy malo. | **Control** [APROBADO·D19]. Se activa en el panel si T3 frustra. |
| Contacto asistido de la v0.2 (ventana de 250/150 ms) | Imán hacia T\*, con reintento si se soltaba demasiado temprano | **Retirada:** premiaba soltar temprano (FR1) |
| Contacto físico puro (T2) | Sin perdón (A = 0) | Se obtiene poniendo A = 0 en el panel. No es una variante aparte. |
| Swing programado | Soltar inicia una preparación de duración fija | Alternativa histórica (v0.2). Solo se retoma si T3 y T3-suave fallan. |

T3 encaja con el movimiento automático: el cuerpo está donde el planificador lo lleva y el jugador solo controla **cuándo**. La causa del error es medible (Δ y la geometría del contacto) y cada caso se puede probar por separado.

#### 7.1.5 Retroalimentación y ayuda de timing

- **Niveles de ayuda visual** [APROBADO·D21; detalles: EXPERIMENTO], seleccionables en el panel y, en el futuro, por nivel del jugador:
  - **Completa:** el anillo se cierra en T\*.
  - **Parcial:** el anillo desaparece en los últimos ~150 ms y hay que *leer* la pelota.
  - **Sin anillo:** solo la pelota y su sombra.
- **Nota** (Perfecto, Adelantado, Atrasado, ¡Al aire!, ¡Tarde!, No llega): se muestra brevemente y se puede desactivar. Distingue de forma visible la categoría del fallo (§7.1.3).
- El sonido y la vibración háptica se disparan en el frame del contacto. El golpe al aire tiene su propio sonido (un "swish") y no vibra (§12.4).
- **[EXPERIMENTO]** Una compensación de latencia del toque, configurable en ms y calibrable en el panel. Se calibra **antes** de medir H3, porque si no un desfase sistemático del dispositivo se confundiría con la habilidad.
- **Validación [D22]:** H3 (el timing mejora con la práctica) y H3b (anticiparse no domina), en §17.B.

### 7.2 Catálogo: intenciones, condiciones de contacto y resultados (F26)

| Clase | Elementos | Estado |
|---|---|---|
| **Intención** (lo que el jugador pide) | Drive paralelo, cruzado, drop | First Shot [APROBADO·D14] |
| | Globo (alto y suave), boast (mira en la pared lateral), **kill** (región bajo y fuerte de la intención de drive) | [FUTURO], milestone Golpes ampliados |
| **Condición de contacto** (cuándo y dónde se le pega) | Tras el bote | First Shot |
| | **Volea** (antes del bote) | Experimento en First Rally [APROBADO·D14]. El jugador la decide: V2 + V3 [HIPÓTESIS·D23] |
| | Tras la pared trasera | Emergente. Se permite si la pelota sigue legal |
| **Resultado emergente** (lo que pasa) | **Nick**, pelota suelta, tin, out, ganador | No se programan como golpes. Si un nick ocurre naturalmente por la física, se respeta. La fidelidad del nick se trabaja en el futuro. |

**Bandas de altura** [EXPERIMENTO]: baja (tin a ~0,9 m), media (~0,9–2,5 m) y alta (> 2,5 m). **Solo sirven para etiquetar** el golpe, elegir la animación y alimentar la IA. **No cambian la física:** la trayectoria es continua en el punto apuntado y la potencia (F12).

Dirección: el paralelo y el cruzado son extremos de un continuo. La pelota puede ir al medio si se apunta ahí o si el error la manda ahí.

### 7.3 Volea como condición de contacto (Prioridad 4, FR2)

**Estado:** [HIPÓTESIS·D23] **V2 + V3**: el jugador decide la volea con su timing y su posición de espera. **V1** (decide el planificador) queda como **control experimental**. La prueba se hace en First Rally [APROBADO·D14].

**Principio [APROBADO·D24]:** el movimiento automático lleva el cuerpo, pero **la decisión de cortar la pelota o dejarla pasar es del jugador**. El planificador **nunca ejecuta una volea por su cuenta**, aunque la considere conveniente (invariante §17.A).

#### 7.3.1 Reglas generales
- **No agrega gestos.** Es el mismo gesto y la misma intención (drive, cruzado o drop) con contacto antes del bote.
- **Restricciones físicas de la volea** [EXPERIMENTO]:
  - la pelota pasa por el alcance entre `h_volea_min` y `h_volea_max` (valor de partida: 0,6–1,9 m);
  - la tolerancia de asistencia es menor: `A_volea = 0,8·A` (§7.1.1).
- **La dificultad de la volea no se programa aparte.** Sale del modelo de error (§7.6): las voleas pegadas a la pared, a una altura incómoda o con mala posición corporal tienen vectores causales mayores [D23]. Una volea **técnicamente posible** puede ser **tácticamente inconveniente**, y el jugador lo aprende por sus consecuencias.

#### 7.3.2 Oportunidades visibles [APROBADO·D23; concreción: PROPUESTA]
- Se muestra un anillo **solo por cada oportunidad físicamente alcanzable y relevante**. No siempre hay dos anillos.
- **Alcanzable:** la oportunidad tiene un tramo K no vacío (§7.1.1) con la cinemática real del personaje.
- **Relevante** [EXPERIMENTO, PENDIENTE PO N3]: la calidad de llegada estimada es q ≥ `q_min_rel`, con un valor de partida **bajo** (0,2), para que el sistema **no** decida por el jugador si una volea "conviene". Solo se ocultan las oportunidades en las que únicamente se podría tocar la pelota sin intención: rozarla en el límite del alcance o con la pelota casi en el suelo.
- **Dos anillos de colores distintos**, uno de volea y otro de bote, cuando las dos oportunidades son visibles. Las ventanas de los dos anillos nunca se superponen.
- **Separación mínima** [EXPERIMENTO: 250 ms]: si entre el final del tramo de volea y el inicio del tramo de bote queda menos que ese umbral, se muestra **solo la oportunidad de bote**, salvo que el bote sea inalcanzable. Así se evita que soltar temprano para el golpe tras el bote se convierta en una volea por accidente. **Es una regla fija de desambiguación, no una preferencia táctica del planificador.**
- **La mira previa** y el indicador de riesgo (§7.6.5) se refieren a la oportunidad cuyo anillo está más próximo a cerrarse.

#### 7.3.3 Movimiento cuando hay dos oportunidades [PROPUESTA; PENDIENTE PO N4]

El cuerpo no puede ir hacia dos puntos a la vez. Se proponen dos modos, intercambiables en el panel:

| Modo | Cómo se mueve el cuerpo | Efecto táctico | Riesgo |
|---|---|---|---|
| **M1: guía por bote (por defecto)** | El planificador siempre lleva el cuerpo hacia el **plan de bote**. La volea aparece cuando, **en ese camino**, la pelota pasa por el alcance antes de botar. | El planificador nunca apuesta por la volea. El jugador crea las voleas con la **posición de espera** (V3) y las elige con el **timing** (V2). | Habrá menos voleas si W está lejos de la trayectoria. |
| **M2: guía por volea** | Mientras la volea está abierta, el cuerpo va hacia el **plan de volea**. Si el jugador no suelta, al cerrarse va hacia el plan de bote, que puede haber quedado con poca holgura o ser inalcanzable. | Más voleas. Dejar pasar la pelota tiene un costo inmediato. | El sistema empuja hacia la volea, lo que acerca el modelo a V1. |

**Por qué M1 por defecto:** respeta D24 de forma más estricta, porque el movimiento automático nunca toma una posición que favorezca una decisión táctica que el jugador no tomó. En los dos modos se cumple que **dejar pasar la pelota no garantiza el golpe posterior** [D23]: si W estaba adelantada, el plan de bote puede quedar con poca holgura o ser inalcanzable.

#### 7.3.4 Posición de espera y volea (V3)
- Una W adelantada, cerca de la T o por delante, hace que más trayectorias pasen por el alcance antes del bote, y por lo tanto genera **más oportunidades de volea**. A cambio, cubre peor las pelotas largas.
- Una W retrasada casi elimina la volea y da más tiempo para el bote.
- Así se conectan la decisión previa (dónde espero) y la inmediata (corto o dejo pasar) (§9.3).

#### 7.3.5 Experimento en First Rally
- **Condiciones:** sin volea / V1 (control) / V2 + V3 con M1 / V2 + V3 con M2 (opcional).
- **Qué se mide:**
  - oportunidades de volea mostradas y elegidas, y su contexto (W, altura, distancia a la pared);
  - holgura del rival tras una volea frente a un golpe tras el bote;
  - resultado de dejar pasar: holgura del plan de bote y puntos perdidos por no llegar;
  - ocupación de la T;
  - forma en que terminan los rallies;
  - valoración del PO: ¿la decisión de volear se siente suya?
- Si la volea genera un dominio del centro sin contrajuego, se evalúa adelantar una salida alta simple, tipo globo (F06).
- **Hipótesis:** H6 y H6b (§17.B).

### 7.4 Resolución física del golpe: shot solver (F12) [PROPUESTA]
1. **La potencia es una velocidad de salida concreta:** `v = v_min(intención) + p·(v_max(personaje, energía) − v_min)` en m/s, con `p ∈ [0; 1]`.
2. **Política "se respeta el objetivo":** dados el contacto C, el objetivo P en el frontis y la velocidad v, se busca la dirección de salida que lleva la pelota de C a P. Entre las dos soluciones balísticas se elige **el arco más bajo**.
3. **Objetivo infactible con esa v** (por ejemplo, demasiado alto para una pelota suave, o un arco que bota antes del frontis): se sube **la velocidad mínima necesaria** hasta un tope. Si ni así hay solución, se baja el objetivo hasta el punto factible más cercano.
4. **Se distinguen tres cosas y las tres se registran:**
   - la **intención** (P y p pedidos);
   - el **ajuste asistido** (la corrección del paso 3);
   - la **salida física** (la velocidad final).
5. **El ajuste se ve antes de soltar:** la mira se desplaza de forma visible al punto factible, o el medidor de potencia indica la corrección.
6. **Orden:** primero se aplica el error (§7.6) sobre P y v, después se resuelve. **Nunca se corrige la salida después** para forzar un acierto.
7. La física es continua. Las bandas no introducen saltos (§7.2).

### 7.5 El drop y el "corte" (F13): diferido a First Shot
- First Shot empieza con **superficies y pelota con propiedades consistentes**. El drop se logra con poca velocidad y un objetivo bajo.
- Si el PO juzga que el drop no "muere" lo suficiente, se agrega un **estado explícito de la pelota** (*cortada*), con menor restitución en el siguiente contacto con el frontis y el suelo. Ese estado lo conocen el predictor, el solver y la IA, y se documenta como una simplificación arcade, no como física.

### 7.6 Calidad del golpe y modelo de error (F14, F15, FR4)

**Estado:** [HIPÓTESIS·D26] **Modelo E-C de dos capas.** Separa la **consecuencia causal** (lo que el jugador puede entender y corregir) de la **variabilidad aleatoria** (la parte que no controla). Reemplaza la fórmula aditiva de la v0.2, en la que la misma `carga` escalaba a la vez el error determinista y la dispersión.

#### 7.6.1 Las dos capas

| | Capa 1: causal (determinista) | Capa 2: aleatoria (variabilidad) |
|---|---|---|
| Qué representa | Consecuencias comprensibles de **cómo** se le pegó | Diferencias entre golpes iguales |
| De qué depende | Geometría del contacto (timing), posición corporal, dificultad situacional, técnica | **Solo** de la dificultad *inherente* del golpe pedido y, como experimento, de la Consistencia |
| De qué **no** depende | Del azar | Del timing, la posición, la dificultad situacional ni el cansancio |
| Cuándo está activa | Desde First Shot | **Apagada** en First Shot, First Rally y First Match. Experimento en First Fun. |
| Reproducible | Siempre | Sí: semilla registrada |

**Dos dificultades que no se mezclan:**
- **Situacional:** depende del contacto. Pelota pegada a la pared, pelota baja o una altura incómoda. Es causal.
- **Inherente:** depende de lo que se pide. Margen pedido sobre el tin, cercanía del objetivo a una línea, o intención (un drop es más exigente que un drive a media altura). Solo puede afectar a la variabilidad.

#### 7.6.2 Capa 1: vectores causales [APROBADO·D26; fórmula: PROPUESTA; valores: EXPERIMENTO]

Cada causa produce un **vector de efecto con signo**, expresado **relativo al cuerpo**: `(Δaltura en el frontis, Δlateral hacia la pared propia o hacia el centro, Δvelocidad)`. Después se proyecta a la cancha, así que el modelo es simétrico al espejar el lado.

| Causa | Variable | Cómo actúa | Efecto (filas de §7.6.3) |
|---|---|---|---|
| **Timing** | Geometría del contacto (§7.1.1), fuera de ±A | **Solo** a través de la posición de la pelota respecto del cuerpo en el contacto. **No existe un término de error de timing aparte** (sin penalización duplicada) [D26]. | Línea base: pelota "más suelta", más alta y lenta, sin sesgo lateral. Con E1/E2 validadas, se agrega el efecto lateral. |
| **Posición corporal** | Calidad de llegada q, estocada, distancia del cuerpo a la pelota (§9.1) | Contacto estirado o atorado | E3, E5 |
| **Dificultad situacional** | Distancia de la pelota a la pared lateral, altura de la pelota en el contacto | Menos ángulo disponible, o una trayectoria forzada | E4, E6, E8 |
| **Técnica** | Atributo (§10.2) | **Reduce la magnitud** de los vectores de posición y de dificultad situacional. No reduce el vector de timing, que sale de la geometría [PROPUESTA]. | Escala de los vectores |
| **Brazo** (First Fun, experimento 2) | Energía < umbral | Baja la `v_max` del golpe y habilita el caso 12 de §7.1 | §9.4 |

- **Composición:** `e_causal = clamp( Σ (1 − t·k_tec,i) · vᵢ ; tope por componente )`, donde `t` es la técnica normalizada y `k_tec,i` indica cuánto atenúa la técnica la causa i (0 para el timing). Los pesos y los topes son configurables.
- **El cansancio no es una fuente directa.** Afecta a la posición y al timing a través de la cadena causal [APROBADO·D06], así que no se cuenta dos veces (F16).
- **Orden en el solver (§7.4):** e_causal (y después e_aleatorio) se aplica sobre el objetivo P y la velocidad v **antes** de resolver la trayectoria. Nunca se corrige la salida después.
- **Registro:** cada vector se guarda por separado, con su causa.

#### 7.6.3 Tabla de casos de error relativos al cuerpo [PENDIENTE PO: P3]

Los casos se expresan relativos al cuerpo (contacto adelantado o atrasado respecto del pie delantero, estirado o atorado, del lado del drive o del revés) y se proyectan después a la cancha. **El PO marca cada fila** como correcta, incorrecta o con matiz.

| # | Causa (relativa al cuerpo) | Efecto candidato | Tipo | Validación del PO |
|---|---|---|---|---|
| E1 | Contacto **adelantado** (pelota por delante del pie delantero) | La pelota cruza más de lo pedido, hacia el centro o el lado contrario | Direccional | ☐ |
| E2 | Contacto **atrasado** (pelota a la altura del cuerpo o detrás) | La pelota se va hacia **la pared lateral propia**. Más paralela, con riesgo de tocar la lateral antes del frontis | Direccional | ☐ |
| E3 | **Estirado** (contacto al límite del alcance, en estocada) | Menos potencia y pelota más alta: **corta y suelta** | No direccional | ☐ |
| E4 | Pelota **baja** (bajo la rodilla) con contacto atrasado | Trayectoria más baja: **riesgo de tin** | Direccional (vertical) | ☐ |
| E5 | **Atorado** (pelota demasiado cerca del cuerpo) | Menos potencia y pelota más alta | No direccional | ☐ |
| E6 | Pelota **pegada a la pared lateral** | Menos ángulo disponible: el cruzado sale corto o al medio | Direccional | ☐ |
| E7 | Timing perfecto con buena posición | Sin vector causal | — | ☐ |
| E8 | **Volea a una altura incómoda** (por encima del hombro, o a la altura de la cintura pegada al cuerpo) [nuevo, D23] | Menos potencia y pelota más alta | No direccional | ☐ |

**Corrección de una contradicción de la v0.2.** La v0.2 aplicaba E3, E5 y E7 desde First Shot, pero también decía que "solo las filas validadas entran al modelo, y como pronto en First Fun". En la v0.3 la regla es esta:
- **Desde First Shot** [EXPERIMENTO] se aplican los efectos **no direccionales**: E3, E5, E7, E8 y la versión simétrica del timing ("más suelta"). No imponen ninguna dirección lateral discutible, y el PO pidió que la posición corporal genere consecuencias comprensibles [D26]. Sus magnitudes se ajustan en el panel.
- **Los efectos direccionales** (E1, E2, E4 y E6) entran **solo cuando el PO valida la fila**, en el milestone en que se haga. First Fun es la fecha más tardía, no la más temprana.
- Las filas que el PO marque como incorrectas no se usan, ni siquiera en su versión simétrica.

#### 7.6.4 Capa 2: variabilidad aleatoria [APROBADO·D26; valores: EXPERIMENTO]
- **Apagada** hasta First Fun. En las primeras validaciones, un mismo golpe con la misma entrada da siempre el mismo resultado.
- **Experimento en First Fun:** una variabilidad **pequeña, acotada y reproducible**:
  - σ vertical y σ lateral, **anisotrópicas**: el drop tolera de forma distinta la altura y el lateral;
  - `σ = σ_base(dificultad inherente) · f(Consistencia)`, con `f` en un rango acotado [EXPERIMENTO: 0,75–1,25];
  - distribución normal truncada a ±2σ, con un tope absoluto `σ_max` en cm sobre el frontis;
  - **semilla registrada** por rally (§13.4).
- **Brazos del experimento** (H9 y H9b, §17.B): (a) apagada, (b) solo dificultad inherente, (c) dificultad inherente + Consistencia. **No se da por hecho que el estado mental sea la causa dominante del error** [D26]: el brazo (c) solo se mantiene si el PO percibe que mejora la experiencia frente al (b).
- La variabilidad **nunca** produce un fallo inevitable con buena posición. Su tope impide que un golpe con margen razonable termine en el tin solo por azar. La calibración se revisa con la relación R6 del anexo C.

#### 7.6.5 Visualización
- **Mira previa:**
  - muestra la zona de variabilidad (capa 2) cuando está activa, es decir, el riesgo propio del golpe elegido;
  - se pone roja si el objetivo está en el tin o fuera, con la misma huella que la regla de líneas (§8.7);
  - [EXPERIMENTO, desactivado por defecto] puede mostrar además el **desplazamiento causal previsto** por la posición y la dificultad situacional, que se conocen antes de soltar. El timing no se conoce todavía, así que no se incluye.
- **Superposición de depuración:** las dos capas por separado, causa por causa. Ejemplo: "causal: +18 cm de altura por estirado; aleatorio: −3 cm". Así se responde "¿fue mi culpa o mala suerte?" mirando un solo golpe.
- **Sin el panel:** la nota de timing (§7.1.5) y la animación del contacto (estirado, atorado o adelantado) tienen que bastar para que el PO explique el fallo (H4).

---

## 8. Física y reglas

### 8.1 Cancha (medidas WSF)
**Verificado con la WSF el 2026-10-20 (§8.7.1):** 9,75 × 6,40 m. Out del frontis a 4,57 m, línea de saque a 1,78 m y out trasero a 2,13 m, **medidas hasta el borde inferior de la línea**. Short line con su borde más cercano a la pared trasera a 4,26 m de ella, es decir, entre 5,44 y 5,49 m del frontis. Cuadros de saque de 1,60 m de lado interior. Líneas de 50 mm. **Tin:** borde superior a 0,48 m, el estándar WSF [P8 resuelto por verificación]. La PSA prefiere 0,43 m, que queda como opción del panel.
**Pelota:** 40 mm de diámetro, por lo tanto **radio físico de 20 mm** (F10).

### 8.2 Elección de la simulación de la pelota (F09, FR5) — Prioridad 5

**Estado para First Ball:** **P2-Analytic elegido por el PO el 2026-10-23** [APROBADO·D30], según §8.2.3. P1 falla C3, C10, C11 y C17 y se conserva como respaldo configurable, sin usarse por defecto. [Decisión del gate](../../reviews/2026-10-23-first-ball-physics-backend-decision.md); [autorización de este registro T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24). D15/D27 describen el proceso previo a esta decisión.

**Principio invariante:** **una sola autoridad sobre el estado de la pelota.** Nunca hay dos sistemas que corrijan alternadamente la misma pelota.

| Alternativa | Descripción |
|---|---|
| **P1: Rigidbody/PhysX** | La pelota es un Rigidbody con CCD. La predicción se hace en una **escena física separada** con simulación manual de paso fijo. |
| **P2: Simulación propia** | Integrador propio, con impacto esfera-plano analítico contra los planos de la cancha. Predicción y juego comparten el mismo código. Unity solo presenta. |
| **P3: Híbrida** | La autoridad sobre la pelota es propia (como P2), y se usan consultas geométricas de Unity (raycast o sweeps) para la detección. Unity presenta y mueve personajes. |

#### 8.2.1 Cuándo coinciden la predicción y la simulación (corrección de FR5)

La v0.2 afirmaba que con P2 la fidelidad estaba "garantizada por construcción". **Esa afirmación se retira** [D27]. Compartir el código es necesario, pero no basta. La predicción coincide con la simulación solo si se cumplen **todas** estas condiciones, con cualquier alternativa:

1. **Mismo estado inicial exacto:** la predicción parte del estado de la simulación, nunca de una posición interpolada para el render.
2. **Mismo paso fijo y mismo orden de integración.** Un predictor que usa pasos más grandes o fórmulas analíticas para ahorrar costo ya no es idéntico.
3. **Mismos parámetros:** un cambio del panel invalida la predicción. Por eso los parámetros físicos se aplican solo al empezar el rally (§13.4), y esto pasa a ser invariante.
4. **Sin influencia externa entre la predicción y el evento:** la predicción solo vale hasta el siguiente contacto con raqueta.
5. **Mismo binario y mismo dispositivo.** No se promete igualdad entre dispositivos ni entre builds distintos, porque los floats y las optimizaciones del compilador pueden diferir.

Con P2, la igualdad es **alcanzable y verificable a bajo costo** si se cumplen las condiciones 1–5. Con P1, depende de que la escena de predicción reproduzca exactamente la escena de juego, y **se mide igual**. Ninguna alternativa se da por buena sin la medición.

#### 8.2.2 Criterios de selección

| Criterio | Cómo se mide | Tipo |
|---|---|---|
| Fidelidad de la predicción | Si el predictor usa el mismo paso y el mismo código que la simulación: **igualdad bit a bit** hasta el siguiente contacto de raqueta en S1–S8. Si usa un predictor más grueso: desvío en el segundo bote, después de ≥ 3 contactos. | **Invariante** (bit a bit), o umbral [EXPERIMENTO]: ≤ 1 cm y ≤ 2 ms |
| Robustez de colisiones | Ninguna pelota atraviesa superficies en 10 000 lanzamientos aleatorios | **Invariante** |
| Estabilidad en esquinas | La rapidez después de un contacto doble nunca supera a la de antes. No hay vibración. | **Invariante** |
| Rendimiento | Costo de predecir una trayectoria completa hasta el segundo bote, en el iPhone del PO | Umbral [EXPERIMENTO]: ≤ 1 ms por predicción |
| Consistencia | Mismo resultado al repetir con el mismo estado inicial, en el mismo binario y el mismo dispositivo | **Invariante** (no se exige determinismo entre plataformas) |
| Facilidad de prueba | ¿Los escenarios corren en EditMode sin escena? | Preferencia |
| Mantenibilidad | Líneas de código y número de conceptos propios a mantener | Juicio |

**Escenarios de prueba (iguales para todas las alternativas):**
- S1: drive paralelo rápido (~28 m/s reales) con 3 rebotes.
- S2: drop bajo, 5 cm sobre el tin.
- S3: rebote rasante en la pared lateral (< 10°).
- S4: unión entre pared lateral y suelo (tipo nick).
- S5: frontis y lateral cerca de la esquina frontal.
- S6: dos contactos dentro de un mismo paso (esquina trasera).
- S7: pelota que sale de la pared trasera.
- S8: 10 000 lanzamientos aleatorios.
- S9 (nuevo): pelota que toca una línea de out, una de tin y una de saque justo en el borde (§8.7.6).

#### 8.2.3 Regla de decisión y forma de la prueba [APROBADO·D27; orden: PROPUESTA]
- Se elige **la alternativa más simple que cumpla todos los invariantes y los umbrales medidos**.
- **No se construyen tres motores completos.** La prueba usa solo una pelota, una caja con las medidas de la cancha y los escenarios S1–S9.
- **Evaluación secuencial**, de menos a más código propio:
  1. **P1** primero, porque es la que menos código propio requiere. Si cumple todo, se elige y la prueba termina.
  2. **P2** solo si P1 falla un criterio concreto. Se registra cuál.
  3. **P3** solo si aparece una necesidad que P2 no cubre con planos analíticos, como geometría no alineada con los ejes. Hoy no se prevé.
- **Inclinación de Claude (no aprobada, no vinculante):** es probable que P1 tenga dificultades con la igualdad entre la escena de predicción y la de juego, y que P2 termine siendo necesaria. La regla manda medir antes de suponerlo.

### 8.3 Requisitos de colisión, válidos para cualquier alternativa (F10)
1. Se detecta el **primer impacto dentro del paso**, no un solapamiento al final del paso.
2. Se consume el **tiempo restante del paso** después del impacto, con un tope de N impactos por paso [EXPERIMENTO: 4].
3. **Contactos simultáneos** (dentro de un ε temporal) en esquinas: se resuelven juntos, reflejando cada componente normal.
4. Cada superficie tiene su restitución y su fricción tangencial. Sin spin en el MVP.
5. **Las líneas se evalúan por la huella de contacto de la esfera, no por su centro:** la huella es configurable, con ~10 mm de radio [EXPERIMENTO; la v0.2 usaba ~20 mm]. Su consecuencia depende de la línea y de la fase (§8.7) [D28].
6. Paso fijo [EXPERIMENTO: 120 Hz para la pelota], independiente del render.
7. Cada contacto con una superficie emite un **evento** con la superficie, el punto de impacto, la huella y el instante. Las reglas (§8.5, §8.7) consumen esos eventos y nunca consultan la física directamente.

**Aclaración D-4 [APROBADO·D31]:** §8.3.1–8.3.3 y §8.3.7 se miden e informan mediante C12–C15, **sin umbral numérico nuevo**. Con P1, el tope de impactos es una propiedad medida, no configurable: `physics.maxImpactsPerStep` no es operativo. Con **P2 sí es operativo**: al alcanzarlo se descarta el tiempo restante, se conserva el último impacto con velocidad reflejada y el reloj avanza un paso; nunca se integra sin detección. Los simultáneos forman un instante atómico y el contador de topes alcanzados se informa en C14. [Decisión D-4](../../reviews/2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación), [adenda r2 aprobada](../../reviews/2026-10-23-first-ball-physics-decision.md), [evidencia P2](../../reviews/2026-10-23-first-ball-physics-spike-P2.md) y [autorización T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24).

### 8.4 Relojes, ritmo y velocidad (F11) [PROPUESTA]
- **Un solo reloj de simulación** gobierna la pelota, los personajes, la energía y las ventanas de contacto.
- **Tres controles separados**, nunca combinados sin querer:
  1. **Ritmo global τ:** escala todo el reloj de simulación. La forma de las trayectorias se mantiene igual a la real y el PO puede validarlas. Valor de partida τ ≈ 0,65 [EXPERIMENTO; equivale al "60–70 %" de D09].
  2. **Escala de lanzamiento λ:** multiplica las velocidades de salida. Con la gravedad igual, **cambia la forma** de las trayectorias. Partida: 1,0.
  3. **Cámara lenta de depuración:** solo existe en el panel y marca las métricas de la sesión como "no válidas".
- **[PROPUESTA] Empezar ajustando solo τ.** λ se toca únicamente si el PO pide trayectorias distintas a las reales.
- **Input:** `t_sim(evento) = t_sim(inicio del frame) + (t_evento − t_real(inicio del frame))·τ`. Se usan las marcas de tiempo del evento del Input System, no el frame en que se procesa.
- **La tolerancia A y los umbrales de timing se definen en tiempo real percibido** (lo que siente la mano) y se convierten a tiempo de simulación con τ. La IA usa los mismos valores convertidos.

### 8.5 Reglas por fase (F24)
Basadas en las [reglas WSF](https://worldsquashofficiating.com/rules-of-squash/), con adaptaciones explícitas.

**Saque:**
- Se saca desde un cuadro de saque. **[Adaptación]** El personaje se coloca solo y no hay foot fault.
- La pelota tiene que ir **directo al frontis**, sin tocar antes otra pared, entre la línea de saque y la de out, sin tocar ninguna de las dos (§8.7.4).
- El primer bote (si el receptor no volea) cae en el **cuarto trasero opuesto**, sin tocar sus líneas (§8.7.4). El receptor puede volear el saque.
- **[VERIFICADO WSF 2025, reglas 5.7, 5.8 y 6.2]** Las adaptaciones están en §8.7.5.
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

### 8.7 Líneas y decisiones arbitrales (FR6) [APROBADO·D28; concreción: PROPUESTA]

#### 8.7.1 Verificación previa [D28] — realizada el 2026-10-20

**Resultado (enmienda E.0):** fuentes consultadas: *WSF Specifications for Squash Courts*, cláusulas 4–5, y *World Squash Rules of Singles Squash 2025*, en vigor desde el 1 de septiembre de 2025, reglas 5–6 y apéndice 1. Se confirmaron las consecuencias de §8.7.4 y **se corrigió** el borde de referencia de §8.7.2. Los detalles están en el anexo E.0.

Antes de implementar cualquier regla de línea, Claude verifica las **dimensiones y las reglas oficiales vigentes de la WSF**, citando la fuente: la especificación de cancha y las reglas de juego. Lo que sigue es la estructura del modelo. Las consecuencias marcadas [SUPUESTO] son la lectura actual de Claude, y se confirman o corrigen en esa verificación. Si la verificación cambia algo, se registra en el anexo E y se avisa al PO.
- **Cuándo:** las dimensiones y la geometría, antes del plan de First Ball, porque las líneas se dibujan y la cancha se modela ahí. Las reglas de línea, antes de implementarlas en First Rally (retorno) y First Match (saque).

#### 8.7.2 Geometría de las líneas (en los datos, no en el código)
Para cada línea se guardan:
- la superficie;
- la posición del **borde de referencia** desde el que se mide la altura o la distancia reglamentaria;
- el **ancho**: 50 mm [VERIFICADO, cláusula 5.01.01];
- el lado hacia el que crece la línea desde el borde de referencia.

**[VERIFICADO WSF, cláusula 5.01.04]** "All vertical dimensions are from the finished floor surface to the **underside** of wall markings". Por lo tanto:
- **Out del frontis:** franja entre 4,570 y 4,620 m. Tocarla o pegar por encima es out.
- **Línea de saque:** franja entre 1,780 y 1,830 m. El saque tiene que pegar por encima de 1,830 m.
- **Out trasero:** franja entre 2,130 y 2,180 m.
- **Laterales:** una diagonal que une los extremos de la línea del frontis y la de la pared trasera. La franja tiene **50 mm de ancho perpendicular a la diagonal**, con el **borde inferior** como referencia [APROBADO·D33]. [Decisión D-6](../../reviews/2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación) y [autorización PO T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24).
- **Tin:** "from the floor up to and including the lowest horizontal line". El tin abarca desde el suelo hasta 0,48 m, y sus 50 mm superiores (0,43–0,48 m) son la franja visible.
- **Suelo:** la short line ocupa entre 5,440 y 5,490 m del frontis. Los cuadros de saque son de 1,600 m de lado interior. *[Interpretación de Claude: la línea media está centrada en x = 3,20 m y las líneas de los cuadros quedan por fuera del lado interior.]*

*La v0.3 suponía el borde superior. Ese supuesto estaba equivocado y se corrige aquí (anexo E.0).*

#### 8.7.3 Huella de contacto [EXPERIMENTO]
- Es una **simplificación experimental** de la deformación de la pelota [D28]: un disco de radio fijo `r_huella`, configurable y con ~10 mm de partida, centrado en el punto de impacto.
- **Tocar una línea** significa que la huella se superpone con la franja de la línea.
- La **mira previa** usa la misma huella, de modo que se pone roja de forma coherente con la regla (§7.6.5).
- Como alternativa se puede registrar el punto de contacto sin huella (`r_huella = 0`). La huella dependiente de la velocidad queda descartada por ahora (FR6, L-C).

#### 8.7.4 Consecuencia por línea y por fase [VERIFICADO WSF 2025: reglas 5.7, 5.8 y 6.2, y apéndice 1]

**No se asume que tocar cualquier línea tenga siempre la misma consecuencia** [D28]. Cada línea se juzga solo en las fases en que es relevante:

| Superficie | Línea | En el saque | En el rally (retorno) |
|---|---|---|---|
| Frontis | Tin (franja inferior y su línea superior) | Tocarla es **saque fallado** (punto para el receptor) | Tocarla es **bajo** (pierde quien pegó) |
| Frontis | Línea de saque (1,78 m) | La pelota tiene que pegar **por encima**: tocar la línea o pegar debajo es **saque fallado** | **No se juzga** |
| Frontis | Out (4,57 m) | Tocar o pasar es saque fallado | Tocar o pasar es **out** |
| Laterales | Out (diagonal) | Tocar o pasar es saque fallado | Out |
| Pared trasera | Out (2,13 m) | Tocar o pasar es saque fallado | Out |
| Suelo | Short line y línea media que delimitan el cuarto del receptor | Si el receptor **no volea**, que el primer bote toque una de esas líneas es **saque fallado** | **No se juzga** |
| Suelo | Líneas del cuadro de saque | No se juzgan para la pelota. El foot fault está desactivado (adaptación, §8.7.5). | No se juzga |

Además, en el saque la pelota tiene que ir **directo al frontis**: tocar antes una lateral es saque fallado, sin importar la línea (§8.5). **También es fallo** si la pelota toca a la vez el frontis y una lateral (regla 5.7.3). En el rally, una pelota que toca el frontis y después el tin es **down** (apéndice 1).

#### 8.7.5 Adaptaciones arcade (registro obligatorio) [D28]
Toda diferencia con el reglamento se documenta aquí, con su motivo. Ninguna se presenta como una validación del reglamento real.

| Adaptación | Reglamento (WSF 2025) | Motivo | Estado |
|---|---|---|---|
| Huella de contacto de radio fijo | La pelota se deforma según la velocidad del impacto | Simplicidad y una regla predecible | [EXPERIMENTO] |
| Game a 7, diferencia de 2 | Games a 11 | Partidos cortos | [APROBADO·D02] |
| Sin let ni stroke, y sin interferencia física | Let y stroke por obstrucción | Alcance del prototipo | [APROBADO·D09] |
| Colocación automática en el cuadro de saque y sin foot fault | El sacador coloca un pie en el cuadro | Movimiento automático | [PROPUESTA] |
| Lanzamiento del saque repetido sin falta si se le pega al aire (§6.6) | Regla 5.7.1: se permite otro intento antes de que la pelota toque algo; si toca el suelo sin haber sido golpeada, es falta. Regla 5.9: lanzar sin intentar golpear no es saque y se puede repetir, **igual que en el juego**. | Fallar el lanzamiento no debe costar un punto | [PROPUESTA] |
| Ritmo global τ < 1 | Velocidad real | Legibilidad en el teléfono | [EXPERIMENTO] |
| Sin spin en el MVP | La pelota gira | Simplicidad | [PROPUESTA] |
| Estado "cortada" del drop, si se agrega (§7.5) | Efecto del corte | Que el drop muera | [FUTURO condicional] |
| Un solo intento de golpe por recepción | Apéndice 1, "further attempt": se puede volver a intentar mientras la pelota esté en juego | Que el timing tenga costo | [APROBADO·D19] |
| Tin plano, en el plano del frontis | El tin sobresale de 15 a 45 mm y está biselado (cláusula 5.02.03) | Simplicidad. Se juzga por la altura del contacto. | [PROPUESTA] |

#### 8.7.6 Marca de cantado [APROBADO·D28]
- Cuando el punto termina por **tin, out o saque fallado**, aparece durante ~1 s una **marca ampliada** del contacto sobre la línea, tipo ojo de halcón: la huella, la franja de la línea y la distancia en mm. Se muestra durante la pausa entre puntos y no interrumpe el juego.
- [EXPERIMENTO, PENDIENTE PO N5] La marca también aparece en las pelotas **buenas por poco**, a menos de `d_ajustado` de la línea (valor de partida: 30 mm), solo al terminar el punto.
- **Pruebas en el borde** (§17.A): contacto justo debajo de la línea, contacto que roza la línea y contacto exacto sobre el borde, en cada superficie y en cada fase en que la línea se juzga.

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
- **Candidatos:** puntos de la trayectoria predicha dentro de la ventana legal L, muestreados cada ~20 ms de simulación. Se agrupan en **oportunidades** (antes del bote, que es volea, o después del bote), y cada oportunidad tiene su propio plan (§7.1.1, §7.3).
- Para cada candidato se calcula el tiempo de llegada `t_arr` y la **holgura** `s = T − t_arr`.
  - **Legal:** `s ≥ 0` con el alcance normal, o `s ≥ 0` usando la estocada.
  - **Preferido:** el que maximiza una puntuación explícita:
    `U = w₁·min(s, s_cap) + w₂·altura_preferida(0,4–1,1 m) + w₃·separación_de_paredes(≥ 0,3 m) − w₄·estocada`
    [pesos: EXPERIMENTO].
- **Calidad de llegada** `q ∈ [0; 1]`: depende de la holgura y de si hizo falta la estocada. Es la fuente "Posición" del error (§7.6).
- **Estabilidad:**
  - el plan se calcula al terminar `d_react`;
  - se cambia solo si deja de ser legal, o si otro candidato lo supera por un margen de **histéresis** y el plan no está bloqueado;
  - **se bloquea** en `T* − 350 ms` [EXPERIMENTO] o al soltar.
- **Asistencia extra = 0:** no hay bonos de velocidad ni de alcance para el humano ni para la IA. Si se cambia en el panel para un experimento, queda registrado.
- **Lo que el planificador no decide** [APROBADO·D24]: no elige entre volea y bote (§7.3.3), no suelta por el jugador y no mueve W. La puntuación U solo elige el **punto de contacto dentro de cada oportunidad**.
- **Sin candidatos legales:** caso 7 de §7.1.3 (inalcanzable).

### 9.2 Recuperación a la T
Después del contacto, el personaje vuelve a la posición de espera W (por defecto la T), con la misma cinemática. Mientras se recupera, es vulnerable: si el golpe propio dio poco tiempo, todavía no llegó a W cuando el rival le pega.

### 9.3 Posición de espera elegida por el jugador [APROBADO·D23 como mecanismo; radio: EXPERIMENTO]
- **W influye en las oportunidades de volea** [D23]. Esperar adelante crea más voleas y cubre peor las pelotas largas (§7.3.4).
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
  - se habilita el caso 12 de §7.1.3 (golpe al marco, error de ejecución), solo con mala posición;
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
| **Volea** (experimento) | Soltar en el anillo de volea o dejar pasar la pelota | Le quita tiempo al rival | Más error y menos tolerancia. Dejar pasar no garantiza el bote. | §7.3.5 |
| **Timing** | Soltar en el momento justo, o arriesgar adelantado o atrasado | Golpe limpio | Golpe suelto o al aire | Distribución de Δ y categorías (§7.1.3) |

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
| Técnica (= 0,5·precisión + 0,5·subatributo del golpe) | Atenúa los vectores causales de posición y de dificultad situacional (§7.6.2). No atenúa el de timing. | ±15 % de su magnitud | Qué golpe intentar |
| **Consistencia** (pilar Mental) | Factor `f` de la variabilidad aleatoria (§7.6.4). **Solo en el brazo (c) del experimento de First Fun.** | `f` ∈ 0,75–1,25 | Ninguna: no reemplaza decisiones |
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
- **Primer efecto Mental:** la Consistencia, como experimento acotado en First Fun (§7.6.4). Su resultado sirve como **indicio** para el milestone Personajes, no como validación del pilar. **No se asume que lo mental sea la causa dominante del error** [D26].
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
| **Prohibido** | El toque, el gesto, la mira o la potencia del humano. Si el humano está apuntando, y su Δ. Si el humano piensa volear o dejar pasar la pelota, y qué anillos ve. La posición de espera elegida *antes* de que el personaje se mueva. Los objetos internos del planificador del rival. **Los valores aleatorios futuros.** El resultado de un golpe antes de que ocurra. |
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

**3. Ejecución, por el mismo contrato que el humano (§7.1, T3):**
- La IA genera la intención (P, p) con **ruido de intención** `σ_int` según dificultad.
- **Elige la oportunidad** (volea o bote) como una opción más de su decisión por utilidad, con las mismas oportunidades visibles que tendría el humano (§7.3.2). El planificador no la elige por ella.
- Muestrea su instante de soltar `r = T*(oportunidad) + Δ`, con `Δ ~ N(0, σ_timing(dificultad))`. **Un solo intento**, como el humano.
- Por lo tanto **puede pegar adelantado o atrasado, pegarle al aire o llegar tarde** si Δ lo lleva fuera de ±A o de K, igual que el humano.
- El sistema compartido decide el contacto y sus dos capas de error (§7.6).

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
- **Sonido distinto** para la raqueta, el frontis, las laterales, el tin, el fin del punto y el **golpe al aire** (§7.1.5).
- **Vibración háptica** breve en el contacto. El golpe al aire no vibra.
- **Marca de cantado** en las decisiones de línea (§8.7.6).
- **Alternativa visual** para cada evento (destello en la pared, marca roja en el tin), por si se juega sin audio.

---

## 13. Panel de desarrollador — Prioridad 7

### 13.1 Alcance, acceso y restricción [APROBADO·D17, D29; acceso: PROPUESTA]
- **El panel es parte del MVP desde First Ball** y crece con cada milestone [D29].
- **Objetivo:** que **ninguna calibración requiera modificar código**. Todo parámetro marcado [EXPERIMENTO] en esta especificación tiene un control en el panel a partir del milestone en que se usa.
- Se abre con tres dedos mantenidos 1 s o desde el menú de pausa.
- **Sin backend** [D17, D29].
- **El código del panel queda fuera de las builds de producción** gracias a un flag de compilación, y se verifica con una prueba (§17.A). Las builds de desarrollo que Codex instala en el iPhone del PO sí lo incluyen.

### 13.2 Funciones por milestone (crecimiento progresivo)

| Milestone | Funciones nuevas |
|---|---|
| First Ball | Lanzador (posición, velocidad, dirección, presets de lanzamiento). Reinicio. **Física** (gravedad, restitución y fricción por superficie, τ, λ, paso fijo, impactos por paso). Cámara (fija o con seguimiento, posición, FOV). Trayectorias de depuración. Cámara lenta. Avance **por pasos de simulación**. **Diagnóstico:** FPS y tiempos de frame, desvío entre predicción y simulación, eventos de contacto. **Presets locales: guardar, cargar y exportar** desde este milestone, para calibrar la física sin tocar código. |
| First Shot | Variante de control (A′, B o C) y sus parámetros. **Timing:** A, `A_volea`, nivel de ayuda (completa, parcial o sin anillo), T3 o T3-suave, compensación de latencia, mostrar la nota. Bandas. **Error, capa causal:** magnitudes y topes de los vectores, filas activas de la tabla §7.6.3 y atenuación por técnica. **Líneas:** `r_huella` y marca de cantado. Superposición de depuración (contacto, P\*, T\*, Δ, K, geometría del contacto, vectores causales). Instrumentación del input. |
| First Rally | **Movimiento:** pesos del planificador, histéresis, bloqueo, alcance, estocada, radio de W y asistencia extra (registrada). **Volea:** sin volea, V1 o V2 + V3, modo M1 o M2, `q_min_rel`, separación mínima y alturas. **IA:** `d_perc`, `d_dec`, `σ_pred`, `σ_int`, `σ_timing` y temperatura. Visualización de la holgura, las oportunidades y el plan. |
| First Match | Reglas (tin, puntos para ganar, líneas por fase). Métricas de ritmo y duración. `d_ajustado` de la marca de cantado. |
| First Fun | **Cansancio:** experimentos 1 y 2, consumo, piso y recuperación. **Error, capa aleatoria:** brazo (a), (b) o (c), `σ_base` por intención, rango de `f(Consistencia)`, `σ_max` y semilla. **IA:** estilos y dificultad. Prueba a ciegas (rival al azar oculto). |

### 13.3 Herramientas de depuración
Reiniciar el rally, cámara lenta, trayectorias, posiciones y métricas (del brief), además de:
- modo práctica con lanzador;
- superposición de contacto y error;
- pausa con avance paso a paso.

### 13.4 Reglas de cambio y persistencia (F21) [PROPUESTA]

**Clases de parámetros:**

| Clase | Ejemplos | Cuándo se aplica |
|---|---|---|
| **Inmediata** | Cámara, ayudas visuales que no cambian el juego (nota de timing, trayectorias, superposición) | En el acto. *El nivel de ayuda del anillo cambia lo que el jugador puede hacer, así que es de "próximo rally".* |
| **Próximo rally** | Física, golpes, timing (incluido el nivel de ayuda), error, líneas, planificador, volea, IA | Al empezar el siguiente rally. El panel muestra "cambios pendientes" y ofrece **"Aplicar y reiniciar rally"**. **Nunca se aplican a mitad de un gesto.** |
| **Reinicio de partido** | Reglas, puntos para ganar, estilo de la IA | En el siguiente partido |

**Persistencia:**
- Los presets se guardan como **JSON local versionado** en el almacenamiento de la app, con nombre, versión de esquema, unidades y rangos.
- Al arrancar se carga el último preset activo. Hay que verificar que se recupera tras reiniciar la app.
- **DefaultPreset.json es la única fuente de valores por defecto**, con validación de esquema, tipos y rangos [APROBADO·D32]. El catálogo define tipos, rangos, opciones y clases de aplicación, sin defaults duplicados. En tiempo de ejecución se trabaja con una copia. El estado de la partida (energía, marcador) **nunca** se guarda en la configuración. [Decisión D-5](../../reviews/2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación) y [autorización PO T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24).

**Trazabilidad:** cada registro de métricas guarda el preset (nombre y hash), la versión del build y la semilla.

**Exportación:** el preset (y opcionalmente las métricas) se exporta como texto mediante el menú de compartir de iOS o el portapapeles. Así el PO se lo pasa a Codex; la promoción a defaults requiere su autorización. **P0-calib-1 no se promueve** [APROBADO·D35]; defaults **0d39e6016bd8** intactos. [Decisión PO, paso 2 de T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24).

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
| Gesto (A′, B y C) y contrato de contacto T3 (con T3-suave como control) | | ✔ | ✔ | ✔ | ✔ |
| Niveles de ayuda de timing | | ✔ | ✔ | ✔ | ✔ |
| Drive, cruzado y drop | | ✔ | ✔ | ✔ | ✔ |
| Modo práctica con lanzador | (lanzador) | ✔ | ✔ | ✔ | ✔ |
| Error: capa causal no direccional | | ✔ | ✔ | ✔ | ✔ |
| Retroalimentación de sonido, háptica y visual | | ✔ | ✔ | ✔ | ✔ |
| Movimiento automático, planificador, T y W | | | ✔ | ✔ | ✔ |
| IA básica (una política, Fondo) | | | ✔ | ✔ | |
| Reglas del retorno y fin del rally, líneas del retorno y marca de cantado | | | ✔ | ✔ | ✔ |
| **Experimento de volea** (sin volea, V1 y V2 + V3) | | | ✔ | | |
| Saque, líneas del saque (incluidas las del suelo), rotación, puntuación a 7 y revancha | | | | ✔ | ✔ |
| Pausa entre puntos | | | | ✔ | ✔ |
| Dos estilos y tres dificultades | | | | | ✔ |
| Cansancio (experimentos 1 y 2) | | | | | ✔ |
| Capa aleatoria acotada (experimento) | | | | | ✔ |
| Vectores causales direccionales (E1, E2, E4, E6) | | (cuando el PO valide cada fila) | | | ✔ |
| Panel y presets locales | ✔ (progresivo, §13.2) | ✔ | ✔ | ✔ | ✔ |

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
| **First Shot** | ¿Puedo pegar con intención y entender el resultado? | Práctica con lanzador. Variantes A′, B y C. Drive, cruzado y drop. Timing T3 y niveles de ayuda. | Variante de control elegida. El timing se aprende y anticiparse no domina (H3, H3b). T3 o T3-suave elegido. Los fallos tienen una causa visible y una categoría distinguible. El drop se siente drop (§7.5). |
| **First Rally** | ¿Hay intercambio y decisiones que cambian el punto? | Rally contra una IA básica (Fondo) sin saque: empieza con un lanzamiento neutro. Energía fija. Sin volea, V1 y V2 + V3. | La asistencia no devuelve todo. Los golpes profundos bajan la holgura del rival. W tiene efecto, también sobre la volea. La volea se siente una decisión propia (H6b). |
| **First Match** | ¿El partido sostiene el ritmo? | Saque, reglas completas, marcador a 7, pausa, revancha. | Reglas estables. Revancha inmediata. Duración **observada**. |
| **First Fun** | ¿Quiero repetir porque aprendo y puedo jugar distinto? | Dos estilos, tres dificultades. Cansancio, capa aleatoria (brazos a, b y c) y vectores direccionales introducidos **por separado**. | El PO reconoce los estilos y su contrajuego, entiende los errores sin el panel y pide revancha en varias sesiones. |
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
| 16.5 | Sin volea, o volea que decide el sistema | Mitigado | Experimento en First Rally con V2 + V3 y V1 como control (§7.3). Invariante: el planificador nunca volea solo. |
| 16.6 | Sin let ni stroke | Aceptado (D09) | No se presenta como validación. Queda para M9+. |
| 16.7 | Errores aleatorios frente a errores comprensibles | Replanteado (D26) | Dos capas separadas. La aleatoria está apagada hasta First Fun y después se prueba en tres brazos, con semilla (§7.6). |
| 16.8 | Calibración de la física | Abierto | Panel, presets exportables, escenarios de referencia (anexo C). |
| 16.9 | Curva de aprendizaje de Unity para el PO | Mitigado | El PO no necesita el editor. Codex compila e instala. |
| 16.10 | Crecimiento del alcance | Mitigado | La tabla §14 es el alcance. Cualquier cambio requiere al PO. |
| 16.11 | Rendimiento | Abierto | §17.A y §17.B, medidos en el iPhone del PO. |
| 16.12 | **Compatibilidad entre Unity 6 LTS y Xcode 27.1 / iOS** | **Nuevo, sin verificar** | Es lo primero que se verifica en la Preparación. Si no hay soporte, se decide entre otra versión de Unity o de Xcode. |
| 16.13 | **Acoplamiento entre potencia y puntería** | Nuevo (F04) | Experimento de §6.3. |
| 16.14 | **Penalizaciones dobles** (cansancio, posición, timing) | Mitigado (F16, D26) | Sin término directo de cansancio. El timing actúa solo a través de la geometría del contacto. La variabilidad no depende de las causas (§7.6.2). Invariante §17.A.12. |
| 16.15 | **La predicción no coincide con la simulación** | Abierto (F09, D27) | Condiciones 1–5 e invariante bit a bit (§8.2.1). Caso 9 de §7.1.3 medido como falla del sistema. |
| 16.16 | **T3 frustra:** demasiados golpes al aire | Nuevo (D19) | Niveles de ayuda, A configurable, compensación de latencia y T3-suave como control (§7.1.4). Se mide la tasa de fallos de timing. |
| 16.17 | **Legibilidad de dos anillos** en vertical, bajo presión | Nuevo (D23) | Solo se muestran las oportunidades relevantes, con colores distintos y separación mínima (§7.3.2). Se valida en First Rally. |
| 16.18 | **Reglas de línea mal supuestas** | Nuevo (D28) | Verificación WSF antes de implementar y registro de adaptaciones (§8.7). |

---

## 17. Criterios de aceptación y métricas

Los criterios son una **guía flexible** y el PO decide cuándo se cumple un milestone [APROBADO·D10]. Se separan en cinco clases (R2 §6, D25). **Solo la clase A es obligatoria.** La clase F se ejecuta siempre, pero un fallo provoca una revisión, no un bloqueo automático. Las demás se ajustan con la evidencia, y ningún valor experimental se convierte en una restricción permanente.

### 17.A Invariantes técnicas (obligatorias)
1. **Una sola autoridad sobre la pelota.** Ninguna pelota atraviesa superficies (0 de 10 000 en S8). En esquinas, la rapidez nunca aumenta tras un contacto.
2. **Consistencia:** el mismo estado inicial produce el mismo resultado en el mismo binario y el mismo dispositivo, con la capa aleatoria apagada o con la misma semilla.
3. **Predicción:** si el predictor comparte paso y código con la simulación, la predicción desde el estado de simulación coincide **bit a bit** con la simulación hasta el siguiente contacto de raqueta, en S1–S9 y en el dispositivo (§8.2.1). Si se usa un predictor más grueso, rigen los umbrales de §8.2.2.
4. **El punto se cierra una sola vez.** Ningún evento posterior cambia el marcador.
5. **Contrato de contacto:** cada caso de §7.1.3 tiene pruebas automáticas y las pasa. No hay contacto después del segundo bote ni fuera de K. **Nunca hay más de un swing por recepción.** Cancelar no ejecuta swing.
6. **Ningún error de ejecución con buena posición** (caso 12). El fallo de timing (casos 4 y 5) **no** cuenta como error de ejecución [D20].
7. **El planificador nunca ejecuta una volea** ni ningún contacto sin que el jugador suelte [D23, D24]. Hay una prueba en la que no se suelta en el tramo de volea: el resultado es siempre "sin volea".
8. **Reglas:**
   - saque directo al frontis frente a uno que toca antes la lateral;
   - líneas por **huella**, según la tabla por línea y por fase de §8.7.4, con **pruebas en el borde** (debajo, rozando y exactamente en el borde) en cada superficie y fase;
   - tin;
   - out en cada pared;
   - líneas del suelo solo en el saque y solo si no hay volea;
   - segundo bote;
   - rotación del saque;
   - diferencia de 2;
   - turno de golpe.
9. **Simetría entre humano e IA:** las restricciones de alcance, tolerancia de timing, oportunidades visibles, intento único y error son idénticas. Hay una prueba de que la IA no accede a los datos prohibidos (§11.2).
10. **Panel excluido de producción.** Una build de producción no contiene sus tipos.
11. **Fallas del sistema** (caso 9 que termina en inalcanzable): se registran y su objetivo es ≈ 0.
12. **Sin penalización duplicada del timing:** con la misma geometría del contacto, Δ no cambia el resultado. Hay una prueba en la que dos contactos con Δ distinto y la misma geometría producen el mismo golpe.
13. **La capa aleatoria no depende de las causas:** con la misma intención, Consistencia y semilla, la desviación aleatoria es la misma con cualquier timing, posición o energía.
14. **Presets:** se recupera el preset tras reiniciar la app. El registro de métricas incluye el preset, el build y la semilla.

### 17.B Hipótesis de jugabilidad (se prueban y pueden fallar)
| # | Hipótesis | Evidencia | Milestone |
|---|---|---|---|
| H1 | En vertical, la pelota se lee sin el panel | Escenarios de §12.3, juicio del PO | Ball |
| H2 | Una de las variantes A′, B o C permite apuntar y dar potencia con intención, con una mano | Métricas de §6.3 y juicio del PO | Shot |
| H3 | **Con T3, el timing es una habilidad que mejora con la práctica** [D22] | La dispersión de Δ y la tasa de fallos de timing del PO bajan entre sesiones (con la latencia ya calibrada). Juicio del PO: "el timing se juega". | Shot |
| H3b | **Anticiparse sistemáticamente no es una estrategia dominante** [D22] | Bots en el modo práctica: uno que suelta con un desfase fijo **fuera de ±A** (−60, −100 y −150 ms) rinde **peor** que uno con timing preciso, medido por el vector causal promedio, la tasa de golpes al aire y el acierto en la zona. Se repite con cada nivel de ayuda. *Un desfase dentro de ±A es equivalente por diseño.* | Shot |
| H3c | T3 no frustra más que T3-suave sin aportar más habilidad | Comparación T3 frente a T3-suave: tasa de golpes al aire, valoración del PO | Shot |
| H4 | Los errores se entienden sin el panel | El PO explica la causa y la categoría de un fallo en la mayoría de los casos | Shot y Fun |
| H5 | El movimiento automático no vuelve pasivo al jugador | Los golpes profundos bajan la holgura del rival, W tiene efecto medible, juicio del PO | Rally |
| H6 | La volea agrega profundidad táctica | Comparación entre sin volea, V1 y V2 + V3 (§7.3.5) | Rally |
| H6b | **La volea se siente una decisión del jugador** | Con V2 + V3, la tasa de volea cambia con W y con la situación. El PO juzga que la decisión es suya, y que las voleas inconvenientes se reconocen por sus consecuencias. | Rally |
| H7 | Los estilos se reconocen y tienen contrajuego | **Prueba a ciegas exploratoria:** rival oculto al azar, el PO lo identifica. *No es evidencia estadística* (F23). | Fun |
| H8 | El cansancio agrega drama sin espiral de derrota | Experimentos 1 y 2 por separado, juicio del PO | Fun |
| H9 | Una variabilidad acotada mejora el juego frente a la capa causal sola | Brazo (b) frente a (a), con semilla, y juicio del PO | Fun |
| H9b | La Consistencia aporta algo perceptible frente a la sola dificultad inherente | Brazo (c) frente a (b). Si no mejora la experiencia, se retira [D26]. | Fun |
| H10 | El juego da ganas de revancha en **varias** sesiones | El PO juega otra partida por iniciativa propia y recuerda una decisión táctica útil | Fun |
| H11 | Las decisiones ajustadas de línea se entienden y se aceptan | Con la marca de cantado, el PO no percibe como injustas las decisiones de tin, out o saque | Rally y Match |

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
- **Bandas de calibración del anexo C (capa 3):** son rangos de partida que el PO ajusta con presets. Se registran los eventos completos de cada golpe: primer y segundo bote (lugar y tiempo), margen al tin y desvío respecto del objetivo. Los porcentajes objetivo los fija el PO al ver los datos.
- **Tasa de fallos de timing:** se mide por nivel de ayuda. No tiene un objetivo fijo, pero sirve para decidir entre T3 y T3-suave (H3c).

### 17.D Decisiones pendientes que afectan a los criterios
Ver §18. En particular: el modelo de iPhone (umbrales de rendimiento), la tabla de errores (§7.6.3), la verificación WSF de las líneas (§8.7.1) y la revisión de las relaciones del anexo C (N6).

### 17.E Métricas registradas
| Grupo | Métricas |
|---|---|
| Tiempo de contacto | Distribución de Δ, categoría (asistido, adelantado, atrasado, al aire, tarde, cancelado, sin orden, inalcanzable), geometría del contacto, nivel de ayuda, T3 o T3-suave, T\* frente al contacto real |
| Latencia del input | Marca del evento → procesamiento → contacto → retroalimentación |
| Precisión | Mira deseada frente a final (experimento de controles), objetivo frente a impacto real en el frontis, primer y segundo bote, margen al tin |
| Rebotes y trayectorias | Igualdad o desvío entre predicción y simulación, contactos por paso, eventos de esquina |
| Líneas | Distancia de la huella a la línea en cada decisión, superficie, fase y si se mostró la marca de cantado |
| FPS y estabilidad | Tiempos de frame (p50/p95/p99), frames largos, uso de CPU y GPU, estado térmico, duración de la sesión |
| Rallies | Duración activa, golpes, forma en que termina (tin, out, segundo bote, ganador, inalcanzable, fallo de timing, sin orden) |
| Errores | Vector causal por causa, vector total y su tope. Desviación aleatoria, brazo y semilla. Causa dominante. |
| Decisiones tácticas | Distribución de golpes por contexto, uso de W, acierto de la apuesta, % de tiempo en la T, holgura del rival según el golpe |
| Volea | Oportunidades mostradas y elegidas, su contexto, resultado de dejar pasar, modo M1 o M2 |
| Partido | Duración (mediana y p90), rallies, revanchas por sesión |

### 17.F Pruebas deportivas del anexo C (capas 1 y 2) [APROBADO·D25]
- Son **pruebas automáticas** de capacidad expresiva y de relaciones comparativas. Cada una se evalúa **solo dentro de sus condiciones de validez** (anexo C).
- **No son invariantes universales** [D25]. Si una falla, se diagnostica: puede ser un error del código, una calibración que rompió la lógica del squash, o una relación cuyas condiciones de validez estaban mal declaradas. **El PO decide** si se corrige el código, se ajusta la calibración o se corrige la relación, y la decisión se registra.
- Por encima de las pruebas está la **capa 4**, el juicio deportivo del PO: un escenario que pasa todas las pruebas pero no se siente squash se revisa igual.

---

## 18. Decisiones pendientes del Product Owner

### 18.1 Resueltas en la v0.3
P1 (contrato de contacto) → D19–D22. P6 (posición de espera) → D23. P9 (forma del anexo C) → D25. P11 (quién decide la volea) → D23. P10 (revisión del registro) queda incluida en la revisión de esta v0.3.

### 18.2 Bloqueantes reales

Solo estos puntos impiden avanzar. **Ninguna decisión de gameplay pendiente bloquea el plan de First Ball**, porque todas tienen un valor por defecto y caen en milestones posteriores.

| # | Qué falta | De quién | Qué bloquea |
|---|---|---|---|
| B1 | **Aprobación de la v0.3 y autorización del plan técnico de First Ball** | PO | El plan de implementación |
| B2 | **Modelo de iPhone y versión de iOS** (P4) | PO | La Preparación: registro del dispositivo, compatibilidad y umbrales de rendimiento |
| B3 | **Unity 6 LTS instalado con iOS Build Support** (falta el inicio de sesión y la licencia Personal en Unity Hub) | PO, con ayuda paso a paso | La Preparación |
| B4 | **Compatibilidad entre Unity 6 LTS y Xcode 27.1 / iOS** (riesgo 16.12) | Verificación técnica en la Preparación | Todo lo que va al iPhone |
| B5 | **Autorización de la prueba técnica de física** y sus escenarios (P5) | PO. Puede venir junto con B1. | La decisión de física en First Ball |
| B6 | **Verificación WSF** de las dimensiones y la geometría de las líneas (§8.7.1) | Claude | El plan de First Ball (modelo de la cancha) |

### 18.3 No bloqueantes (tienen valor por defecto)

| # | Decisión | Cuándo | Valor por defecto |
|---|---|---|---|
| P2 | **Autorizar el experimento de controles A′, B y C** (§6.3) y su protocolo | Antes del plan de First Shot | Las tres variantes |
| P3 | **Validar la tabla de error relativa al cuerpo** (§7.6.3), fila por fila | Cuanto antes. A más tardar, antes de First Fun. | Solo los efectos no direccionales (E3, E5, E7, E8 y el timing simétrico) |
| P7 | **Lateralidad:** ¿el drive y el revés tienen calidades distintas durante la validación? | Antes de First Shot | Mismo nivel. Solo cambia la animación. |
| P8 | **Altura del tin** | Antes de First Ball, junto con B6 | 0,48 m, si la WSF lo confirma |
| P12 | Mecanismo de ralentización: ritmo global τ o escala de lanzamiento λ | Después de First Ball | τ |
| P13 | Nombres provisionales de los rivales | Cuando quiera | "Fondo" y "Shot" |
| N1 | ¿**Cancelar** consume el intento de la recepción? (§6.5, §7.1.3 caso 10) | Antes de First Shot | No lo consume. El costo es el tiempo. |
| N2 | ¿Un **toque accidental** en Recepción se ignora, sin consumir el intento? (§6.5) | Antes de First Shot | Se ignora |
| N3 | Umbral de **relevancia** para mostrar una oportunidad de volea (§7.3.2) | Durante First Rally | `q_min_rel` = 0,2, bajo a propósito |
| N4 | **Movimiento con dos oportunidades:** M1 (guía por bote) o M2 (guía por volea) (§7.3.3) | Durante First Rally | M1 |
| N5 | ¿La marca de cantado también en las pelotas **buenas por poco**? (§8.7.6) | Durante First Rally | Sí, a menos de 30 mm, solo al terminar el punto |
| N6 | Revisar el **contenido** del anexo C: escenarios, relaciones, condiciones de validez y bandas | Antes de First Shot | Los del anexo C de la v0.3 |
| N7 | **Fallo por soltar tarde:** la v0.3 lo clasifica como fallo de timing ("¡Tarde!"), no como omisión, para que el jugador lo distinga de no haber soltado (§7.1.3, caso 5) | Antes de First Shot | Fallo de timing |

---

## Anexo A. Supuestos y lineamientos técnicos (F20)

- [APROBADO·D01] Unity, C#, macOS, iPhone y vertical.
- [SUPUESTO] **Unity 6 LTS** con el módulo iOS. **Hay que verificar su compatibilidad con Xcode 27.1** (riesgo 16.12). Unity Hub está instalado. *Estado el 2026-10-20: el Mac tiene instalado Unity 6000.6.6f1, sin módulo iOS. La línea LTS vigente es la 6.3 (6000.3.x), con soporte hasta diciembre de 2027; la 6.0 LTS termina su soporte en octubre de 2026. La compatibilidad con Xcode 27.1 no está documentada y se verifica en la Preparación del plan de First Ball.*
- [PROPUESTA] URP y el Input System nuevo, usando las marcas de tiempo de los eventos (§8.4).
- [PROPUESTA] **Arquitectura proporcional:**
  - **Dos ensamblados.**
    - *Lógica:* reglas, contrato de contacto, solver, modelo de error, decisión de la IA y puntuación. Sin dependencias de escena, para que las pruebas EditMode sean rápidas.
    - *Integración:* presentación, input, cámara, panel y el adaptador de física.
  - El adaptador de física **puede usar Unity** si gana P1 o P3.
  - Corrección de la v0.1: **el C# puro no es un requisito para Android** (Unity ya es multiplataforma) y **el modo online no justifica ninguna abstracción hoy**. La frontera existe solo porque abarata las pruebas.
  - **No se usan:** ECS/DOTS, contenedores de inyección, buses globales de eventos, frameworks genéricos de habilidades, motores de red, aritmética determinista ni motores físicos de propósito general.
- [PROPUESTA] El panel se excluye de las builds de producción con un flag de compilación (§13.1).
- [APROBADO·D32] **DefaultPreset.json como única fuente de defaults**, con validación de esquema, tipos y rangos; copia en tiempo de ejecución y presets JSON (§13.4). [Decisión D-5](../../reviews/2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación) y [autorización PO T7.3](../../reviews/2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24).
- [PROPUESTA] Git y GitHub con PRs revisados por Claude. Estructura `docs/superpowers/specs`, `docs/superpowers/plans` y `docs/reviews`. Agregar `.superpowers/` al `.gitignore`.
- [APROBADO·D18] Codex trabaja en el Mac del PO. [PROPUESTA] Compila con Unity en modo batch, genera el proyecto Xcode y lo instala en el iPhone conectado con `xcodebuild` / `devicectl`. TestFlight llega con la progresión.
- [SUPUESTO] El PO tiene Xcode 27.1 con la licencia aceptada (verificado), cuenta de Apple Developer de pago (declarada) y un Mac con chip M5 (verificado).

## Anexo B. Glosario

- **T:** intersección de la short line con la línea media.
- **Tin:** franja inferior del frontis.
- **PAR:** se gana un punto en cada rally.
- **Holgura:** tiempo de margen con que el personaje llega al punto de contacto. Si es negativa, no llega.
- **T\*, P\*:** instante y punto de contacto planificados para una oportunidad.
- **Δ:** diferencia entre el momento de soltar y T\*_visible.
- **A:** tolerancia de asistencia de timing (±40 ms de partida). Dentro de ella, el contacto es "Perfecto".
- **K (tramo alcanzable):** instantes en que la pelota está legal y al alcance real del personaje.
- **Oportunidad de contacto:** tramo de K antes del bote (volea) o después del bote.
- **Geometría del contacto:** posición de la pelota respecto del cuerpo en el contacto (adelantado o atrasado). Es la vía por la que actúa el timing.
- **Fallo de timing:** golpe al aire (soltar antes de K) o tarde (soltar después de K). Es distinto del error de ejecución, de lo inalcanzable y de la omisión.
- **Intento:** el único swing permitido por recepción.
- **Capa causal / capa aleatoria:** las dos capas del modelo de error E-C (§7.6).
- **Dificultad situacional / inherente:** la del contacto (causal) y la del golpe pedido (variabilidad).
- **Huella de contacto:** disco de radio `r_huella` con que se juzgan las líneas.
- **Marca de cantado:** visualización ampliada del contacto en una decisión de línea.
- **W:** posición de espera durante la Recuperación.
- **τ:** ritmo global de la simulación.
- **Shot solver:** cálculo de la velocidad de salida a partir del objetivo y la potencia.
- **Nick, drop, boast, volea, let/stroke:** igual que en la v0.1.

## Anexo C. Escenarios deportivos de referencia [APROBADO·D25 como estructura; contenido: PENDIENTE PO N6]

**Objetivo:** comprobar que el juego **permite** jugar un squash tácticamente reconocible, **no** imponer un resultado automático a cada golpe [D25].

**Condiciones comunes**, salvo que una fila diga otra cosa: capa aleatoria apagada, energía fija, ayuda de timing completa, contacto asistido (|Δ| ≤ A), lanzamiento estandarizado del modo práctica (§6.3) y parámetros del preset base registrado.

### C.1 Capa 1: capacidad expresiva (prueba automática, §17.F)
Para cada escenario, **existe** al menos una combinación de mira y potencia, dentro del rango del control, que produce el resultado descrito. La prueba busca en una grilla de miras y potencias. No exige que el resultado salga con la mira por defecto.

| # | Desde | Existe un golpe que… |
|---|---|---|
| X1 | Cuarto trasero, pelota a media altura | …es un **drive paralelo pegado**: primer bote detrás de la short line y trayectoria cercana a la lateral propia en el último tramo |
| X2 | Cuarto trasero | …es un **cruzado ancho**: toca la lateral contraria después del primer bote |
| X3 | Adelante, pelota cómoda | …es un **drop que muere adelante**: segundo bote en el tercio delantero |
| X4 | Atrás | …es un **drop defensivo** que llega al frontis por encima del tin |
| X5 | Cualquier contacto legal con buena posición | …devuelve la pelota al frontis por encima del tin y por debajo del out (que siempre haya un golpe seguro) |
| X6 | Cuadro de saque | …es un **saque válido** al cuarto contrario (§8.7.4) |
| X7 | Cerca de la T, volea habilitada | …es una **volea de drive** que llega al cuarto trasero |

### C.2 Capa 2: relaciones comparativas (prueba automática, §17.F)
Cada relación compara dos golpes que difieren **solo** en la variable indicada. **Fuera de sus condiciones de validez, no se evalúa.**

| # | Relación | Condiciones de validez | Fuera de validez (no se evalúa) |
|---|---|---|---|
| R1 | Con la misma mira, **más potencia** da un primer bote **más profundo** | Mismo contacto y pelota entrante. Mira en la banda media. Potencia en el rango en que el solver no ajusta la velocidad (§7.4, paso 3). Ninguna pared antes del primer bote. | Si con más potencia la pelota toca primero la pared trasera, cuenta como "más profunda", pero no se compara la distancia. Pelotas que tocan la lateral. |
| R2 | Con la misma potencia, una **mira más alta** dentro de la banda media da un primer bote **más profundo** | Mismo contacto. Las dos miras en la banda media y sin ajuste del solver. | La banda alta, donde la pelota puede caer más corta por exceso de arco. Contactos muy bajos. |
| R3 | El **drop desde atrás** deja su segundo bote **más lejos del frontis** que el drop desde adelante, con el mismo margen pedido sobre el tin | La misma intención y el mismo objetivo. Contactos a ≤ 2 m y a ≥ 6 m del frontis, los dos con buena posición y altura cómoda. | Contactos pegados a la pared y voleas |
| R4 | Un golpe **estirado** sale **más corto y más alto** que el mismo golpe con buena posición | Mismo objetivo y potencia, vector E3 activo, sin capa aleatoria | Si E3 se desactiva o el PO marca E3 como incorrecta |
| R5 | Una **volea** deja al rival **menos holgura** que el mismo golpe tras el bote | Misma intención y objetivo, rival que parte de la misma W, calidad de llegada comparable (diferencia en q ≤ 0,1) | Voleas inconvenientes (vectores E6 o E8 activos), en las que la volea puede ser peor a propósito |
| R6 | Apuntar **más cerca del tin** aumenta a la vez la probabilidad de ganador y la de error | **Solo con capa aleatoria activa** (brazos b o c) o con un vector causal distinto de cero, y medido como frecuencia sobre N golpes con semillas registradas | La línea base sin variabilidad y con contacto perfecto: ahí, apuntar al tin con margen > 0 nunca falla, y eso es correcto |
| R7 | Con una pelota entrante **más rápida**, el tramo alcanzable K **dura menos** | La misma trayectoria geométrica a distinta velocidad, la misma posición del cuerpo y el mismo alcance | Trayectorias que cambian de forma, por ejemplo con λ ≠ 1 |
| R8 | Una W **más adelantada** produce **más oportunidades de volea** sobre un mismo conjunto de lanzamientos | El mismo conjunto de lanzamientos y la misma cinemática. Volea habilitada. | Lanzamientos altos que superan `h_volea_max` |

### C.3 Capa 3: bandas de calibración (objetivos flexibles, §17.C)
Son los números de la v0.2, convertidos en **rangos de partida**. **No bloquean.** El PO los ajusta con presets según lo que vea en el iPhone.

| # | Escenario | Banda de partida |
|---|---|---|
| C1 | Drive paralelo desde el cuarto trasero, a media altura | 1.er bote detrás de la short line. 2.º bote cerca de la pared trasera o contra ella. Trayectoria a menos de ~0,5 m de la lateral en su último tramo. |
| C2 | Cruzado desde el cuarto trasero | Toca la lateral contraria después del 1.er bote, a la altura del jugador que recibe. 2.º bote en el cuarto trasero contrario. |
| C3 | Drop desde adelante (pelota cómoda) | Margen sobre el tin de ~5–15 cm. 2.º bote a menos de ~2 m del frontis. |
| C4 | Drop desde atrás (defensivo) | Margen mayor que en C3. 2.º bote a menos de ~3 m del frontis. |
| C5 | Drive con mala posición (estirado) | 1.er bote cerca de la short line o por delante ("suelta"). |
| C6 | Volea de drive desde la T (experimento) | Holgura del rival menor que con el mismo golpe tras el bote, en una magnitud que el PO juzga perceptible |
| C7 | Saque al cuadro contrario | Golpea el frontis entre la línea de saque y el out. 1.er bote en el cuarto trasero contrario. |

### C.4 Capa 4: juicio deportivo del PO
El PO juega o mira cada escenario en el iPhone y lo valora de 1 a 5 en "se siente squash". **Es la palabra final** [D25]. Si un escenario pasa las capas 1–3 y no se siente squash, se revisa igual. Si falla una banda pero se siente squash, la banda se corrige.

## Anexo D. Trazabilidad

### D.1 Hallazgos de Codex

| Hallazgo | Sección de la v0.3 |
|---|---|
| F01 | §0.1, §0.3 |
| F02 | §14, §15 |
| F03 | §7.1 |
| F04 | §6.2, §6.3 |
| F05 | §6.5 |
| F06 | §7.3.5 |
| F07 | §9.1, §9.3, §9.5 |
| F08 | §6.4, §12.1, §12.3 |
| F09 | §8.2 |
| F10 | §8.1, §8.3, §8.7 |
| F11 | §8.4 |
| F12 | §7.4 |
| F13 | §7.5 |
| F14 | §7.6.3 |
| F15 | §7.6.2, §7.6.5 |
| F16 | §9.4, §7.6.2 |
| F17 | §11.2 |
| F18 | §11.3, §11.4 |
| F19 | §10.1, §10.2, §10.5 |
| F20 | Anexo A |
| F21 | §13.2, §13.4 |
| F22 | §16.12, §17.A, §17.C, §17.E |
| F23 | §17, anexo C |
| F24 | §8.5, §8.7.4 |
| F25 | §5.4, §12.4, §17.C |
| F26 | §7.2, §10.3 |

### D.2 Revisión focalizada y directrices del PO (R3)

| Punto | Decisión del PO | Decisión del registro | Secciones de la v0.3 |
|---|---|---|---|
| FR1: timing | PO-1, PO-2, PO-3 | D19, D20, D21, D22 | §5.1, §6.5, §6.6, §7.1, §11.3, §12.4, §17.A.5–6, §17.B H3–H3c |
| FR2: volea | PO-4, PO-5 | D23, D24 | §4.10, §7.2, §7.3, §9.1, §9.3, §9.5, §11.2, §11.3, §17.A.7, §17.B H6–H6b |
| FR3: anexo C | PO-6 | D25 | Anexo C, §17.C, §17.F |
| FR4: error | PO-7, PO-8 | D26 | §4.1, §7.6, §10.2, §10.5, §17.A.12–13, §17.B H9–H9b |
| FR5: física | (sin decisión) | D27 | §8.2, §17.A.3 |
| FR6: líneas | PO-9, PO-10 | D28 | §8.1, §8.3, §8.5, §8.7, §12.4, §17.A.8, §17.B H11 |
| R3 §7: panel | — | D29 | §13.1, §13.2, §13.4 |
| R3 §8: ratificación | — | D01–D13 | §0.3 |

## Anexo E. Registro de cambios

### E.0 Enmienda tras la verificación WSF (2026-10-20), después de la aprobación de la v0.3

| Sección | Cambio | Fuente |
|---|---|---|
| §8.1 | Medidas verificadas. La short line ocupa entre 5,44 y 5,49 m. Tin a 0,48 m (P8 resuelto). | WSF Court Specifications 4.01, 5.01–5.05 |
| §8.7.2 | **Corrección:** las alturas se miden hasta el **borde inferior** de las líneas, no hasta el superior. Se agregan franjas concretas y se marcan las interpretaciones de Claude. | Cláusula 5.01.04 |
| §8.7.4 | Consecuencias confirmadas. Se agregan el fallo por tocar a la vez el frontis y una lateral en el saque, y el "down" por frontis y después tin. | Rules of Singles Squash 2025: 5.7, 5.8, 6.2, apéndice 1 |
| §8.7.5 | Se precisan las reglas reales del saque (5.7.1, 5.9) y del "further attempt". Nueva adaptación: el tin es plano. | Ídem |

**Para el PO:** ninguna decisión aprobada cambia. Solo cambia la posición exacta de las líneas, unos 5 cm en el out, la línea de saque y el out trasero.

### E.1 De la v0.2 a la v0.3

| Sección | Cambio | Origen |
|---|---|---|
| Encabezado, §0.1 | Versión 0.3. Nueva etiqueta [HIPÓTESIS·Dnn]. Referencias FR y PO-n. | R3 §9 |
| §0.3 | Fuente R3. Decisiones D19–D29. Nota de ratificación de las decisiones previas. | R3 §1–8 |
| §0.4 | Resumen ejecutivo de los cambios respecto de la v0.2 | R3 §9.7 |
| §4 | Principio 1 con dos capas. Principios nuevos 10 (D24) y 11 (categorías de fallo). | D24, D26 |
| §5.1 | Loop del rally con T3, un intento y oportunidades de volea | D19, D23 |
| §6.3 | Ayuda completa durante la comparación de controles. Métrica por categoría de contacto. | D19, D21 |
| §6.5 | Un intento por recepción. La cancelación no consume el intento (N1). Toque accidental (N2). Reglas renumeradas. | D19 |
| §6.6 | Saque con T3. Repetición sin falta documentada como adaptación arcade. | D19, D28 |
| §7.1 | **Reescrito:** T3, K, A, geometría del contacto, 14 casos, cuatro categorías de fallo, T3-suave como control, niveles de ayuda | D19–D22 |
| §7.2 | La fila de la volea remite a D23 | D23 |
| §7.3 | **Reescrito:** V2 + V3, V1 como control, oportunidades relevantes, separación mínima, modos M1/M2 (N4), W y volea, experimento | D23, D24 |
| §7.6 | **Reescrito:** E-C en dos capas, vectores causales, dificultad situacional frente a inherente, E8 nueva, **corrección de la contradicción** sobre qué filas se aplican antes de validarlas, variabilidad en tres brazos, visualización | D26 |
| §8.1, §8.3, §8.5 | Verificación WSF. Huella de ~10 mm (antes ~20 mm). Eventos de contacto. Remisión a §8.7. | D28 |
| §8.2 | **Reescrito:** se retira la "fidelidad garantizada". Condiciones 1–5, invariante bit a bit, S9, evaluación secuencial P1 → P2 → P3 sin tres motores. | D27 |
| §8.7 | **Nueva:** geometría de las líneas, huella, consecuencia por línea y por fase, adaptaciones arcade, marca de cantado | D28 |
| §9.1, §9.3, §9.4, §9.5 | Oportunidades por plan. Lo que el planificador no decide. W y volea. Remisión al caso 12. Palancas de volea y timing. | D23, D24 |
| §10.2, §10.5 | La técnica atenúa los vectores causales. La Consistencia entra como experimento, no como causa dominante. | D26 |
| §11.2, §11.3 | La IA no ve la intención de volear del humano. La IA elige la oportunidad y ejecuta con T3 y un solo intento. | D19, D23, D16 |
| §12.4 | Sonido del golpe al aire. Marca de cantado. | D20, D28 |
| §13 | Panel en el MVP desde First Ball, con presets locales desde First Ball. Controles por milestone para timing, volea, error y líneas. Exclusión de producción. | D29 |
| §14, §15 | Filas y salidas actualizadas para T3, volea, error y líneas | D19–D28 |
| §16 | 16.5, 16.7, 16.14 y 16.15 actualizados. Nuevos 16.16–16.18. | — |
| §17 | Invariantes nuevas (A.3, A.7, A.12, A.13) y ampliadas (A.5, A.6, A.8, A.9). Hipótesis H3, H3b, H3c, H6b, H9, H9b y H11. Nueva clase F. | D19–D28 |
| §18 | Resueltas P1, P6, P9, P10 y P11. Bloqueantes reales B1–B6. Aclaraciones menores N1–N7 con valor por defecto. | R3 §9.6 |
| Anexo B | Términos nuevos | — |
| Anexo C | **Reescrito en cuatro capas**, con condiciones de validez | D25 |
| Anexo D | Trazabilidad de FR1–FR6 y de R3 | R3 §9.2 |

**Decisiones de Claude en esta consolidación que el PO debería revisar** (no estaban explícitas en R3): N1, N2, N4 (M1 por defecto), N7 (el soltar tarde como fallo de timing), la evaluación secuencial P1 → P2 → P3 de §8.2.3, que la técnica no atenúe el vector de timing (§7.6.2) y la regla de desambiguación de §7.3.2, que muestra el bote cuando las dos oportunidades están demasiado cerca.

### E.2 De la v0.1 a la v0.2 (histórico)

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
